using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.Services;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Banjo.CLI.IntegrationTests
{
    public class TestManagementConnection : IManagementConnection
    {
        
        private readonly ITestOutputHelper _output;

        public TestManagementConnection(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public async Task<T> GetAsync<T>(Uri uri, IDictionary<string, string> headers, JsonConverter[] converters = null)
        {
            _output.WriteLine("GetAsync: {0}", uri);
            _output.WriteLine("GetAsync: Headers: {0}", JsonConvert.SerializeObject(headers, Formatting.Indented));
            
            var desiredType = typeof(T);
            var desiredOpenGenericType = desiredType.GetGenericTypeDefinition();
            var pagedListOpenGeneric = typeof(PagedList<>);
            if (typeof(IPagedList<>).IsAssignableFrom(desiredOpenGenericType))
            {
                Type pagedListClosedGeneric = pagedListOpenGeneric.MakeGenericType(desiredType.GetGenericArguments());
                var constructorInfo = pagedListClosedGeneric.GetConstructor(new Type[] {});
                return (T) constructorInfo.Invoke(null);
            }
            return Activator.CreateInstance<T>();
        }

        public async Task<T> SendAsync<T>(HttpMethod method, Uri uri, object body, IDictionary<string, string> headers, IList<FileUploadParameter> files = null)
        {
            _output.WriteLine("SendAsync: {0} {1}", method, uri);
            _output.WriteLine("SendAsync: Headers: {0}", JsonConvert.SerializeObject(headers, Formatting.Indented));
            _output.WriteLine("SendAsync: Body: {0}", JsonConvert.SerializeObject(body, Formatting.Indented));
            
            var reflectorer = typeof(Reflectorisor).GetMethod(nameof(Reflectorisor.CopyMembers)).MakeGenericMethod(body.GetType(), typeof(T));
            var returnValue = (T) reflectorer.Invoke(null, new[] {body});
            return returnValue;
        }
    }
}