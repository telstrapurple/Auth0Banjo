using System.Threading.Tasks;

namespace Banjo.CLI.Services
{
    public interface IAuth0TokenFactory
    {
        Task<string> GetAuth0ManagementClientToken();
    }
}