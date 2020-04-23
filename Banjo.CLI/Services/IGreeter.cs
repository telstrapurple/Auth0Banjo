namespace Banjo.CLI.Services
{
    public interface IGreeter
    {
        string GetHello(string name);
    }

    public class BanjoGreeter : IGreeter
    {
        public string GetHello(string name)
        {
            return $"Hello {name}";
        }
    }
}