using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth0.ManagementApi.Clients;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.Services
{
    public static class Auth0ManagementApiExtensions
    {
        
        public static IAsyncEnumerable<Client> GetAllAsync(this ClientsClient client, GetClientsRequest request, IReporter reporter = null)
        {
            return GetAllAsyncInternal(page => client.GetAllAsync(request, page), "clients", reporter);
        }
        
        public static IAsyncEnumerable<ClientGrant> GetAllAsync(this ClientGrantsClient client, GetClientGrantsRequest request, IReporter reporter = null)
        {
            return GetAllAsyncInternal(page => client.GetAllAsync(request, page), "client grants", reporter);
        }
        
        public static IAsyncEnumerable<ResourceServer> GetAllAsync(this ResourceServersClient client, IReporter reporter = null)
        {
            return GetAllAsyncInternal(client.GetAllAsync, "resource servers", reporter);
        }
        
        public static IAsyncEnumerable<Connection> GetAllAsync(this ConnectionsClient client, GetConnectionsRequest request, IReporter reporter = null)
        {
            return GetAllAsyncInternal(page => client.GetAllAsync(request, page), "connections", reporter);
        }
        
        public static IAsyncEnumerable<Role> GetAllAsync(this RolesClient client, GetRolesRequest request, IReporter reporter = null)
        {
            return GetAllAsyncInternal(page => client.GetAllAsync(request, page), "roles", reporter);
        }
        
        public static IAsyncEnumerable<Rule> GetAllAsync(this RulesClient client, GetRulesRequest request, IReporter reporter = null)
        {
            return GetAllAsyncInternal(page => client.GetAllAsync(request, page), "rules", reporter);
        }
        
        private static async IAsyncEnumerable<T> GetAllAsyncInternal<T>(Func<PaginationInfo, Task<IPagedList<T>>> getter, string resourceName, IReporter reporter = null)
        {
            reporter ??= NullReporter.Singleton;
            
            int page = 0;
            const int perPage = 50;
            
            while (true)
            {
                reporter.Verbose($"Getting {resourceName} page {page} requesting {perPage} per page");
                var result = await getter.Invoke(new PaginationInfo(page, perPage, true));

                var resultAsList = result.ToList();
                foreach (var r in resultAsList)
                {
                    yield return r;
                }
            
                if (resultAsList.Count < perPage)
                {
                    break;
                }
                page++;
            }
        }
    }
}