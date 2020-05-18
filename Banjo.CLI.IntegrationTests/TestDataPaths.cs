namespace Banjo.CLI.IntegrationTests
{
    public class TestDataPaths
    {
        private readonly string _testName;

        public TestDataPaths(string testName)
        {
            _testName = testName;
        }
        
        public string OverridesPath(string overridesFilename)
        {
            return $"./TestData/{_testName}/overrides/{overridesFilename}";
        }
        
        public string TemplatesPath()
        {
            return $"./TestData/{_testName}/templates/";
        }
        
        public string OutputPath()
        {
            return $"./TestDataOutput/{_testName}/";
        }
    }
}