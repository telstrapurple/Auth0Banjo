using System;
using Xunit;

namespace Banjo.CLI.IntegrationTests
{
    [CollectionDefinition("HasEnvironmentVariables")]
    public class HasEnvironmentVariablesCollectionFixture : ICollectionFixture<Auth0ConfigVariablesFixture>
    {
        
    }
    
    public class Auth0ConfigVariablesFixture : IDisposable
    {
        public string Domain { get; } = "example.com";
        public string ClientId { get; } = "clientid";
        public string ClientSecret { get; } = "clientsecret";
        
        public Auth0ConfigVariablesFixture()
        {
            Environment.SetEnvironmentVariable("AUTH0__DOMAIN", Domain);
            Environment.SetEnvironmentVariable("AUTH0__CLIENTID", ClientId);
            Environment.SetEnvironmentVariable("AUTH0__CLIENTSECRET", ClientSecret);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("AUTH0__DOMAIN", null);
            Environment.SetEnvironmentVariable("AUTH0__CLIENTID", null);
            Environment.SetEnvironmentVariable("AUTH0__CLIENTSECRET", null);
        }
    }
}