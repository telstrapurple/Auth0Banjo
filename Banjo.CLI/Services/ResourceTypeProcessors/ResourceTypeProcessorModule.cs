using System.Reflection;
using Autofac;
using Module = Autofac.Module;

namespace Banjo.CLI.Services.ResourceTypeProcessors
{
    public class ResourceTypeProcessorModule : Module
    {
        protected override void Load(ContainerBuilder container)
        {
            container.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(ResourceTypeProcessorModule)))
                .Where(t => t.IsAssignableTo<IResourceTypeProcessor>())
                .AsImplementedInterfaces();
        }
    }
}