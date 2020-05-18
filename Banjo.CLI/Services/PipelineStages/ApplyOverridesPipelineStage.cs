using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services.PipelineStages
{
    public class ApplyOverridesPipelineStage : IPipelineStage<Auth0ResourceTemplate>
    {
        public string Name { get; } = "Apply Overrides";

        private readonly IOverridesSource _overridesSource;

        public ApplyOverridesPipelineStage(IOverridesSource overridesSource)
        {
            _overridesSource = overridesSource;
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            if (t.Template == null)
            {
                throw new ArgumentNullException("t.Template", "Template property must be set on input argument t");
            }

            if (_overridesSource == null)
            {
                //if no overrides, no-op and return
                return t;
            }

            var rootOverrides = await _overridesSource.GetOverridesAsync();
            t.Overrides = rootOverrides;

            var candidateOverrides = t.Type.OverridesAccessor.Invoke(rootOverrides) ?? new List<TemplateOverride>();
            var overrides = candidateOverrides.Where(x => x.TemplateName == t.Filename).SelectMany(x => x.Overrides).ToList();
            foreach (var @override in overrides)
            {
                var token = t.Template.SelectToken(@override.Path, true);
                token.Replace(@override.Replacement);
            }

            t.OverridesApplied = true;

            return t;
        }
    }
}