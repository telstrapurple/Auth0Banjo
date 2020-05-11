using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Banjo.CLI.Model.Templates;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Banjo.CLI.Services.ResourceTypeProcessors
{
    //Pages are handled differently to most other resource types. Each page type has its own way of being set/updated,
    //but the a0deploy CLI exports/imports them as a single virtual resource type "pages". The A0 management client
    //has no concept of the kind of unified 'pages' api that a0deploy offers.
    //This is why PagesResourceTypeProcessor declares it handles a custom model class <BanjoPage>, and not any type
    //from the A0 management client model.
    public class PagesProcessor : AbstractSingleTypeResourceTypeProcessor<BanjoPage>
    {
        protected override ResourceType Type { get; } = ResourceType.Pages;

        private readonly ManagementApiClientFactory _managementApiClientFactory;
        private readonly IConverter<Auth0ResourceTemplate, BanjoPage> _converter;

        public PagesProcessor(
            IOptionsMonitor<Auth0ProcessArgsConfig> args,
            IConverter<Auth0ResourceTemplate, BanjoPage> converter,
            IReporter reporter,
            ManagementApiClientFactory managementApiClientFactory)
            : base(args, converter, reporter)
        {
            _managementApiClientFactory = managementApiClientFactory;
            _converter = converter;
        }

        public override async Task Preprocess(Auth0ResourceTemplate template)
        {
            var htmlToken = template.Template.SelectTokens("html").OfType<JValue>().FirstOrDefault(x => x.Type == JTokenType.String);
            if (htmlToken == null)
            {
                throw new Auth0InvalidTemplateException(
                    $"{Type.Name} template {template.Filename} does not contain a required property \'html\' " +
                    $"of type String.");
            }

            var htmlFilename = htmlToken.Value as string;
            if (string.IsNullOrWhiteSpace(htmlFilename))
            {
                throw new Auth0InvalidTemplateException(
                    $"{Type.Name} template {template.Filename} does not contain a valid value for property " +
                    $"\'html\'. Must not be null or empty.");
            }

            var htmlPath = Path.Combine(Directory.GetParent(template.Location.FullName).FullName, htmlFilename);
            if (!File.Exists(htmlPath))
            {
                throw new Auth0InvalidTemplateException(
                    $"{Type.Name} template {template.Filename} does not contain a valid value for property " +
                    $"\'html\'. The file must exist but does not: {htmlPath}");
            }

            htmlToken.Replace(await File.ReadAllTextAsync(htmlPath));
        }

        public override async Task ProcessAsync(Auth0ResourceTemplate template)
        {
            var page = _converter.Convert(template);
            if (!template.Preprocessed)
            {
                throw new Auth0InvalidTemplateException(
                    $"{template.Type.Name} template {template.Filename} has not been preprocessed prior to being executed.");
            }

            switch (page.PageType)
            {
                case PageType.Login:
                    await SetLoginPage(page);
                    break;
                case PageType.PasswordReset:
                    await SetPasswordResetPage(page);
                    break;
                case PageType.GuardianMultifactor:
                    await SetGuardianMfaPage(page);
                    break;
                default:
                    throw new Auth0InvalidTemplateException("Page name must be one of [login, reset_password]");
            }
        }

        private async Task SetLoginPage(BanjoPage page)
        {
            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var globalClients = await managementClient.Clients.GetAllAsync(new GetClientsRequest() { IsGlobal = true }, new PaginationInfo());
            var globalClient = globalClients.FirstOrDefault();
            if (globalClient == null)
            {
                throw new Auth0ResourceNotFoundException("No global client found.");
            }

            var clientUpdateRequest = new ClientUpdateRequest() { CustomLoginPage = page.Html, IsCustomLoginPageOn = true };
            await Update(async () =>
                {
                    await managementClient.Clients.UpdateAsync(globalClient.ClientId, clientUpdateRequest);
                    return page;
                },
                page.PageType.ToString());
        }

        private async Task SetPasswordResetPage(BanjoPage page)
        {
            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var updateRequest = new TenantSettingsUpdateRequest()
            {
                ChangePassword = new TenantChangePassword() { Enabled = true, Html = page.Html }
            };

            await Update(async () =>
                {
                    await managementClient.TenantSettings.UpdateAsync(updateRequest);
                    return page;
                },
                page.PageType.ToString());
        }
        
        private async Task SetGuardianMfaPage(BanjoPage page)
        {
            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var updateRequest = new TenantSettingsUpdateRequest()
            {
                GuardianMfaPage = new TenantGuardianMfaPage() { Enabled = true, Html = page.Html }
            };

            await Update(async () =>
                {
                    await managementClient.TenantSettings.UpdateAsync(updateRequest);
                    return page;
                },
                page.PageType.ToString());
        }
    }
}