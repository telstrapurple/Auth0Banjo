using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Banjo.CLI.Services.ResourceTypeProcessors
{
    public class RulesProcessor : AbstractSingleTypeResourceTypeProcessor<Rule>
    {
        protected override ResourceType Type { get; } = ResourceType.Rules;

        private readonly ManagementApiClientFactory _managementApiClientFactory;
        private readonly IConverter<Auth0ResourceTemplate, Rule> _converter;

        public RulesProcessor(
            IOptionsMonitor<Auth0ProcessArgsConfig> args,
            IConverter<Auth0ResourceTemplate, Rule> converter,
            IReporter reporter, ManagementApiClientFactory managementApiClientFactory)
            : base(args, converter, reporter)
        {
            _managementApiClientFactory = managementApiClientFactory;
            _converter = converter;
        }

        public override async Task Preprocess(Auth0ResourceTemplate template)
        {
            var scriptToken = template.Template.SelectTokens("script").OfType<JValue>().FirstOrDefault(x => x.Type == JTokenType.String);
            if (scriptToken == null)
            {
                throw new Auth0InvalidTemplateException(
                    $"{Type.Name} template {template.Filename} does not contain a required property \'script\' " +
                    $"of type String.");
            }

            var scriptFilename = scriptToken.Value as string;
            if (string.IsNullOrWhiteSpace(scriptFilename))
            {
                throw new Auth0InvalidTemplateException(
                    $"{Type.Name} template {template.Filename} does not contain a valid value for property " +
                    $"\'script\'. Must not be null or empty.");
            }

            var scriptPath = Path.Combine(Directory.GetParent(template.Location.FullName).FullName, scriptFilename);
            if (!File.Exists(scriptPath))
            {
                throw new Auth0InvalidTemplateException(
                    $"{Type.Name} template {template.Filename} does not contain a valid value for property " +
                    $"\'script\'. The file must exist but does not: {scriptPath}");
            }

            scriptToken.Replace(await File.ReadAllTextAsync(scriptPath));
        }

        public override async Task ProcessAsync(Auth0ResourceTemplate template)
        {
            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var rule = _converter.Convert(template);

            //todo support proper pagination - how to do this where every api call is different?!
            var results = await managementClient.Rules.GetAllAsync(new GetRulesRequest(), new PaginationInfo());

            var matchingRule = results.FirstOrDefault(x => string.Equals(x.Name, rule.Name));
            if (matchingRule == null)
            {
                var createRequest = Reflectorisor.CopyMembers<Rule, RuleCreateRequest>(rule);
                await Create(
                    async () => await managementClient.Rules.CreateAsync(createRequest),
                    request => request.Id,
                    rule.Name);
            }
            else
            {
                var updateRequest = Reflectorisor.CopyMembers<Rule, RuleUpdateRequest>(rule);
                await Update(
                    async () => await managementClient.Rules.UpdateAsync(matchingRule.Id, updateRequest),
                    matchingRule.Id,
                    rule.Name
                );
            }
        }
    }
}