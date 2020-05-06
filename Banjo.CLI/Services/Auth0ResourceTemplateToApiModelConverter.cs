using Banjo.CLI.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Banjo.CLI.Services
{
    public class Auth0ResourceTemplateToApiModelConverter<To> : IConverter<Auth0ResourceTemplate, To>
    {
        public To Convert(Auth0ResourceTemplate @from)
        {
            return new JsonSerializer().Deserialize<To>(new JTokenReader(@from.Template));
        }
    }
}