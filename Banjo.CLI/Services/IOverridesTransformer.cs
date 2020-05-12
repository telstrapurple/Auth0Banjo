using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services
{
    public interface IOverridesTransformer
    {
        Task TransformAsync(Overrides overrides);
    }
}