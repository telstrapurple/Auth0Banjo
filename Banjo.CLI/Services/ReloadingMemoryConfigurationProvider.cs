using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Banjo.CLI.Services
{
    
    //Copied from Microsoft.Extensions.Configuration.MemoryMemoryConfigurationProvider
    //to support change notifications when the underlying in-memory data is changed
    public class ReloadingMemoryConfigurationProvider : ConfigurationProvider, IEnumerable<KeyValuePair<string, string>>
    {
        private readonly ReloadingMemoryConfigurationSource _source;

        /// <summary>
        /// Initialize a new instance from the source.
        /// </summary>
        /// <param name="source">The source settings.</param>
        public ReloadingMemoryConfigurationProvider(ReloadingMemoryConfigurationSource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));

            if (_source.InitialData != null)
            {
                foreach (var pair in _source.InitialData)
                {
                    Data.Add(pair.Key, pair.Value);
                }
            }
        }

        /// <summary>
        /// Add a new key and value pair.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="value">The configuration value.</param>
        public void Add(string key, string value)
        {
            Data.Add(key, value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public override void Set(string key, string value)
        {
            base.Set(key, value);
            OnReload();
        }
        
        private void InnerSet(string key, string value)
        {
            base.Set(key, value);
        }
        
        public void SetMany(Dictionary<string, string> newValues)
        {
            foreach (var (key, value) in newValues)
            {
                InnerSet(key, value);
            }
            OnReload();
        }
    }
}