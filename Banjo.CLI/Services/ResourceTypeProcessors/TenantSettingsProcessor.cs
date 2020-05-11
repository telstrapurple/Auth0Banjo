using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services.ResourceTypeProcessors
{
    public class TenantSettingsProcessor : AbstractSingleTypeResourceTypeProcessor<TenantSettings>
    {
        protected override ResourceType Type { get; } = ResourceType.TenantSettings;

        private readonly ManagementApiClientFactory _managementApiClientFactory;
        private readonly IConverter<Auth0ResourceTemplate, TenantSettings> _converter;

        public TenantSettingsProcessor(
            IOptionsMonitor<Auth0ProcessArgsConfig> args,
            IConverter<Auth0ResourceTemplate, TenantSettings> converter,
            IReporter reporter,
            ManagementApiClientFactory managementApiClientFactory)
            : base(args, converter, reporter)
        {
            _managementApiClientFactory = managementApiClientFactory;
            _converter = converter;
        }

        public override async Task ProcessAsync(Auth0ResourceTemplate template)
        {
            var settings = _converter.Convert(template);

            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var updateRequest = Reflectorisor.CopyMembers<TenantSettings, TenantSettingsUpdateRequest>(settings);

            await Update(async () => await managementClient.TenantSettings.UpdateAsync(updateRequest),
                template.Type.Name);
        }
    }
}