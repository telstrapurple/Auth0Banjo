using System.Linq;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services.ResourceTypeProcessors
{
    public class ResourceServersProcessor : AbstractSingleTypeResourceTypeProcessor<ResourceServer>
    {
        protected override ResourceType Type { get; } = ResourceType.ResourceServers;

        private readonly ManagementApiClientFactory _managementApiClientFactory;
        private readonly IConverter<Auth0ResourceTemplate, ResourceServer> _converter;

        public ResourceServersProcessor(
            IOptionsMonitor<Auth0ProcessArgsConfig> args,
            IConverter<Auth0ResourceTemplate, ResourceServer> converter,
            IReporter reporter,
            ManagementApiClientFactory managementApiClientFactory)
            : base(args, converter, reporter)
        {
            _managementApiClientFactory = managementApiClientFactory;
            _converter = converter;
        }

        public override async Task ProcessAsync(Auth0ResourceTemplate template)
        {
            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var templatedResourceServer = _converter.Convert(template);

            // //todo support proper pagination - how to do this where every api call is different?!
            var results = await managementClient.ResourceServers.GetAllAsync(new PaginationInfo());

            FixIllegalOptions(templatedResourceServer);

            var matchingResourceServer = results.FirstOrDefault(x => string.Equals(x.Name, templatedResourceServer.Name));
            if (matchingResourceServer == null)
            {
                var createRequest = Reflectorisor.CopyMembers<ResourceServer, ResourceServerCreateRequest>(templatedResourceServer);
                await Create(
                    async () => await managementClient.ResourceServers.CreateAsync(createRequest),
                    request => request.Id,
                    templatedResourceServer.Name
                );
            }
            else
            {
                var updateRequest = Reflectorisor.CopyMembers<ResourceServer, ResourceServerUpdateRequest>(templatedResourceServer);
                await Update(
                    async () => await managementClient.ResourceServers.UpdateAsync(matchingResourceServer.Id, updateRequest),
                    matchingResourceServer.Id,
                    templatedResourceServer.Name
                );
            }
        }

        private static void FixIllegalOptions(ResourceServer resourceServer)
        {
            resourceServer.TokenLifetimeForWeb = null;
        }
    }
}