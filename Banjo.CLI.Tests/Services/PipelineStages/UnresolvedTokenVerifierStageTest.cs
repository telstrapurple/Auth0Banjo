using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services.PipelineStages;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace Banjo.CLI.Tests.Services.PipelineStages
{
    public class UnresolvedTokenVerifierStageTest
    {

        [Theory]
        [InlineData(new[] {"no unreplaced tokens"}, new string[] {})]
        [InlineData(new[]{"%%HELLO%% %%hello%%"}, new [] {"%HELLO%%", "%%hello%%"})] //multiple unreplaced tokens in a single line
        [InlineData(new[]{"%%HELLO%%", "%%hello%%"}, new [] {"%HELLO%%", "%%hello%%"})] //one unreplaced token per line
        [InlineData(new[]{"something something %%unreplacedTOKEN%% something else"}, new[] {"%%unreplacedTOKEN%%"})] //one unreplaced token with other ok content
        public async Task Test(IEnumerable<string> jtokenValues, IEnumerable<string> requiredTokens)
        {
            var messageCollector = new TestReporter();
            var stage = new UnresolvedTokenVerifierStage(messageCollector);
            var json = new JObject();
            
            foreach (var value in jtokenValues)
            {
                json.Add(Guid.NewGuid().ToString(), new JValue(value));
            }
            var t = new Auth0ResourceTemplate
            {
                Type = ResourceType.Clients,
                Template = json
            };
            await stage.Process(t);

            var requiredTokenList = requiredTokens.ToList();
            if (!requiredTokenList.Any())
            {
                messageCollector.Messages.ShouldBeEmpty();
            } else
            {
                foreach (var token in requiredTokenList)
                {
                    messageCollector.ErrorMessages.ShouldHaveMessageThatContains(token);
                }
            }
        }
    }
}