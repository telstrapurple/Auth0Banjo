using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AsyncLazy;
using Banjo.CLI.Configuration;
using IdentityModel.Client;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;

namespace Banjo.CLI.Services
{
    public class Auth0TokenFactory : IAuth0TokenFactory
    {
        private readonly AsyncLazy<string> _token;
        private readonly HttpClient _httpClient;
        private readonly IOptionsMonitor<Auth0AuthenticationConfig> _config;
        private readonly IReporter _reporter;

        public Auth0TokenFactory(HttpClient httpClient, IOptionsMonitor<Auth0AuthenticationConfig> config, IReporter reporter)
        {
            _httpClient = httpClient;
            _config = config;
            _reporter = reporter;


            _token = new AsyncLazy<string>(async () =>
            {
                var disco = await _httpClient.GetDiscoveryDocumentAsync(address: $"https://{_config.CurrentValue.Domain}/");

                if (disco.IsError)
                {
                    _reporter.Error(disco.Error);
                    _reporter.Error(disco.Exception.Message);
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
                    _reporter.Error(token.ErrorDescription);
                    _reporter.Error(token.Exception.Message);
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