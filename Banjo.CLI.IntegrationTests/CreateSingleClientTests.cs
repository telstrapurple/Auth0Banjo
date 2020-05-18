using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Banjo.CLI.IntegrationTests
{
    [Collection("HasEnvironmentVariables")]
    public class CreateSingleClientTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public CreateSingleClientTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task TestSomething()
        {
            var paths = new TestDataPaths(nameof(CreateSingleClientTests));
            var program = new ITProgram(_outputHelper);

            var domain = "example.com";
            Environment.SetEnvironmentVariable("AUTH0__DOMAIN", domain);
            Environment.SetEnvironmentVariable("AUTH0__CLIENTID", "clientid");
            Environment.SetEnvironmentVariable("AUTH0__CLIENTSECRET", "clientsecret");

            await program.Run(new string[]
            {
                "process",
                "-v",
                "-out", paths.OutputPath(),
                "-t", paths.TemplatesPath(),
                "-o", paths.OverridesPath("overrides.json")
            });

            A.CallTo(
                    () => program.FakeManagementConnection.SendAsync<Client>(
                        HttpMethod.Post,
                        A<Uri>.That.Matches(x => x.ToString().Contains($"https://{domain}/api/v2/clients")),
                        A<object>.That.Matches(x => (x as ClientCreateRequest).Name == "My Name Is Tones"),
                        A<Dictionary<string, string>>.That.Contains(KeyValuePair.Create("Authorization", $"Bearer {(program.TokenFactory as TestTokenFactory).Token}")),
                        A<IList<FileUploadParameter>>._))
                .MustHaveHappened();
        }
    }
}