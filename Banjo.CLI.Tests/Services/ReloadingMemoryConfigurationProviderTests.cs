using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Banjo.CLI.Services;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Banjo.CLI.Tests.Services
{
    public class ReloadingMemoryConfigurationProviderTests
    {
        [Fact]
        public async Task TestSetShouldInvokeChangeHandlerExactlyOnce()
        {
            var configBuilder = new ConfigurationBuilder();
            var sut = new ReloadingMemoryConfigurationSource().Build(configBuilder) as ReloadingMemoryConfigurationProvider;
            sut.ShouldNotBeNull();

            var callback = A.Fake<Action<object>>();

            sut.GetReloadToken().RegisterChangeCallback(callback, null);

            sut.Set("myKey", "myValue");

            A.CallTo(() => callback.Invoke(A<object>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task TestSetManyShouldInvokeChangeHandlerExactlyOnce()
        {
            var configBuilder = new ConfigurationBuilder();
            var sut = new ReloadingMemoryConfigurationSource().Build(configBuilder) as ReloadingMemoryConfigurationProvider;
            sut.ShouldNotBeNull();

            var callback = A.Fake<Action<object>>();

            sut.GetReloadToken().RegisterChangeCallback(callback, null);

            sut.SetMany(new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2" } });

            A.CallTo(() => callback.Invoke(A<object>._)).MustHaveHappenedOnceExactly();
        }
    }
}