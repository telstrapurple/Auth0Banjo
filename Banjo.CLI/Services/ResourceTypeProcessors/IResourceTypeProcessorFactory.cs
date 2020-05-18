using Banjo.CLI.Model;

namespace Banjo.CLI.Services.ResourceTypeProcessors
{
    public interface IResourceTypeProcessorFactory
    {
        IResourceTypeProcessor GetProcessor(ResourceType type);
    }
}