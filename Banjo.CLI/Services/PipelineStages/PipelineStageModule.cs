using System.Reflection;
using Autofac;
using Banjo.CLI.Model;
using Module = Autofac.Module;

namespace Banjo.CLI.Services.PipelineStages
{
    public class PipelineStageModule : Module
    {
        protected override void Load(ContainerBuilder container)
        {
            container.RegisterType<PipelineStageFactory>().AsImplementedInterfaces();
            
            container.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(PipelineStageModule)))
                .Where(t => t.IsAssignableTo<IPipelineStage<Auth0ResourceTemplate>>())
                .AsSelf(); //these are resolved by concrete type, so need to be registered AsSelf() rather than AsImplementedInterface()
        }
    }
}