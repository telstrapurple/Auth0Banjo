using System.IO;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Banjo.CLI.Services;
using DeepEqual.Syntax;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Banjo.CLI.Tests
{
    public class ReflectorisorTests
    {
        [Fact]
        public async Task TestClient()
        {
            var from = JsonConvert.DeserializeObject<Client>(await File.ReadAllTextAsync("./SampleData/Templates/clients/tones-localhost.template.json"));
            
            var to = Reflectorisor.CopyMembers<Client, ClientCreateRequest>(from);

            to.AddOns?.ShouldBe(from.AddOns);
            to.AllowedClients.ShouldBe(from.AllowedClients);
            to.AllowedLogoutUrls.ShouldBe(from.AllowedLogoutUrls);
            to.AllowedOrigins.ShouldBe(from.AllowedOrigins);
            //Application is declared with the same sig on both typeof(to) and typeof(from) so should be copied.
            to.ApplicationType.ShouldBe(from.ApplicationType);
            to.Callbacks.ShouldBe(from.Callbacks);
            to.ClientAliases.ShouldBe(from.ClientAliases);
            to.ClientMetaData?.ShouldBe(from.ClientMetaData);
            to.ClientSecret.ShouldBe(from.ClientSecret);
            to.CustomLoginPage.ShouldBe(from.CustomLoginPage);
            to.CustomLoginPagePreview.ShouldBe(from.CustomLoginPagePreview);
            to.Description.ShouldBe(from.Description);
            to.EncryptionKey.ShouldBe(from.EncryptionKey);
            to.FormTemplate.ShouldBe(from.FormTemplate);
            to.InitiateLoginUri.ShouldBe(from.InitiateLoginUri);
            to.IsCustomLoginPageOn.ShouldBe(from.IsCustomLoginPageOn);
            to.IsFirstParty.ShouldBe(from.IsFirstParty);
            // to.IsHerokuApp.ShouldBe(from...); //IsHerokuApp doesn't exist on CreateClientRequest
            to.JwtConfiguration.ShouldDeepEqual(from.JwtConfiguration);
            to.LogoUri.ShouldBe(from.LogoUri);
            to.Mobile.ShouldBe(from.Mobile);
            to.Name.ShouldBe(from.Name);
            to.OidcConformant.ShouldBe(from.OidcConformant);
            to.ResourceServers.ShouldBe(from.ResourceServers);
            to.Sso.ShouldBe(from.Sso);
            //TokenEndpointAuthMethod is declared with the same sig on both typeof(to) and typeof(from) so should be copied.
            to.TokenEndpointAuthMethod.ShouldBe(from.TokenEndpointAuthMethod);
            to.WebOrigins.ShouldBe(from.WebOrigins);
        }
    }
}