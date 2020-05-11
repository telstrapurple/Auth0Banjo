using System.Net.Http;
using Auth0.ManagementApi;
using Autofac;
using Banjo.CLI.Configuration;
using Banjo.CLI.Services;
using Banjo.CLI.Services.PipelineStages;
using Banjo.CLI.Services.ResourceTypeProcessors;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable RedundantTypeArgumentsOfMethod - Type arguments make it easier to see exactly what is being registered

namespace Banjo.CLI
{
    public static class ApplicationRegistrations
    {
        public static void RegisterApplicationServices(this ContainerBuilder container)
        {
            container.Register<ConsoleReporter>(
                    context =>
                    {
                        var args = context.Resolve<IOptionsMonitor<Auth0ProcessArgsConfig>>();
                        return new ConsoleReporter(context.Resolve<IConsole>(), args?.CurrentValue.Verbose ?? false, false);
                    })
                .AsImplementedInterfaces();
            container.Register<PhysicalConsole>(context => PhysicalConsole.Singleton as PhysicalConsole)
                .SingleInstance()
                .AsImplementedInterfaces();

            container.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();
            container.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            container.RegisterType<BanjoGreeter>().AsImplementedInterfaces();
            container.RegisterType<DefaultTemplateSource>().AsImplementedInterfaces();
            container.RegisterType<OverridesSource>().AsImplementedInterfaces();

            container.RegisterType<Auth0TokenFactory>().AsImplementedInterfaces().SingleInstance();

            container.RegisterInstance(new HttpClient()).As(typeof(HttpClient));

            container.RegisterType<ManagementApiClientFactory>()
                .AsSelf()
                .AsImplementedInterfaces();

            container.RegisterType<HttpClientManagementConnection>().AsImplementedInterfaces().SingleInstance();

            container.RegisterGeneric(typeof(Auth0ResourceTemplateToApiModelConverter<>))
                .AsSelf()
                .AsImplementedInterfaces();

            container.RegisterType<PipelineStageFactory>()
                .AsSelf()
                .AsImplementedInterfaces();

            container.RegisterType<ArgumentConfigurator>().AsSelf().AsImplementedInterfaces();
            container.RegisterType<ResourceTypeProcessorFactory>().AsSelf().AsImplementedInterfaces();


            container.RegisterType<ClientsProcessor>().AsImplementedInterfaces();
            container.RegisterType<ResourceServersProcessor>().AsImplementedInterfaces();
            container.RegisterType<ClientGrantsProcessor>().AsImplementedInterfaces();
            container.RegisterType<RolesProcessor>().AsImplementedInterfaces();
            container.RegisterType<RulesProcessor>().AsImplementedInterfaces();
            //add more resource type processors as we implement them.
        }
    }
}