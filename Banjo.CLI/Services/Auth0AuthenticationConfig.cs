namespace Banjo.CLI.Services
{
    public class Auth0AuthenticationConfig
    {
        public string Domain { get; set;} //dev-4er5bl24.au.auth0.com
        public string ClientId { get; set;}
        public string ClientSecret { get; set;}

        // public Auth0AuthenticationConfig(string domain, string clientId, string clientSecret)
        // {
        //     Domain = domain;
        //     ClientId = clientId;
        //     ClientSecret = clientSecret;
        // }
    }
}