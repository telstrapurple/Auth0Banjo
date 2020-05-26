using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.TestSupport;
using Banjo.CLI.TestSupport.ApiModel;
using FakeItEasy;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Banjo.CLI.IntegrationTests
{
    [Collection("HasEnvironmentVariables")]
    public class CreateSingleClientTests
    {
        private const string ExpectedClientName = "My Name Is Tones";

        private readonly ITestOutputHelper _outputHelper;
        private readonly Auth0ConfigVariablesFixture _envVars;
        private readonly TestDataPaths _testDataPaths;

        public CreateSingleClientTests(ITestOutputHelper outputHelper, Auth0ConfigVariablesFixture envVars)
        {
            _outputHelper = outputHelper;
            _envVars = envVars;
            _testDataPaths = new TestDataPaths(nameof(CreateSingleClientTests));
        }

        [Fact]
        public async Task TestClientWouldBeCreatedWithOverriddenName()
        {
            var program = new ITProgram(_outputHelper);

            await program.Run(new[]
            {
                "process",
                "-v",
                "-out", _testDataPaths.OutputPath(),
                "-t", _testDataPaths.TemplatesPath(),
                "-o", _testDataPaths.OverridesPath("overrides.json")
            });

            A.CallTo(
                    () => program.FakeManagementConnection.SendAsync<Client>(
                        HttpMethod.Post,
                        A<Uri>.That.Matches(x => x.ToString().Contains($"https://{_envVars.Domain}/api/v2/clients")),
                        A<object>.That.Matches(x => (x as ClientCreateRequest).Name == ExpectedClientName),
                        A<Dictionary<string, string>>.That.IsAuthorized(program),
                        A<IList<FileUploadParameter>>._))
                .MustHaveHappened();
        }
        
        [Fact]
        public async Task TestClientWouldBeCreatedWithoutOverriddenName()
        {
            var program = new ITProgram(_outputHelper);

            await program.Run(new[]
            {
                "process",
                "-v",
                "-out", _testDataPaths.OutputPath(),
                "-t", _testDataPaths.TemplatesPath()
                //no overrides specified
            });

            A.CallTo(
                    () => program.FakeManagementConnection.SendAsync<Client>(
                        HttpMethod.Post,
                        A<Uri>.That.Matches(x => x.ToString().Contains($"https://{_envVars.Domain}/api/v2/clients")),
                        A<object>.That.Matches(x => (x as ClientCreateRequest).Name == "HerpaDerpaClientName"),
                        A<Dictionary<string, string>>.That.IsAuthorized(program),
                        A<IList<FileUploadParameter>>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task TestClientWouldBeCreateIfNotForDryRunFlag()
        {
            var program = new ITProgram(_outputHelper);

            await program.Run(new[]
            {
                "process",
                "-v",
                "-out", _testDataPaths.OutputPath(),
                "-t", _testDataPaths.TemplatesPath(),
                "-o", _testDataPaths.OverridesPath("overrides.json"),
                "-d"
            });

            //GET all clients
            A.CallTo(
                    () => program.FakeManagementConnection.GetAsync<IPagedList<Client>>(
                        A<Uri>.That.Matches(x => x.ToString().Contains($"https://{_envVars.Domain}/api/v2/clients")),
                        A<Dictionary<string, string>>.That.IsAuthorized(program),
                        A<JsonConverter[]>._))
                .MustHaveHappened();

            //POST (create) a client - should NOT happen due to -d dry run
            A.CallTo(
                    () => program.FakeManagementConnection.SendAsync<Client>(
                        HttpMethod.Post,
                        A<Uri>.That.Matches(x => x.ToString().Contains($"https://{_envVars.Domain}/api/v2/clients")),
                        A<object>._,
                        A<Dictionary<string, string>>._,
                        A<IList<FileUploadParameter>>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task TestClientWouldBeUpdated()
        {
            var initialClient = new Client
            {
                Name = ExpectedClientName,
                ClientId = Guid.NewGuid().ToString()
            };

            var initialStoreState = new Auth0InMemoryStore { Clients = { initialClient } };
            var program = new ITProgram(_outputHelper, initialStoreState);

            await program.Run(new[]
            {
                "process",
                "-v",
                "-out", _testDataPaths.OutputPath(),
                "-t", _testDataPaths.TemplatesPath(),
                "-o", _testDataPaths.OverridesPath("overrides.json")
            });

            //GET all clients
            A.CallTo(
                    () => program.FakeManagementConnection.GetAsync<IPagedList<Client>>(
                        A<Uri>.That.Matches(x => x.ToString().Contains($"https://{_envVars.Domain}/api/v2/clients")),
                        A<Dictionary<string, string>>.That.IsAuthorized(program),
                        A<JsonConverter[]>._))
                .MustHaveHappened();

            //PATCH (update) a specific client
            A.CallTo(
                    () => program.FakeManagementConnection.SendAsync<Client>(
                        HttpMethod.Patch, //Patch == Update
                        A<Uri>.That.Matches(x => x.ToString().Contains($"https://{_envVars.Domain}/api/v2/clients/{initialClient.ClientId}")),
                        A<object>.That.Matches(x => (x as ClientUpdateRequest).Name == ExpectedClientName),
                        A<Dictionary<string, string>>.That.IsAuthorized(program),
                        A<IList<FileUploadParameter>>._))
                .MustHaveHappened();
        }
    }

    public static class FakeItEasyMatcherExtensions
    {
        public static T IsAuthorized<T>(this IArgumentConstraintManager<T> manager, ITProgram program) where T : IEnumerable
        {
            return manager.Contains(KeyValuePair.Create("Authorization", $"Bearer {program.TokenFactory.Token}"));
        }
    }
}