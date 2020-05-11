#nullable enable
using System.Linq;
using System.Reflection;

namespace Banjo.CLI.Services
{
    public class Reflectorisor
    {
        /// <summary>
        /// Copy properties from an instance of one type to a new instance of another type.
        /// Matches properties by name, then by return/parameter type. That is, the setter has to have exactly one
        /// parameter and its type has to match the getters return type.
        /// Requires a public setter on the <c>To</c> property and a public getter on the <c>From</c> property.
        /// Only performs reference copy, does not deep copy or clone the property values.
        /// Creates a new instance of the <c>To</c> type.
        /// </summary>
        /// <param name="from">the instance to copy properties from</param>
        /// <typeparam name="TFrom">The type of the instance to copy properties from</typeparam>
        /// <typeparam name="TTo">The type of the required return type</typeparam>
        /// <returns>A new instance of the To type with the matching properties copied from <c>from</c></returns>
        public static TTo CopyMembers<TFrom, TTo>(TFrom from) where TTo : new()
        {
            var to = new TTo();

            foreach (var property in typeof(TTo).GetProperties().ToList())
            {
                MethodInfo? getter = GetPublicGetter<TFrom>(property.Name);
                if (getter == null) continue;

                MethodInfo? setter = GetPublicSetter<TTo>(getter, property.Name);
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