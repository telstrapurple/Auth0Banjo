using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services.ResourceTypeProcessors
{
    public interface IResourceTypeProcessor
    {
        public ResourceType[] ResourceTypes { get; }

        public void Verify(Auth0ResourceTemplate template);
        
        public Task ProcessAsync(Auth0ResourceTemplate template);
    }
}