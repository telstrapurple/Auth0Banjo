using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services
{
    public interface IResourceTypeProcessor
    {
        public ResourceType[] ResourceTypes { get; }

        public Task ProcessAsync(TemplateMetadata template);
    }
}