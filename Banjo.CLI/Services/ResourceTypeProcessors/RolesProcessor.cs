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
    public class RolesProcessor : AbstractSingleTypeResourceTypeProcessor<Role>
    {
        protected override ResourceType Type { get; } = ResourceType.Roles;

        private readonly ManagementApiClientFactory _managementApiClientFactory;
        private readonly IConverter<Auth0ResourceTemplate, Role> _converter;

        public RolesProcessor(
            IOptionsMonitor<Auth0ProcessArgsConfig> args,
            IConverter<Auth0ResourceTemplate, Role> converter,
            IReporter reporter, ManagementApiClientFactory managementApiClientFactory)
            : base(args, converter, reporter)
        {
            _managementApiClientFactory = managementApiClientFactory;
            _converter = converter;
        }

        public override async Task ProcessAsync(Auth0ResourceTemplate template)
        {
            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var role = _converter.Convert(template);

            //todo support proper pagination - how to do this where every api call is different?!
            var results = await managementClient.Roles.GetAllAsync(new GetRolesRequest(), new PaginationInfo());


            var matchingRole = results.FirstOrDefault(x => string.Equals(x.Name, role.Name));
            if (matchingRole == null)
            {
                var createRequest = Reflectorisor.CopyMembers<Role, RoleCreateRequest>(role);
                await Create(
                    async () => await managementClient.Roles.CreateAsync(createRequest),
                    request => request.Id, 
                    role.Name);
            }
            else
            {
                var updateRequest = Reflectorisor.CopyMembers<Role, RoleUpdateRequest>(role);
                await Update(
                    async () => await managementClient.Roles.UpdateAsync(matchingRole.Id, updateRequest),
                    matchingRole.Id,
                    role.Name
                );
            }
        }
    }
}