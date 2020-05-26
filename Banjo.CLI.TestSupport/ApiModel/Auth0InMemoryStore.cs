using System.Collections.Generic;
using Auth0.ManagementApi.Models;

namespace Banjo.CLI.TestSupport.ApiModel
{
    public class Auth0InMemoryStore
    {
        public IList<Client> Clients { get; } = new List<Client>();
        public IList<ResourceServer> ResourceServers { get; } = new List<ResourceServer>();
        public IList<ClientGrant> ClientGrants { get; } = new List<ClientGrant>();
    }
}