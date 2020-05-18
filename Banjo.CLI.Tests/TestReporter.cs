using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.Tests
{
    public class TestReporter : IReporter
    {
        public IEnumerable<string> Messages => VerboseMessages.Concat(OutputMessages).Concat(WarnMessages).Concat(ErrorMessages);
        
        public List<string> VerboseMessages { get; } = new  List<string>();
        public List<string> OutputMessages { get; } = new  List<string>();
        public List<string> WarnMessages { get; } = new  List<string>();
        public List<string> ErrorMessages { get; } = new  List<string>();


        public void Verbose(string message)
        {
            VerboseMessages.Add(message);
        }

        public void Output(string message)
        {
            OutputMessages.Add(message);
        }

        public void Warn(string message)
        {
            WarnMessages.Add(message);
        }

        public void Error(string message)
        {
            ErrorMessages.Add(message);
        }
    }
}