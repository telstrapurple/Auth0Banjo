using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Banjo.CLI.Services;
using Banjo.CLI.Services.PipelineStages;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

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
            Description = "The output path for writing the effective templates")]
        public string ProcessedOutputPath { get; set; }

        [Option(
            CommandOptionType.NoValue,
            LongName = "dry-run",
            ShortName = "d",
            Description = "Process the templates, plan the mutation operations to make (which may include some Auth0 API calls), but do not create/update any Auth0 resources.")]
        public bool DryRun { get; set; } = false;

        [Option(
            CommandOptionType.NoValue,
            LongName = "verify",
            ShortName = "y",
            Description = "Verify the resulting effective templates, but do not make any Auth0 API calls. Should be used in conjunction with -out|--output {output-path}")]
        public bool Verify { get; set; } = false;

        private readonly ITemplateSource _templateSource;
        private readonly PipelineStageFactory _pipelineStageFactory;
        private readonly ArgumentConfigurator _configurator;
        private readonly ILogger<ProcessCommand> _logger;

        public ProcessCommand(
            ITemplateSource templateSource,
            PipelineStageFactory pipelineStageFactory,
            ArgumentConfigurator configurator,
            ILogger<ProcessCommand> logger)
        {
            _templateSource = templateSource;
            _pipelineStageFactory = pipelineStageFactory;
            _configurator = configurator;
            _logger = logger;
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

            if (Verify && string.IsNullOrEmpty(ProcessedOutputPath))
            {
                _logger.LogWarning(
                    "Banjo will verify the result of processing the templates, but will not write the " +
                    "result of the processed template, which will make it hard to debug any issues if the " +
                    "templates are found to be not valid.");
                _logger.LogWarning("Recommend also setting -out|--output {output-path} to have Banjo " +
                                   "write the effective templates.");
            }

            var pipelineStages = new List<IPipelineStage<Auth0ResourceTemplate>>
            {
                _pipelineStageFactory.CreateTemplateReader(),
                _pipelineStageFactory.CreateOverridesProcessor(),
                _pipelineStageFactory.CreateOutputProcessor(),
                _pipelineStageFactory.CreateVerifier()
            };

            if (Verify == false)
            {
                //if Verify == true, then user has explicitly passed '-y', which means no API calls.
                //But in this block, it's false, so we need to do the api calls.
                pipelineStages.Add(_pipelineStageFactory.CreateApiExecutor());
            }

            var pipeline = new PipelineExecutor(pipelineStages);

            var templatesToProcess = ResourceType.SupportedResourceTypes
                .SelectMany(x => _templateSource.GetTemplates(TemplatesPath, x))
                .ToList();

            await pipeline.ExecuteAsync(templatesToProcess);
        }
    }
}