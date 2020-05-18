using System.Threading.Tasks;
using Banjo.CLI.Services;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Banjo.CLI.Tests.Services
{
    public class ReloadingMemoryConfigurationSourceTests
    {
        [Fact]
        public async Task TestBuildCreatesReloadingMemoryConfigurationProvider()
        {
            var configBuilder = new ConfigurationBuilder();
            new ReloadingMemoryConfigurationSource().Build(configBuilder).ShouldBeOfType(typeof(ReloadingMemoryConfigurationProvider));
        }
    }
}