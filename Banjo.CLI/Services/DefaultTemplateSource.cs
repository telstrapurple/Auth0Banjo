using System.Collections.Generic;
using System.IO;
using System.Linq;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.Services
{
    public class DefaultTemplateSource : ITemplateSource
    {
        private readonly IReporter _reporter;

        public DefaultTemplateSource(IReporter reporter)
        {
            _reporter = reporter;
        }

        public IEnumerable<Auth0ResourceTemplate> GetTemplates(string templatesPath, ResourceType templateType)
        {
            //todo handle no such resource type, ie, search path does not exist.
            var searchPath = Path.Combine(templatesPath, templateType.DirectoryName);
            if (!Directory.Exists(searchPath))
            {
                _reporter.Verbose($"No template directory exists for templates for resource type [{templateType.Name}]");
                return new List<Auth0ResourceTemplate>();
            }

            var foundTemplates = Directory.EnumerateFiles(searchPath, templateType.FilenamePattern)
                .Select(x => new Auth0ResourceTemplate()
                {
                    Location = new FileInfo(x),
                    Type = templateType
                }).ToList();

            _reporter.Output($"Found {foundTemplates.Count} {templateType.Name} templates.");
            foreach (var t in foundTemplates)
            {
                _reporter.Verbose($"\t{t.Location.FullName}");
            }

            return foundTemplates;
        }
    }
}