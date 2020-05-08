using System;
using System.Collections.Generic;

namespace Banjo.CLI.Model
{
    public class ResourceType
    {
        public const string DefaultTemplateFilePattern = "*.template.json";

        public static readonly ResourceType Clients = new ResourceType("Client", "clients", x => x.Clients);
        public static readonly ResourceType ResourceServers = new ResourceType("Resource Server", "resource-servers", x => x.ResourceServers);
        public static readonly ResourceType ClientGrants = new ResourceType("Client Grant", "grants", x => x.Grants);

        public static readonly IReadOnlyList<ResourceType> SupportedResourceTypes = new[]
        {
            //order is important.
            //For instance, the client and resource-server resource must exist for the client-grant to be created to link them.
            Clients, 
            ResourceServers,
            ClientGrants, 
        };

        public string Name { get; }
        public string DirectoryName { get; }
        public string FilenamePattern { get; }
        public Func<Overrides, IEnumerable<TemplateOverride>> OverridesAccessor { get; }

        private ResourceType(
            string name,
            string directoryName,
            Func<Overrides, IEnumerable<TemplateOverride>> overridesAccessor,
            string filenamePattern = DefaultTemplateFilePattern)
        {
            Name = name;
            DirectoryName = directoryName;
            FilenamePattern = filenamePattern;
            OverridesAccessor = overridesAccessor;
        }
    }
}