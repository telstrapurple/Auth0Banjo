using System.Threading.Tasks;

namespace Banjo.CLI.Services.PipelineStages
{
    public interface IPipelineStage<T>
    {
        Task<T> Process(T t);
    }
}