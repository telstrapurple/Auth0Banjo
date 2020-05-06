using System.Linq;
using System.Reflection;

namespace Banjo.CLI.Services
{
    public class Reflectorisor
    {
        public static To CopyMembers<From, To>(From from) where To : new()
        {
            var to = new To();

            foreach (var property in typeof(To).GetProperties().ToList())
            {
                MethodInfo? getter = GetPublicGetter<From>(property.Name);
                if (getter == null) continue;

                MethodInfo? setter = GetPublicSetter<To>(getter, property.Name);
                if (setter == null) continue;

                var result = getter.Invoke(from, null);
                setter.Invoke(to, new[] { result });
            }

            return to;
        }

        private static MethodInfo? GetPublicGetter<T>(string name)
        {
            return typeof(T).GetProperty(name)?.GetGetMethod();
        }

        private static MethodInfo? GetPublicSetter<T>(MethodInfo? getter, string name)
        {
            if (getter == null) return null;
            var matchingProperty = typeof(T).GetProperty(name);
            var candidateSetter = matchingProperty?.GetSetMethod();
            if (candidateSetter == null) return null;
            var setterParams = candidateSetter.GetParameters();
            if (setterParams.Length != 1) return null;
            return setterParams[0].ParameterType == getter.ReturnType ? candidateSetter : null;
        }
    }
}