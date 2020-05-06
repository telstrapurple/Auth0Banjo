using System.Collections.Generic;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services
{
    public interface ITemplateSource
    {
        IEnumerable<Auth0ResourceTemplate> GetTemplates(string templatesPath, ResourceType templateType);
    }
}