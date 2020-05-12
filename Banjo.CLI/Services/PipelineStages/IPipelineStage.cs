using System.Threading.Tasks;

namespace Banjo.CLI.Services.PipelineStages
{
    public interface IPipelineStage<T>
    {
        string Name { get; }
        Task<T> Process(T t);
    }
}