using System.IO;

namespace Banjo.CLI.Model
{
    public class TemplateMetadata
    {
        public ResourceType Type { get; set; }
        public FileInfo Location { get; set; }
    }
}