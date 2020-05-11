using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Banjo.CLI.Model.Templates
{
    public class BanjoPage
    {
        [JsonProperty("html")]
        public string HtmlPath { get; set; }

        public bool Enabled { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("name")]
        public PageType PageName { get; set; }
    }

    public enum PageType
    {
        Login,
        PasswordReset
    }
}