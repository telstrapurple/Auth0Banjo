using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Oakton;

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
        private readonly IConfiguration _config;

        public HelloWorldCommand(IGreeter greeter, IConfiguration config)
        {
            _greeter = greeter;
            _config = config;
        }

        public override async Task OnExecuteAsync(CommandLineApplication app)
        {
            ConsoleWriter.Write(Colour, _greeter.GetHello(Name));
            
            foreach (var pair in _config.AsEnumerable().ToImmutableSortedDictionary(k => k.Key, v => v.Value))
            {
                ConsoleWriter.Write(ConsoleColor.Cyan, $"{pair.Key} = {pair.Value}");
            }
            
            
        }
    }
}