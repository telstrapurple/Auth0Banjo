using System;
using System.IO;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Newtonsoft.Json;

namespace Banjo.CLI.Services
{
    [Obsolete]
    public class TemplateReaderOld<T> : ITemplateReaderOld<T>
    {
        public async Task<T> ReadTemplateContents(Auth0ResourceTemplate templateMetadata)
        {
            return JsonConvert.DeserializeObject<T>(await File.ReadAllTextAsync(templateMetadata.Location.FullName));
        }
    }
}