using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Banjo.CLI.IntegrationTests
{
    public class BasicTestSetupTests
    {
        private readonly ITestOutputHelper _outputter;

        public BasicTestSetupTests(ITestOutputHelper outputter)
        {
            _outputter = outputter;
        }

        [Fact]
        public void TestRootHelpArgWritesHelpOutput()
        {
            var program = new ITProgram(_outputter);
            program.Run(new string[] { "--help" });
            
            program.StandardOutput.AllOutput.ShouldContain("Show version information");
            program.StandardOutput.AllOutput.ShouldContain("banjo [command] [options]");
            program.StandardOutput.AllOutput.ShouldContain("process");
        }
        
        [Fact]
        public void TestProcessHelpArgWritesHelpOutput()
        {
            var program = new ITProgram(_outputter);
            program.Run(new string[] { "process", "--help" });

            var allStandard = program.StandardOutput.AllOutput;
            allStandard.ShouldContain("Usage:");
            allStandard.ShouldContain("--help");
            allStandard.ShouldContain("--override");
            allStandard.ShouldContain("--dry-run");
        }
    }
}