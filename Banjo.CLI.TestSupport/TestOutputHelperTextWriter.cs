using System;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Banjo.CLI.IntegrationTests
{
    public class TestOutputHelperTextWriter : TextWriter
    {
        private readonly StringBuilder _allOutputBuffer = new StringBuilder();
        private readonly StringBuilder _buffer = new StringBuilder();
        
        public string AllOutput => _allOutputBuffer.ToString();
        
        public override Encoding Encoding { get; } = Encoding.UTF8;
        
        private readonly ITestOutputHelper _outputHelper;

        public TestOutputHelperTextWriter(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer.Length == 2 && buffer[0] == '\r' && buffer[1] == '\n')
            {
                _outputHelper.WriteLine(_buffer.ToString());
                _allOutputBuffer.Append(_buffer.ToString()).Append("\r\n");
                _buffer.Clear();
            } else
            {
                _buffer.Append(buffer, index, count);
            }
        }
    }
}