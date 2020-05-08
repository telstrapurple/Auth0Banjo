using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Banjo.CLI.Services
{
    //Copied from Microsoft.Extensions.Configuration.MemoryMemoryConfigurationSource
    //to support change notifications when the underlying in-memory data is changed
    public class ReloadingMemoryConfigurationSource : IConfigurationSource
    {
        public IEnumerable<KeyValuePair<string, string>> InitialData { get; set; }
        
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ReloadingMemoryConfigurationProvider(this);
        }
    }
}