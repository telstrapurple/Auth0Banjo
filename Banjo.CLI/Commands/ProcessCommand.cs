using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.Commands
{
    [Command(Name = "process")]
    public class ProcessCommand : BanjoCommandBase
    {
        [Required]
        [DirectoryExists]
        [Option(
            CommandOptionType.SingleValue,
            LongName = "template",
            ShortName = "t",
            Description = "The path to a directory of input templates")]
        public string TemplatesPath { get; set; }

        [FileExists]
        [Option(
            CommandOptionType.SingleValue,
            LongName = "override",
            ShortName = "o",
            Description = "The path to an override file")]
        public string OverridePath { get; set; }

        [Option(
            CommandOptionType.SingleValue,
            LongName = "output",
            ShortName = "out",
            Description = "The output path for processed template files")]
        public string ProcessedOutputPath { get; set; } = "./output";

        [Option(
            CommandOptionType.NoValue,
            LongName = "dry-run",
            ShortName = "d",
            Description = "Process the templates, but don't execute the API calls")]
        public bool DryRun { get; set; } = false;

        private readonly IOverridesSource _overridesSource;
        private readonly ITemplateSource _templateSource;
        private readonly ClientsProcessor _clientsProcessor;

        public ProcessCommand(IOverridesSource overridesSource, ITemplateSource templateSource, ClientsProcessor clientsProcessor)
        {
            _overridesSource = overridesSource;
            _templateSource = templateSource;
            _clientsProcessor = clientsProcessor;
        }

        public override async Task OnExecuteAsync(CommandLineApplication app)
        {
            Console.WriteLine($"{nameof(TemplatesPath)} = {TemplatesPath}");
            Console.WriteLine($"{nameof(OverridePath)} = {OverridePath}");
            Console.WriteLine($"{nameof(ProcessedOutputPath)} = {ProcessedOutputPath}");
            Console.WriteLine($"{nameof(DryRun)} = {DryRun}");
            
            //read the overrides file
            //read the token replacements map (file or args)
            //foreach template type
            //  read the templates
            //  foreach template
            //    apply the overrides
            //    apply the token replacements
            //  if dryrun, write out the files
            //  validate resulting template by parsing into Auth0 model classes
            //  if !dryrun, do api calls

            var overrides = _overridesSource.GetOverrides(OverridePath);
            
            var supportedTemplateTypes = new[] {ResourceType.Clients};
            foreach (var templateType in supportedTemplateTypes)
            {
                var templates = _templateSource.GetTemplates(TemplatesPath, templateType);
                foreach (var templateMetadata in templates)
                {
                    Console.WriteLine($"{templateMetadata.Type.Name} {templateMetadata.Location.FullName}");
                    
                    //skip the overrides for now, let's just get the basics working
                    // - read the template AS A WHOLE PAYLOAD
                    // - make the api calls
                    //done
                    
                    await _clientsProcessor.ProcessAsync(templateMetadata);
                }
            }
        }
    }
}