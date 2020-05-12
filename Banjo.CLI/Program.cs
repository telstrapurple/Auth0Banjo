using System;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Banjo.CLI.Commands;
using Banjo.CLI.Configuration;
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
        typeof(ProcessCommand), typeof(HelloWorldCommand)
    )]
    public class Program : BanjoCommandBase
    {
        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;


        public static async Task<int> Main(string[] args)
        {
            // using var httpEventListener = new EventSourceListener("Microsoft-System-Net-Http");
            
            return await Host.CreateDefaultBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging((context, builder) => { builder.SetMinimumLevel(LogLevel.Information).AddConsole(); })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    
                    config.Add(new ReloadingMemoryConfigurationSource());
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<Auth0AuthenticationConfig>(hostContext.Configuration.GetSection("Auth0"));
                    services.Configure<Auth0ProcessArgsConfig>(hostContext.Configuration);
                })
                .ConfigureContainer<ContainerBuilder>(container => { container.RegisterApplicationServices(); })
                .RunCommandLineApplicationAsync<Program>(args);
        }

        public override Task OnExecuteAsync(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return Task.FromResult(0);
        }
    }
    
    sealed class EventSourceListener : EventListener
    {
        private readonly string _eventSourceName;
        private readonly StringBuilder _messageBuilder = new StringBuilder();

        public EventSourceListener(string name)
        {
            _eventSourceName = name;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            base.OnEventSourceCreated(eventSource);

            if (eventSource.Name == _eventSourceName)
            {
                EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            base.OnEventWritten(eventData);

            string message;
            lock (_messageBuilder)
            {
                _messageBuilder.Append("<- Event ");
                _messageBuilder.Append(eventData.EventSource.Name);
                _messageBuilder.Append(" - ");
                _messageBuilder.Append(eventData.EventName);
                _messageBuilder.Append(" : ");
                _messageBuilder.AppendJoin(',', eventData.Payload);
                _messageBuilder.AppendLine(" ->");
                message = _messageBuilder.ToString();
                _messageBuilder.Clear();
            }
            Console.WriteLine(message);
        }
    }
}