using System.Collections.Generic;

namespace Banjo.CLI.TestSupport.ApiModel
{
    public static partial class ApiCalls
    {
        public static readonly List<ApiCall> AllApiCalls = new List<ApiCall> { Clients.GetAll, Clients.Get, Clients.Update, Clients.Create };
    }
}