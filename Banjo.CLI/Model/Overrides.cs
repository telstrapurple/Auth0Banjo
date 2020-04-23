using System.Collections.Generic;
using Newtonsoft.Json;

namespace Banjo.CLI.Model
{
    public class Overrides
    {
        [JsonProperty("clients")]
        public IEnumerable<TemplateOverride> Clients { get; set; }

        [JsonProperty("resource-servers")]
        public IEnumerable<TemplateOverride> ResourceServers { get; set; }

        [JsonProperty("grants")]
        public IEnumerable<TemplateOverride> Grants { get; set; }

        [JsonProperty("tenant-settings")]
        public IEnumerable<TemplateOverride> TenantSettings { get; set; }
    }

    public class TemplateOverride
    {
        [JsonProperty("name")]
        public string TemplateName { get; set; }

        [JsonProperty("jsonpath")]
        public string Path { get; set; }

        [JsonProperty("replacement")]
        public dynamic Replacement { get; set; }
    }
}