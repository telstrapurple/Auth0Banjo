using System.IO;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Banjo.CLI.Services.PipelineStages
{
    public class TemplateReaderPipelineStage : IPipelineStage<Auth0ResourceTemplate>
    {
        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            var templateFileContents = await File.ReadAllTextAsync(t.Location.FullName);
            var templateContents = JsonConvert.DeserializeObject<JObject>(templateFileContents);
            t.Template = templateContents;
            return t;
        }
    }
}