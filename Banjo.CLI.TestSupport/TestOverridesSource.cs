using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services;

namespace Banjo.CLI.TestSupport
{
    public class TestOverridesSource : IOverridesSource
    {
        public Overrides Overrides { get; }

        public TestOverridesSource(Overrides overrides = null)
        {
            Overrides = overrides ?? new Overrides();
        }

        public async Task<Overrides> GetOverridesAsync()
        { 
            return Overrides;
        }
    }
}