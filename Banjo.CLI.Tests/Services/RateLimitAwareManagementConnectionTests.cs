using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Auth0.Core.Exceptions;
using Auth0.ManagementApi;
using Banjo.CLI.Services;
using FakeItEasy;
using Shouldly;
using Xunit;

namespace Banjo.CLI.Tests.Services
{
    public class RateLimitAwareManagementConnectionTests
    {
        [Fact]
        public async Task TestSuccessPassThrough()
        {
            var fakeMC = A.Fake<IManagementConnection>();
            var reporter = new TestReporter();

            var sut = new RateLimitAwareManagementConnection(fakeMC, reporter, 0);
            var uri = new Uri("http://hello/");
            var dictionary = (Dictionary<string, string>) null;
            await sut.GetAsync<string>(uri, dictionary);

            A.CallTo(() => fakeMC.GetAsync<string>(uri, dictionary, null)).MustHaveHappened();
        }

        [Fact]
        public async Task TestRateLimitExceptionsAreRetried()
        {
            var fakeMC = A.Fake<IManagementConnection>();
            var reporter = new TestReporter();
            var uri = new Uri("http://hello/");
            var dictionary = (Dictionary<string, string>) null;

            A.CallTo(() => fakeMC.GetAsync<string>(uri, dictionary, null)).Throws<RateLimitApiException>();

            var sut = new RateLimitAwareManagementConnection(fakeMC, reporter, 0);
            await Should.ThrowAsync<RateLimitApiException>(async () => await sut.GetAsync<string>(uri, dictionary, null));

            A.CallTo(() => fakeMC.GetAsync<string>(uri, dictionary, null)).MustHaveHappened(4, Times.Exactly);
            reporter.VerboseMessages.ShouldNotBeEmpty();
        }
        
        [Fact]
        public async Task TestRateLimitExceptionsThenSuccessResultsInSuccess()
        {
            var fakeMC = A.Fake<IManagementConnection>();
            var reporter = new TestReporter();
            var uri = new Uri("http://hello/");
            var dictionary = (Dictionary<string, string>) null;

            A.CallTo(() => fakeMC.GetAsync<string>(uri, dictionary, null))
                .Throws<RateLimitApiException>()
                .NumberOfTimes(2)
                .Then.Returns("hello");

            var sut = new RateLimitAwareManagementConnection(fakeMC, reporter, 0);
            (await sut.GetAsync<string>(uri, dictionary, null)).ShouldBe("hello");

            A.CallTo(() => fakeMC.GetAsync<string>(uri, dictionary, null)).MustHaveHappened(3, Times.Exactly);
            reporter.VerboseMessages.ShouldNotBeEmpty();
        }
    }
}