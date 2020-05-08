using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Newtonsoft.Json.Linq;

namespace Banjo.CLI.Services.PipelineStages
{
    public class StringReplacementStage : IPipelineStage<Auth0ResourceTemplate>
    {
        public StringReplacementStage()
        {
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            if ((t.Overrides?.Replacements?.Count ?? 0) == 0)
            {
                //no overrides or no replacements defined in it, so bail out.
                return t;
            }

            var tokens = t.Template.SelectTokens("$..*").OfType<JValue>().Where(x => x.Type == JTokenType.String);

            foreach (var token in tokens)
            {
                if (!(token.Value is string v) || string.IsNullOrWhiteSpace(v)) continue;
                
                foreach (var (key, value) in t.Overrides.Replacements)
                {
                    token.Value = (token.Value as string)?.Replace(key, value);
                }
            }

            return t;
        }
    }
}