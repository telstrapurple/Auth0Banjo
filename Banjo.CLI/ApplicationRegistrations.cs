using System.Net.Http;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Autofac;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Banjo.CLI.Services;
using Banjo.CLI.Services.Processors;
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

            container.RegisterGeneric(typeof(TemplateReaderOld<>))
                .As(typeof(ITemplateReaderOld<>));

            container.RegisterType<ClientsProcessor>()
                .AsSelf()
                .AsImplementedInterfaces();

            container.RegisterType<HttpClientManagementConnection>().AsImplementedInterfaces().SingleInstance();

            container.RegisterGeneric(typeof(Auth0ResourceTemplateToApiModelConverter<>))
                .AsSelf()
                .AsImplementedInterfaces();

            container.RegisterType<ProcessorFactory>()
                .AsSelf()
                .AsImplementedInterfaces();

            container.RegisterType<ArgumentConfigurator>().AsSelf().AsImplementedInterfaces();
            container.RegisterType<ResourceTypeProcessorFactory>().AsSelf().AsImplementedInterfaces();

            container.RegisterType<ClientsProcessor>().AsImplementedInterfaces();
            //add more resource type processors as we implement them.
        }
    }
}