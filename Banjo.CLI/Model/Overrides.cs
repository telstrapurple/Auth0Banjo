using System.Collections.Generic;
using Auth0.ManagementApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Banjo.CLI.Model
{
    public class Overrides
    {
        [JsonProperty("replacements")]
        public Dictionary<string, string> Replacements { get; set; }

        [JsonProperty("clients")]
        public IEnumerable<TemplateOverride> Clients { get; set; }

        [JsonProperty("resource-servers")]
        public IEnumerable<TemplateOverride> ResourceServers { get; set; }

        [JsonProperty("grants")]
        public IEnumerable<TemplateOverride> Grants { get; set; }

        [JsonProperty("tenant-settings")]
        public IEnumerable<TemplateOverride> TenantSettings { get; set; }

        [JsonProperty("roles")]
        public IEnumerable<TemplateOverride> Roles { get; set; }
        
        [JsonProperty("pages")]
        public IEnumerable<TemplateOverride> Pages { get; set; }
        
        [JsonProperty("rules")]
        public IEnumerable<TemplateOverride> Rules { get; set; }
        
        
    }

    public class TemplateOverride
    {
        [JsonProperty("name")]
        public string TemplateName { get; set; }

        [JsonProperty("overrides")]
        public IEnumerable<OverrideDefinition> Overrides { get; set; }
    }

    public class OverrideDefinition
    {
        [JsonProperty("jsonpath")]
        public string Path { get; set; }

        [JsonProperty("replacement")]
        public JToken Replacement { get; set; }
    }
}