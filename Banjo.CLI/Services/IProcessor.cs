using System.Threading.Tasks;

namespace Banjo.CLI.Services
{
    public interface IProcessor<T>
    {
        Task<T> Process(T t);
    }
}