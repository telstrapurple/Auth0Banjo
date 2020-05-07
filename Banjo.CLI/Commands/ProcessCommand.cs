using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Banjo.CLI.Services;
using Banjo.CLI.Services.Processors;
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

        private readonly ITemplateSource _templateSource;
        private readonly ClientsProcessor _clientsProcessor;
        private readonly ProcessorFactory _processorFactory;
        private readonly ArgumentConfigurator _configurator;

        public ProcessCommand(
            ITemplateSource templateSource,
            ClientsProcessor clientsProcessor,
            ProcessorFactory processorFactory, ArgumentConfigurator configurator)
        {
            _templateSource = templateSource;
            _clientsProcessor = clientsProcessor;
            _processorFactory = processorFactory;
            _configurator = configurator;
        }

        public override async Task OnExecuteAsync(CommandLineApplication app)
        {
            Console.WriteLine($"{nameof(TemplatesPath)} = {TemplatesPath}");
            Console.WriteLine($"{nameof(OverridePath)} = {OverridePath}");
            Console.WriteLine($"{nameof(ProcessedOutputPath)} = {ProcessedOutputPath}");
            Console.WriteLine($"{nameof(DryRun)} = {DryRun}");

            _configurator.AddConfiguration(new Auth0ProcessArgsConfig
            {
                DryRun = DryRun,
                OutputPath = ProcessedOutputPath,
                OverrideFilePath = OverridePath,
                TemplateInputPath = TemplatesPath
            });

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


            foreach (var templateType in ResourceType.SupportedResourceTypes)
            {
                var templates = _templateSource.GetTemplates(TemplatesPath, templateType);

                var pipeline = new List<IProcessor<Auth0ResourceTemplate>>
                {
                    _processorFactory.CreateTemplateReader(),
                    _processorFactory.CreateOverridesProcessor(),
                    _processorFactory.CreateOutputProcessor(),
                    _processorFactory.CreateApiExecutor()
                };

                await new PipelineExecutor().ExecuteAsync(pipeline, templates);
            }
        }
    }
}