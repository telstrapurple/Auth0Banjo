using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Banjo.CLI.Model
{
    public class Overrides
    {
        [JsonProperty("replacements")]
        public IEnumerable<ReplacementDefinition> Replacements { get; set; }

        [JsonIgnore]
        public IDictionary<string, string> VerbatimReplacements
        {
            get
            {
                return (IDictionary<string, string>) Replacements
                           ?.Where(x => !string.IsNullOrEmpty(x.Value))
                           .ToImmutableDictionary(k => k.Token, v => v.Value)
                       ?? new Dictionary<string, string>();
            }
        }

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

        [JsonProperty("database-connections")]
        public IEnumerable<TemplateOverride> DatabaseConnections { get; set; }
    }

    public class ReplacementDefinition
    {
        [Required]
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("environment-variable")]
        public string EnvironmentVariable { get; set; }

        private sealed class TokenEqualityComparer : IEqualityComparer<ReplacementDefinition>
        {
            public bool Equals(ReplacementDefinition x, ReplacementDefinition y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Token == y.Token;
            }

            public int GetHashCode(ReplacementDefinition obj)
            {
                return (obj.Token != null ? obj.Token.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<ReplacementDefinition> TokenComparer { get; } = new TokenEqualityComparer();
    }

    public class TemplateOverride
    {
        [Required]
        [JsonProperty("name")]
        public string TemplateName { get; set; }

        [JsonProperty("overrides")]
        public IEnumerable<OverrideDefinition> Overrides { get; set; }
    }

    public class OverrideDefinition
    {
        [Required]
        [JsonProperty("jsonpath")]
        public string Path { get; set; }

        [Required]
        [JsonProperty("replacement")]
        public JToken Replacement { get; set; }
    }
}