using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Banjo.CLI.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Banjo.CLI.Tests
{
    public class DeserialisationTests
    {
        
        private readonly ITestOutputHelper output;

        public DeserialisationTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task TestAThing()
        {
            var input = await File.ReadAllTextAsync("./SampleData/Templates/clients/tones-localhost.template.json");
            JsonSerializer s = new JsonSerializer();
            var j = s.Deserialize<JObject>(new JsonTextReader(new StringReader(input)));
            
            // var c1 = s.Deserialize<Client>(new JTokenReader(j));
            // var c2 = j.ToObject<Client>();

            var alg = j.SelectToken("jwt_configuration.alg");
            var grant_types = j.SelectToken("grant_types");
            alg.Replace(grant_types);
            
            output.WriteLine(JsonConvert.SerializeObject(j, Formatting.Indented));
        }
        
        [Fact]
        public async Task TestReplacement()
        {
            var input = await File.ReadAllTextAsync("./SampleData/Templates/clients/tones-localhost.template.json");
            var overridesContent = await File.ReadAllTextAsync("./SampleData/Overrides/tones.overrides.json");
            
            JsonSerializer s = new JsonSerializer();
            var j = s.Deserialize<JObject>(new JsonTextReader(new StringReader(input)));
            var overrides = s.Deserialize<Overrides>(new JsonTextReader(new StringReader(overridesContent)));

            var clientOverride = overrides.Clients.FirstOrDefault();
            foreach (var o in clientOverride.Overrides)
            {
                var token = j.SelectToken(o.Path, true);
                token.Replace(o.Replacement);
            }
            
            output.WriteLine(JsonConvert.SerializeObject(j, Formatting.Indented));
        }
    }
}