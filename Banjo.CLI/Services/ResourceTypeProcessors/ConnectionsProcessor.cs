using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Banjo.CLI.Services.ResourceTypeProcessors
{
    public class ConnectionsProcessor : AbstractSingleTypeResourceTypeProcessor<Connection>
    {
        protected override ResourceType Type { get; } = ResourceType.Connections;

        private readonly IOptionsMonitor<Auth0ProcessArgsConfig> _args;
        private readonly ManagementApiClientFactory _managementApiClientFactory;
        private readonly IConverter<Auth0ResourceTemplate, Connection> _converter;
        private readonly IReporter _reporter;

        public ConnectionsProcessor(
            IOptionsMonitor<Auth0ProcessArgsConfig> args,
            IConverter<Auth0ResourceTemplate, Connection> converter,
            IReporter reporter,
            ManagementApiClientFactory managementApiClientFactory)
            : base(args, converter, reporter)
        {
            _args = args;
            _managementApiClientFactory = managementApiClientFactory;
            _converter = converter;
            _reporter = reporter;
        }

        //internal class to more easily pull out the enabled_clients_match_conditions than working with the JToken api directly.
        private class ConnectionClientMatchConditions
        {
            [JsonProperty("enabled_clients_match_conditions")]
            public IEnumerable<string> EnabledClientsMatchConditions { get; set; }
        }

        public override async Task Preprocess(Auth0ResourceTemplate template)
        {
            //Connection templates have a property enabled_clients_match_conditions that contains regexes or string
            //literals of client names that the connection should be associated with.
            //This preprocessing step looks up all the deployed clients and 
            
            var matchConditions = new JsonSerializer().Deserialize<ConnectionClientMatchConditions>(
                    new JTokenReader(template.Template))
                ?.EnabledClientsMatchConditions?.ToList() ?? new List<string>();

            if (matchConditions.Count == 0)
            {
                template.Preprocessed = true;
                return;
            }

            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var getClientsRequest = new GetClientsRequest()
            {
                IsGlobal = false, IncludeFields = true, Fields = "name,client_id"
            };
            var clients = await managementClient.Clients.GetAllAsync(getClientsRequest, new PaginationInfo());

            var matchConditionsRegexes = matchConditions.Select(x => new Regex(x));
            var matchingClientIds = clients.Where(x =>
                //check for exact string match OR regex match
                matchConditionsRegexes.Any(regex => string.Equals(regex.ToString(), x.Name) || regex.IsMatch(x.Name))
            ).Select(x => (object) new JValue(x.ClientId)).ToList();

            if (!(template.Template is JObject t))
            {
                throw new InvalidOperationException(
                    $"{Type.Name} template {template.Filename} processed type is not of type JObject." +
                    $" Found {template.Template.GetType().Name}");
            }

            //add the enabled_clients
            t.Add("enabled_clients", new JArray(matchingClientIds.ToArray()));

            //remove the enabled_clients_match_conditions
            t.Remove("enabled_clients_match_conditions");
            
            if (_args.CurrentValue.DryRun)
            {
                _reporter.Warn(
                    "Dry-run flag is set. Any clients that do not exist but that will be created by " +
                    "these templates when run without the dry-run flag may not be found and included in this " +
                    "connections\' enabled_clients list. The complete list of matching clients will be found when " +
                    "run without the dry-run flag.");
            }

            template.Preprocessed = true;
        }

        public override async Task ProcessAsync(Auth0ResourceTemplate template)
        {
            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var templatedConnection = _converter.Convert(template);

            // //todo support proper pagination - how to do this where every api call is different?!
            var getConnectionsRequest = new GetConnectionsRequest
            {
                IncludeFields = true,
                Fields = "name,id"
            };
            var allConnections = await managementClient.Connections.GetAllAsync(getConnectionsRequest, new PaginationInfo());

            var matchingConnection = allConnections.FirstOrDefault(x => string.Equals(x.Name, templatedConnection.Name));
            if (matchingConnection == null)
            {
                var createRequest = Reflectorisor.CopyMembers<Connection, ConnectionCreateRequest>(templatedConnection);
                await Create(
                    async () => await managementClient.Connections.CreateAsync(createRequest),
                    request => request.Id,
                    templatedConnection.Name
                );
            }
            else
            {
                var updateRequest = Reflectorisor.CopyMembers<Connection, ConnectionUpdateRequest>(templatedConnection);
                //Remove the name property. It's not allowed on an update.
                updateRequest.Name = null;
                
                await Update(
                    async () => await managementClient.Connections.UpdateAsync(matchingConnection.Id, updateRequest),
                    matchingConnection.Id,
                    templatedConnection.Name
                );
            }
        }
    }
}