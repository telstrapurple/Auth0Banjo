using System.Net.Http;
using Autofac;
using Banjo.CLI.Services;
using Microsoft.Extensions.Logging;

// ReSharper disable RedundantTypeArgumentsOfMethod - Type arguments make it easier to see exactly what is being registered

namespace Banjo.CLI
{
    public static class ApplicationRegistrations
    {
        public static void RegisterApplicationServices(this ContainerBuilder container)
        {
            container.RegisterType<Application>().As<IApplication>().SingleInstance();
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

            container.RegisterGeneric(typeof(DeserialisingTemplateReader<>))
                .As(typeof(ITemplateReader<>));

            container.RegisterType<ClientsProcessor>()
                .AsSelf()
                .AsImplementedInterfaces();
        }
    }
}