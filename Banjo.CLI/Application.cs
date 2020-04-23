using System.Data;
using Microsoft.Extensions.Logging;
using Oakton;

namespace Banjo.CLI
{
    public interface IApplication
    {
        CommandExecutor Executor { get; set; }

        int Run(string[] args);
    }
    
    public class Application : IApplication
    {
        public CommandExecutor Executor { get; set; }

        private readonly ILogger<Application> _logger;

        public Application(ILogger<Application> logger)
        {
            _logger = logger;
        }

        public int Run(string[] args)
        {
            if (Executor == null)
                throw new NoNullAllowedException(nameof(Executor));

            // Execute CLI in interactive mode
            _logger.LogTrace("application initialized");

            return Executor.Execute(args);
        }
    }
}