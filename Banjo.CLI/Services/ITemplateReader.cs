using System;
using System.Threading.Tasks;
using Banjo.CLI.Model;

namespace Banjo.CLI.Services
{
    [Obsolete("Needs to be refactored to handle the result of applying the overrides and token replacements")]
    public interface ITemplateReader<T>
    {
        Task<T> ReadTemplateContents(TemplateMetadata templateMetadata);
    }
}