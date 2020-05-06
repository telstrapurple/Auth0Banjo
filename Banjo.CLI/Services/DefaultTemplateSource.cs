using System.Collections.Generic;
using System.IO;
using System.Linq;
using Banjo.CLI.Model;
using Microsoft.Extensions.Logging;

namespace Banjo.CLI.Services
{
    public class DefaultTemplateSource : ITemplateSource
    {
        private readonly ILogger<ITemplateSource> _logger;

        public DefaultTemplateSource(ILogger<ITemplateSource> logger)
        {
            _logger = logger;
        }

        public IEnumerable<Auth0ResourceTemplate> GetTemplates(string templatesPath, ResourceType templateType)
        {
            //todo handle no such resource type, ie, search path does not exist.
            var searchPath = Path.Combine(templatesPath, templateType.DirectoryName);
            if (!Directory.Exists(searchPath))
            {
                _logger.LogDebug($"No template directory exists for templates for resource type [{templateType.Name}]");
                return new List<Auth0ResourceTemplate>();
            }

            return Directory.EnumerateFiles(searchPath, templateType.FilenamePattern)
                .Select(x => new Auth0ResourceTemplate()
                {
                    Location = new FileInfo(x),
                    Type = templateType
                });
        }
    }
}