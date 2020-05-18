using System.Collections.Generic;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services;

namespace Banjo.CLI.TestSupport
{
    public class TestTemplateSource : ITemplateSource
    {
        public Dictionary<ResourceType, List<Auth0ResourceTemplate>> Templates { get; set; }
        
        public IEnumerable<Auth0ResourceTemplate> GetTemplates(string templatesPath, ResourceType templateType)
        {
            if (Templates.TryGetValue(templateType, out var result))
            {
                return result;
            }
            return new List<Auth0ResourceTemplate>();
        }
    }
}