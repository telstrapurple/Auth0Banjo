using System.Threading.Tasks;
using Banjo.CLI.Services;

namespace Banjo.CLI.TestSupport
{
    public class TestTokenFactory : IAuth0TokenFactory
    {
        public string Token { get; set; }

        public async Task<string> GetAuth0ManagementClientToken()
        {
            return Token;
        }
    }
}