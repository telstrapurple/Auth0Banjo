using Autofac;
using Banjo.CLI.Services;
using Microsoft.Extensions.Logging;

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
        }
    }
}