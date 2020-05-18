using System.Threading.Tasks;
using AsyncLazy;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services
{
    public class CachingOverridesSource : IOverridesSource
    {
        private AsyncLazy<Overrides> _lazy;

        public CachingOverridesSource(IOverridesSource overridesSource)
        {
            _lazy = new AsyncLazy<Overrides>(async () => await overridesSource.GetOverridesAsync());
        }

        public async Task<Overrides> GetOverridesAsync()
        {
            return await _lazy.GetValueAsync();
        }
    }
}