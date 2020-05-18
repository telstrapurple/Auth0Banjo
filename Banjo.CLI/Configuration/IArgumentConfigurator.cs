namespace Banjo.CLI.Configuration
{
    public interface IArgumentConfigurator
    {
        void AddConfiguration(Auth0ProcessArgsConfig config);
    }
}