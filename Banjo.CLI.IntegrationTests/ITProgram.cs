using System;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Autofac;
using Banjo.CLI.Configuration;
using Banjo.CLI.Model;
using Banjo.CLI.Services;
using Banjo.CLI.Services.PipelineStages;
using Banjo.CLI.TestSupport;
using FakeItEasy;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Banjo.CLI.IntegrationTests
{
    public class ITProgram : Program
    {
        public ITestOutputHelper OutputHelper { get; set; }
        public TestOutputHelperTextWriter StandardOutput { get; set; }
        public TestOutputHelperTextWriter StandardError { get; set; }
        public IAuth0TokenFactory TokenFactory { get; set; }
        public IOverridesSource OverridesSource { get; set; }
        public IManagementConnection FakeManagementConnection { get; set; }

        public ITemplateSource TemplateSource { get; set; }
        // public TestWriteOutputPipelineStage OutputStage {get; set;} //will we even need to deal with this in tests?

        private readonly Action<ContainerBuilder>[] _builders;

        public ITProgram(ITestOutputHelper outputter, params Action<ContainerBuilder>[] builders)
        {
            OutputHelper = outputter;
            _builders = builders;

            StandardOutput = new TestOutputHelperTextWriter(outputter);
            StandardError = new TestOutputHelperTextWriter(outputter);
            TokenFactory = new TestTokenFactory { Token = "TestToken" };
            OverridesSource = new TestOverridesSource();
            TemplateSource = new TestTemplateSource();
            
            var realManagementConnection = new TestManagementConnection(outputter);
            var fakeManagementConnection = A.Fake<IManagementConnection>(o => o.Wrapping(realManagementConnection));
            A.CallTo(fakeManagementConnection).CallsWrappedMethod();
            
            FakeManagementConnection = fakeManagementConnection;
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            var hostBuilder = base.CreateHostBuilder();
            hostBuilder.ConfigureContainer<ContainerBuilder>(container =>
            {
                container.Register(context => new TextWriterConsole(StandardOutput, StandardError)).AsImplementedInterfaces().SingleInstance();
                container.Register(context => TokenFactory).AsImplementedInterfaces().SingleInstance();
                // container.Register(context => OverridesSource).AsImplementedInterfaces();
                container.Register(context => FakeManagementConnection).AsImplementedInterfaces();
                // container.Register(context => TemplateSource).AsImplementedInterfaces();
                // container.RegisterType<TestWriteOutputPipelineStage>().As<WriteOutputPipelineStage>();
                // container.RegisterType<NoOpTemplateReaderStage>().As<TemplateReaderPipelineStage>();

                foreach (var builder in _builders)
                {
                    builder.Invoke(container);
                }
            });

            return hostBuilder;
        }
    }

    public class NoOpTemplateReaderStage : TemplateReaderPipelineStage
    {
        public override async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            return t;
        }
    }

    public class TestWriteOutputPipelineStage : WriteOutputPipelineStage
    {
        private readonly IReporter _reporter;

        public TestWriteOutputPipelineStage(IReporter reporter, IOptionsMonitor<Auth0ProcessArgsConfig> args) : base(reporter, args)
        {
            _reporter = reporter;
        }

        public override async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            _reporter.Output(JsonConvert.SerializeObject(t.Template, Formatting.Indented));
            return t;
        }
    }
}