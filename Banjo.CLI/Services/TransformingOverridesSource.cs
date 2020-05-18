using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services
{
    public class TransformingOverridesSource : IOverridesSource
    {
        private readonly IOverridesSource _wrapped;
        private readonly IEnumerable<IOverridesTransformer> _transformers;

        public TransformingOverridesSource(IOverridesSource wrapped, IEnumerable<IOverridesTransformer> transformers)
        {
            _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
            _transformers = transformers ?? new List<IOverridesTransformer>();
        }

        public async Task<Overrides> GetOverridesAsync()
        {
            var ov = await _wrapped.GetOverridesAsync();

            foreach (var transformer in _transformers)
            {
                await transformer.TransformAsync(ov);
            }

            return ov;
        }
    }
}