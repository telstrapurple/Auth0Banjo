using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Banjo.CLI.Commands;
using Banjo.CLI.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Banjo.CLI
{
    [Command(Name = "banjo")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        // typeof(HelloWorldCommand),
        typeof(ProcessCommand), typeof(HelloWorldCommand)
    )]
    public class Program2 : BanjoCommandBase
    {
        private static string GetVersion()
            => typeof(Program2).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;


        public static async Task<int> Main(string[] args)
        {
            return await Host.CreateDefaultBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging((context, builder) => { builder.SetMinimumLevel(LogLevel.Information).AddConsole(); })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<Auth0AuthenticationConfig>(hostContext.Configuration.GetSection("Auth0"));
                })
                .ConfigureContainer<ContainerBuilder>(container =>
                {
                    container.RegisterApplicationServices();
                })
                .RunCommandLineApplicationAsync<Program2>(args);
        }

        public override Task OnExecuteAsync(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return Task.FromResult(0);
        }
    }
}