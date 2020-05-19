using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Banjo.CLI.Tests
{
    public class SchemaTestsFixture : IDisposable
    {
        public JSchema OverridesSchema { get; }

        public SchemaTestsFixture()
        {
            OverridesSchema = JSchema.Parse(GetOverridesSchemaDoc());
        }

        public void Dispose()
        {
            //no-op, nothing to dispose
        }

        private static string GetOverridesSchemaDoc()
        {
            var assembly = Assembly.GetAssembly(typeof(Program));
            var resourceName = "Banjo.CLI.overrides.schema.resources";
            using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            using var resourceReader = new ResourceReader(resourceStream);
            var schemaDoc = GetResource(resourceReader, "overrides.schema.json") as string;
            return schemaDoc;
        }
        
        private static object GetResource(ResourceReader reader, string resourceName)
        {
            var enumerator = reader.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (string.Equals((string) enumerator.Key, resourceName, StringComparison.OrdinalIgnoreCase))
                {
                    return enumerator.Value;
                }
            }

            return null;
        }
    }
    
    public class SchemaTests : IClassFixture<SchemaTestsFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly SchemaTestsFixture _fixture;

        public SchemaTests(ITestOutputHelper output, SchemaTestsFixture fixture)
        {
            _output = output;
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(GoodSchemaPaths))]
        public async Task GoodSchemasShouldValidateCorrectly(string path)
        {
            var (o, messages) = await DeserialiseOverrides(path);
            o.ShouldNotBeNull();
            messages.ShouldBeEmpty();
        }
        
        [Theory]
        [MemberData(nameof(BadSchemaPaths))]
        public async Task BadSchemasShouldNotValidateCorrectly(string path)
        {
            var (o, errors) = await DeserialiseOverrides(path); 
            o.ShouldNotBeNull(); //should deserialise to JObject
            errors.ShouldNotBeEmpty(); //but should have validation errors
        }
        
        [Theory]
        [MemberData(nameof(MalformedSchemaPaths))]
        public async Task MalformedSchemasShouldThrowException(string path)
        {
            Should.Throw<JsonException>(async () => await DeserialiseOverrides(path));
        }
        
        private async Task<(JObject, IList<string>)> DeserialiseOverrides(string path)
        {
            _output.WriteLine("Testing path {0}", path);
            _output.WriteLine("Resolves to {0}", Path.GetFullPath(path));

            var sample = await File.ReadAllTextAsync(path);
            JsonTextReader reader = new JsonTextReader(new StringReader(sample));

            JSchemaValidatingReader validatingReader = new JSchemaValidatingReader(reader) { Schema = _fixture.OverridesSchema };

            IList<string> messages = new List<string>();
            validatingReader.ValidationEventHandler += (sender, eventArgs) => messages.Add(eventArgs.Message);

            JsonSerializer serializer = new JsonSerializer();
            JObject o = serializer.Deserialize<JObject>(validatingReader);
            
            return (o, messages);
        }

        public static TheoryData<string> GoodSchemaPaths()
        {
            return EnumerateFilesAsTheoryData("./SchemaTestSamples/GoodSamples");
        }
        
        public static TheoryData<string> BadSchemaPaths()
        {
            return EnumerateFilesAsTheoryData("./SchemaTestSamples/BadSamples");
        }
        
        public static TheoryData<string> MalformedSchemaPaths()
        {
            return EnumerateFilesAsTheoryData("./SchemaTestSamples/MalformedSamples");
        }
        
        private static TheoryData<string> EnumerateFilesAsTheoryData(string basePath)
        {
            Directory.Exists(Path.GetFullPath(basePath)).ShouldBeTrue();
            var goodSchemaPaths = Directory.EnumerateFiles(basePath, "*.json")
                .ToList();
            //let's just validate that we've picked up all the sample files we're looking for.
            goodSchemaPaths.ShouldNotBeEmpty();
            
            var data = new TheoryData<string>();
            foreach (var path in goodSchemaPaths)
            {
                data.Add(path);
            }
            return data;
        }
    }
}