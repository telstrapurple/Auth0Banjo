using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;

namespace Banjo.CLI.TestSupport
{
    public class TextWriterConsole : IConsole
    {
        public TextWriterConsole(TextWriter output, TextWriter error)
        {
            Out = output;
            Error = error;
            In = new StringReader("");
        }

        public void ResetColor()
        {
            //no-op
        }

        public TextWriter Out { get; }
        public TextWriter Error { get; }
        public TextReader In { get; }
        public bool IsInputRedirected { get; }
        public bool IsOutputRedirected { get; }
        public bool IsErrorRedirected { get; }
        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }
        public event ConsoleCancelEventHandler CancelKeyPress;
    }
}