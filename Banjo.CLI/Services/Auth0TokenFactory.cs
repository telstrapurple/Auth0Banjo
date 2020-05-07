using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AsyncLazy;
using Banjo.CLI.Configuration;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services
{
    public class Auth0TokenFactory : IAuth0TokenFactory
    {
        private readonly AsyncLazy<string> _token;
        private readonly HttpClient _httpClient;
        private readonly IOptionsMonitor<Auth0AuthenticationConfig> _config;
        private readonly ILogger<Auth0TokenFactory> _logger;

        public Auth0TokenFactory(HttpClient httpClient, IOptionsMonitor<Auth0AuthenticationConfig> config, ILogger<Auth0TokenFactory> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;

            _token = new AsyncLazy<string>(async () =>
            {
                var disco = await _httpClient.GetDiscoveryDocumentAsync(address: $"https://{_config.CurrentValue.Domain}/");

                if (disco.IsError)
                {
                    _logger.LogError(disco.Exception, disco.Error);
                    throw disco.Exception;
                }

                var token = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
                {
                    Address = disco.TokenEndpoint,
                    ClientId = _config.CurrentValue.ClientId,
                    ClientSecret = _config.CurrentValue.ClientSecret,
                    Parameters = new Dictionary<string, string> { { "audience", $"https://{_config.CurrentValue.Domain}/api/v2/" } }
                });

                if (token.IsError)
                {
                    _logger.LogError(token.Exception, token.ErrorDescription);
                    throw token.Exception;
                }

                return token.AccessToken;
            });
        }


        public async Task<string> GetAuth0ManagementClientToken()
        {
            return await _token.GetValueAsync();
        }
    }
}