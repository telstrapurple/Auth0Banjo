using System.Threading.Tasks;
using Auth0.ManagementApi;

namespace Banjo.CLI.Services
{
    public interface IManagementApiClientFactory
    {
        Task<ManagementApiClient> CreateAsync();
    }
}