namespace Banjo.CLI.Services
{
    public interface IConverter<From, To>
    {
        To Convert(From from);
    }
}