using System;
using System.Collections.Generic;

namespace Banjo.CLI.Model
{
    public class ResourceType
    {
        public const string DefaultTemplateFilePattern = "*.template.json";
        
        public static readonly ResourceType Clients = new ResourceType("clients", x => x.Clients);
        public static readonly ResourceType ResourceServers = new ResourceType("resource-servers", x => x.ResourceServers);

        public string Name {get;}
        public string DirectoryName { get; }
        public string FilenamePattern { get; }
        public Func<Overrides, IEnumerable<TemplateOverride>> OverridesAccessor { get; }

        private ResourceType(
            string name,
            string directoryName, 
            string filenamePattern, 
            Func<Overrides, IEnumerable<TemplateOverride>> overridesAccessor)
        {
            Name = name;
            DirectoryName = directoryName;
            FilenamePattern = filenamePattern;
            OverridesAccessor = overridesAccessor;
        }

        private ResourceType(string directoryName, Func<Overrides, IEnumerable<TemplateOverride>> overridesAccessor) 
            : this(directoryName, directoryName, DefaultTemplateFilePattern, overridesAccessor)
        {
        }
    }
}