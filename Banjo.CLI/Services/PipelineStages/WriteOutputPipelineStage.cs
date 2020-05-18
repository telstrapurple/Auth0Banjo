using System.IO;
using System.Threading.Tasks;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Banjo.CLI.Services.PipelineStages
{
    public class WriteOutputPipelineStage : IPipelineStage<Auth0ResourceTemplate>
    {
        public string Name { get; } = "Write Output";

        private readonly IReporter _reporter;
        private readonly IOptionsMonitor<Auth0ProcessArgsConfig> _args;

        public WriteOutputPipelineStage(IReporter reporter, IOptionsMonitor<Auth0ProcessArgsConfig> args)
        {
            _reporter = reporter;
            _args = args;
        }

        public virtual async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            var serialised = JsonConvert.SerializeObject(t.Template, Formatting.Indented);
            
            if (!string.IsNullOrEmpty(_args.CurrentValue.OutputPath))
            {
                await WriteOutput(t, serialised);
            }
            
            return t;
        }
        
        private async Task WriteOutput(Auth0ResourceTemplate t, string serialisedContent)
        {
            var dir = Directory.CreateDirectory(_args.CurrentValue.OutputPath);
            var fullPath = dir.FullName;
            
            var templateDir = Directory.CreateDirectory(Path.Join(fullPath, t.Type.DirectoryName));

            var outputPath = Path.Join(templateDir.FullName, t.Filename);
            _reporter.Output($"Writing effective template result for {t.Type.Name} template {t.Filename}:");
            _reporter.Output($"\t{outputPath}");
            await File.WriteAllTextAsync(outputPath , serialisedContent);
        }
    }
}