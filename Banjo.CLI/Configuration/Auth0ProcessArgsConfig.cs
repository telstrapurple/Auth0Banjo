namespace Banjo.CLI.Configuration
{
    public class Auth0ProcessArgsConfig
    {
        public string OutputPath { get; set; }
        public string TemplateInputPath { get; set; }
        public string OverrideFilePath { get; set; }
        public bool DryRun { get; set; }
    }
}