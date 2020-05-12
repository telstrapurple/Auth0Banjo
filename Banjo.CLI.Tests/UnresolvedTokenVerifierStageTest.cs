using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services.PipelineStages;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace Banjo.CLI.Tests
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
            var messageCollector = new MessageCollectorReporter();
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
                    messageCollector.ShouldHaveMessageThatContains(token);
                }
            }
        }
    }
    
    public class MessageCollectorReporter : IReporter
    {
        public List<string> Messages { get; } = new  List<string>();

        public void Verbose(string message)
        {
            Messages.Add(message);
        }

        public void Output(string message)
        {
            Messages.Add(message);
        }

        public void Warn(string message)
        {
            Messages.Add(message);
        }

        public void Error(string message)
        {
            Messages.Add(message);
        }
    }
    
    [ShouldlyMethods]
    public static class ShouldlyMessageCollectorReporterExtensions
    {
        public static void ShouldHaveMessageThatContains(this MessageCollectorReporter collector, string substring)
        {
            foreach (var message in collector.Messages)
            {
                try
                {
                    message.ShouldContain(substring);
                    //one of them succeeded, so hooray!
                    return;
                } catch (ShouldAssertException)
                {
                    //no need to do anything, just move on to the next message
                }
            }
            
            throw new ShouldAssertException($"None of the messages contained the target substring \"{substring}\"");
        }
    }
}