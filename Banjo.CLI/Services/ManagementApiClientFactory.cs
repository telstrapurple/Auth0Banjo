using System.Threading.Tasks;
using Auth0.ManagementApi;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services
{
    public class ManagementApiClientFactory
    {
        private readonly IAuth0TokenFactory _tokenFactory;
        private readonly IOptionsMonitor<Auth0AuthenticationConfig> _config;

        public ManagementApiClientFactory(IAuth0TokenFactory tokenFactory, IOptionsMonitor<Auth0AuthenticationConfig> config)
        {
            _tokenFactory = tokenFactory;
            _config = config;
        }

        public async Task<ManagementApiClient> CreateAsync()
        {
            var token = await _tokenFactory.GetAuth0ManagementClientToken();
            return new ManagementApiClient(token, _config.CurrentValue.Domain);
        }
    }
}