using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.Commands
{
    [HelpOption("--help")]
    public abstract class BanjoCommandBase
    {
        public abstract Task OnExecuteAsync(CommandLineApplication app);
    }
}