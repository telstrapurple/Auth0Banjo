using System.Collections.Generic;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.Services
{
    public class TransformingOverridesSource : IOverridesSource
    {
        private readonly IOverridesSource _wrapped;
        private readonly IEnumerable<IOverridesTransformer> _transformers;

        public TransformingOverridesSource(IOverridesSource wrapped, IEnumerable<IOverridesTransformer> transformers)
        {
            _wrapped = wrapped;
            _transformers = transformers;
        }

        public async Task<Overrides> GetOverridesAsync(string overridesFileLocation)
        {
            var ov = await _wrapped.GetOverridesAsync(overridesFileLocation);

            foreach (var transformer in _transformers)
            {
                await transformer.TransformAsync(ov);
            }

            return ov;
        }
    }
}