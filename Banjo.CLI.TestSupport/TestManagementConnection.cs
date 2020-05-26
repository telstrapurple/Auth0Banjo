using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.Services;
using Banjo.CLI.TestSupport.ApiModel;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Banjo.CLI.TestSupport
{
    public class TestManagementConnection : IManagementConnection
    {
        
        private readonly ITestOutputHelper _output;
        private readonly Auth0InMemoryStore _store;
        private readonly IList<ApiCall> _apiCalls;

        public TestManagementConnection(ITestOutputHelper output, Auth0InMemoryStore store, IList<ApiCall> apiCalls)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _store = store;
            _apiCalls = apiCalls;
        }

        public async Task<T> GetAsync<T>(Uri uri, IDictionary<string, string> headers, JsonConverter[] converters = null)
        {
            _output.WriteLine("GetAsync: {0}", uri);
            _output.WriteLine("GetAsync: Headers: {0}", JsonConvert.SerializeObject(headers, Formatting.Indented));
            
            return (T) ExecuteApiCall(HttpMethod.Get, uri, null);
        }

        public async Task<T> SendAsync<T>(HttpMethod method, Uri uri, object body, IDictionary<string, string> headers, IList<FileUploadParameter> files = null)
        {
            _output.WriteLine("SendAsync: {0} {1}", method, uri);
            _output.WriteLine("SendAsync: Headers: {0}", JsonConvert.SerializeObject(headers, Formatting.Indented));
            _output.WriteLine("SendAsync: Body: {0}", JsonConvert.SerializeObject(body, Formatting.Indented));

            return (T) ExecuteApiCall(method, uri, body);
        }

        private object ExecuteApiCall(HttpMethod method, Uri uri, object body)
        {
            foreach (var call in _apiCalls)
            {
                if (call.IsMatch(method, uri))
                {
                    return call.Execute(uri, body, _store);
                }
            }

            throw new NotImplementedException($"No api call implemented to mirror {method} {uri}");
        }
    }
}