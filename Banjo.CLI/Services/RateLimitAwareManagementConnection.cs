using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Auth0.Core.Exceptions;
using Auth0.ManagementApi;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace Banjo.CLI.Services
{
    public class RateLimitAwareManagementConnection : IManagementConnection
    {
        private readonly AsyncRetryPolicy<object> _retryPolicy;

        private readonly IManagementConnection _wrapped;

        public RateLimitAwareManagementConnection(IManagementConnection wrapped, IReporter reporter)
        {
            _wrapped = wrapped;
            _retryPolicy = Policy<object>
                .Handle<RateLimitApiException>()
                .WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(3)
                    },
                    (result, span, context) => { reporter.Verbose("Rate limit exceeded, waiting before retrying."); });
        }

        public async Task<T> GetAsync<T>(Uri uri, IDictionary<string, string> headers, JsonConverter[] converters = null)
        {
            return (T) await _retryPolicy.ExecuteAsync(async () => await _wrapped.GetAsync<T>(uri, headers, converters));
        }

        public async Task<T> SendAsync<T>(HttpMethod method, Uri uri, object body, IDictionary<string, string> headers, IList<FileUploadParameter> files = null)
        {
            return (T) await _retryPolicy.ExecuteAsync(async () => await _wrapped.SendAsync<T>(method, uri, body, headers, files));
        }
    }
}