using System;

namespace Banjo.CLI.Services
{
    [Obsolete("demo service, can be deleted")]
    public interface IGreeter
    {
        string GetHello(string name);
    }

    [Obsolete("demo service, can be deleted")]
    public class BanjoGreeter : IGreeter
    {
        public string GetHello(string name)
        {
            return $"Hello {name}";
        }
    }
}