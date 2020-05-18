using System.Collections.Generic;
using Shouldly;

namespace Banjo.CLI.Tests
{
    [ShouldlyMethods]
    public static class ShouldlyStringEnumerableExtensions
    {
        public static void ShouldHaveMessageThatContains(this IEnumerable<string> messages, string substring)
        {
            foreach (var message in messages)
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