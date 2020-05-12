using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json.Linq;

namespace Banjo.CLI.Services.PipelineStages
{
    public class UnresolvedTokenVerifierStage : IPipelineStage<Auth0ResourceTemplate>
    {
        private readonly IReporter _reporter;

        public UnresolvedTokenVerifierStage(IReporter reporter)
        {
            _reporter = reporter;
        }

        public string Name { get; } = "Check unresolved tokens";

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            var allStrings = t.Template.SelectTokens("$..*").OfType<JValue>().Where(x => x.Type == JTokenType.String);

            // var regex = new Regex("(%%[a-zA-Z-_]+%%)+.*(?>(%%[a-zA-Z-_]+%%)+.*)");
            var regex = new Regex("%%[a-zA-Z-_]+%%");

            foreach (var sToken in allStrings)
            {
                var match = regex.Matches(sToken.Value as string ?? "").SelectMany(x => x.Groups.Values).ToList();
                if (match.Any())
                {
                    _reporter.Error(
                        $"{t.Type.Name} template {t.Filename} contains unresolved tokens, you should ensure " +
                        $"you define every token to be replaced in the overrides file.");
                    _reporter.Error($"[{string.Join(", ", match.Select(x => x.Value))}]");
                }
            }

            return t;
        }
    }
}