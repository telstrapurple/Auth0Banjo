using System.Collections.Generic;
using Banjo.CLI.Model;
using Microsoft.Extensions.Logging;

namespace Banjo.CLI.Services.ResourceTypeProcessors
{
    public class ResourceTypeProcessorFactory
    {
        private readonly Dictionary<string, IResourceTypeProcessor> _processorMap;
        private readonly ILogger<ResourceTypeProcessorFactory> _logger;

        public ResourceTypeProcessorFactory(IEnumerable<IResourceTypeProcessor> availableProcessors, ILogger<ResourceTypeProcessorFactory> logger)
        {
            _logger = logger;

            _processorMap = new Dictionary<string, IResourceTypeProcessor>();
            foreach (var processor in availableProcessors)
            {
                foreach (var resourceType in processor.ResourceTypes)
                {
                    if (!_processorMap.ContainsKey(resourceType.Name))
                    {
                        _processorMap.Add(resourceType.Name, processor);
                    }
                    else
                    {
                        _logger.LogWarning($"Multiple processors claim to handle resource type {resourceType.Name}. Ignoring processor registration {processor.GetType().Name}.");
                    }
                }
            }
        }

        public IResourceTypeProcessor GetProcessor(ResourceType type)
        {
            var exists = _processorMap.TryGetValue(type.Name, out var result);
            if (!exists)
            {
                throw new KeyNotFoundException($"No processor registered that can handle resource type {type.Name}");
            }

            return result;
        }
    }
}