using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Banjo.CLI.Model.Templates
{
    public class BanjoPage
    {
        [JsonProperty("html")]
        public string Html { get; set; }

        public bool Enabled { get; set; }

        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
        [JsonProperty("name")]
        public PageType PageType { get; set; } //pasword_reset, error, login, guardian_multifactor
    }

    public enum PageType
    {
        Login,
        PasswordReset,
        //Error
        GuardianMultifactor
    }
}