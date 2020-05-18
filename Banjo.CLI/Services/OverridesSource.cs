using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services
{
    public interface IOverridesSource
    {
        Task<Overrides> GetOverridesAsync();
    }
}