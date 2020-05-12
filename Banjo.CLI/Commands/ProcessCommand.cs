using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Banjo.CLI.Services;
using Banjo.CLI.Services.PipelineStages;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            LongName = "verbose",
            ShortName = "v",
            Description = "Enable Verbose level output")]
        public bool Verbose { get; set; } = false;

        public ProcessCommand()
        {
        }

        public override async Task OnExecuteAsync(CommandLineApplication app)
        {
            ArgumentConfigurator configurator = app.GetRequiredService<ArgumentConfigurator>();
            configurator.AddConfiguration(new Auth0ProcessArgsConfig
            {
                DryRun = DryRun,
                OutputPath = ProcessedOutputPath,
                OverrideFilePath = OverridePath,
                TemplateInputPath = TemplatesPath,
                Verbose = Verbose
            });

            //resolve the things we need _after_ we've set the parsed configuration using the ArgumentConfigurator.
            //Depending on how the instances are constructed, if constructed in time to be injected into this classes
            //constructor then they might have be built with stale configuration. (ConsoleReporter I'm looking at you)
            //
            //Generally, Banjo-specific classes can avoid working with stale config by accepting an
            //IOptionMonitor<Auth0ProcessArgsConfig>
            ITemplateSource templateSource = app.GetRequiredService<ITemplateSource>();
            PipelineStageFactory pipelineStageFactory = app.GetRequiredService<PipelineStageFactory>();

            var reporter = app.GetRequiredService<IReporter>();

            reporter.Verbose($"{nameof(TemplatesPath)} = {TemplatesPath}");
            reporter.Verbose($"{nameof(OverridePath)} = {OverridePath}");
            reporter.Verbose($"{nameof(ProcessedOutputPath)} = {ProcessedOutputPath}");
            reporter.Verbose($"{nameof(DryRun)} = {DryRun}");

            if (DryRun && string.IsNullOrEmpty(ProcessedOutputPath))
            {
                reporter.Warn(
                    "Banjo will dry-run the result of processing the templates, but will not write the " +
                    "result of the processed template, which will make it hard to debug any issues if the " +
                    "templates are found to be not valid.");
                reporter.Warn("Recommend also setting -out|--output {output-path} to have Banjo " +
                              "write the effective templates.");
            }
            
            var auth0Args = app.GetRequiredService<IOptionsMonitor<Auth0AuthenticationConfig>>().CurrentValue;
            reporter.Output($"Current Auth0 domain: {auth0Args.Domain.ToSafeForOutput()}");
            reporter.Verbose($"Connecting with client ID: {auth0Args.ClientId.ToSafeForOutput()}");
            reporter.Verbose($"Connecting with client secret: {auth0Args.ClientSecret.ToSafeForOutput(true)}");

            var pipelineStages = new List<IPipelineStage<Auth0ResourceTemplate>>
            {
                pipelineStageFactory.CreateTemplateReader(),
                await pipelineStageFactory.CreateOverridesProcessor(),
                pipelineStageFactory.CreateTokenReplacementStage(),
                pipelineStageFactory.CreateResourcePreprocessingStage(),
                pipelineStageFactory.CreateOutputProcessor(),
                pipelineStageFactory.CreateVerifier(),
                pipelineStageFactory.CreateUnresolvedTokenVerifier(),
                pipelineStageFactory.CreateApiExecutor()
            };

            var pipeline = new PipelineExecutor(pipelineStages, reporter);

            var templatesToProcess = ResourceType.SupportedResourceTypes
                .SelectMany(x => templateSource.GetTemplates(TemplatesPath, x))
                .ToList();

            await pipeline.ExecuteAsync(templatesToProcess);
        }
    }

    internal static class SecretStringExtensions
    {
        internal static string ToSafeForOutput(this string input, bool maskValue = false)
        {
            return input switch
            {
                null => "<null>",
                { } s when s.Length == 0 => "<empty>",
                _ => maskValue ? "****" : input
            };
        }
    }
}