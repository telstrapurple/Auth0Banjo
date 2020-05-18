using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Banjo.CLI.Commands;
using Banjo.CLI.Configuration;
using Banjo.CLI.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Banjo.CLI
{
    [Command(Name = "banjo")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(ProcessCommand)
    )]
    public class Program : BanjoCommandBase
    {
        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        public async Task<int> Run(string[] args)
        {
            return await _Run(args, CreateHostBuilder());
        }
        
        protected virtual async Task<int> _Run(string[] args, IHostBuilder hostBuilder)
        {
            return await hostBuilder.RunCommandLineApplicationAsync<Program>(args);
        }

        public static async Task<int> Main(string[] args)
        {
            return await new Program().Run(args);
        }

        protected virtual IHostBuilder CreateHostBuilder()
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging((context, builder) => { builder.SetMinimumLevel(LogLevel.Information).AddConsole(); })
                .ConfigureAppConfiguration((hostingContext, config) => { config.Add(new ReloadingMemoryConfigurationSource()); })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<Auth0AuthenticationConfig>(hostContext.Configuration.GetSection("Auth0"));
                    services.Configure<Auth0ProcessArgsConfig>(hostContext.Configuration);
                })
                .ConfigureContainer<ContainerBuilder>(container => { container.RegisterApplicationServices(); });
            return hostBuilder;
        }

        public override Task OnExecuteAsync(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return Task.FromResult(0);
        }
    }
}