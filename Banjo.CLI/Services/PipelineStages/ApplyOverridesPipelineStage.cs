using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services.PipelineStages
{
    public class ApplyOverridesPipelineStage : IPipelineStage<Auth0ResourceTemplate>
    {
        private readonly Overrides _overrides;

        public ApplyOverridesPipelineStage(Overrides overrides)
        {
            _overrides = overrides;
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            if (t.Template == null)
            {
                throw new ArgumentNullException("t.Template", "Template property must be set on input argument t");
            }

            if (_overrides == null)
            {
                //if no overrides, no-op and return
                return t;
            }

            var candidateOverrides = t.Type.OverridesAccessor.Invoke(_overrides) ?? new List<TemplateOverride>();
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