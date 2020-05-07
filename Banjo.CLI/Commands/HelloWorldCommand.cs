using System;
using System.Threading.Tasks;
using Banjo.CLI.Services;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.Commands
{
    [Obsolete("demo service, can be deleted")]
    [Command(Name = "hello")]
    public class HelloWorldCommand : BanjoCommandBase
    {
        [Option(CommandOptionType.SingleValue, ShortName = "n", LongName = "name")]
        public string Name { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "c", LongName = "colour")]
        public ConsoleColor Colour { get; set; } = ConsoleColor.Green;

        private readonly IGreeter _greeter;

        public HelloWorldCommand(IGreeter greeter)
        {
            _greeter = greeter;
        }

        public override async Task OnExecuteAsync(CommandLineApplication app)
        {
            Console.WriteLine(_greeter.GetHello(Name));
        }
    }
}