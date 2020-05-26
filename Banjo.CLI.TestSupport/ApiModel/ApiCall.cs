using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Banjo.CLI.TestSupport.ApiModel
{
    public class ApiCall
    {
        private readonly HttpMethod _method;
        private readonly string _template;
        private readonly TemplateMatcher _matcher;
        private readonly Func<Auth0InMemoryStore, IDictionary<string, object>, object, object> _executor;

        public ApiCall(HttpMethod method, string path, Func<Auth0InMemoryStore, IDictionary<string, object>, object, object> executor)
        {
            _method = method;
            _executor = executor;
            _template = $"/api/v2/{path}";

            var template = TemplateParser.Parse(_template);
            _matcher = new TemplateMatcher(template, GetDefaults(template));
        }

        public ApiCall(HttpMethod method, string path, Func<Auth0InMemoryStore, object> executor)
            : this(method, path, (store, routeValues, input) => executor.Invoke(store))
        {
        }

        public ApiCall(HttpMethod method, string path, Func<Auth0InMemoryStore, IDictionary<string, object>, object> executor)
            : this(method, path, (store, routeValues, input) => executor.Invoke(store, routeValues))
        {
        }

        public bool IsMatch(HttpMethod method, Uri uri)
        {
            return _matcher.TryMatch(uri.AbsolutePath, new RouteValueDictionary()) && method == _method;
        }

        public IDictionary<string, object> GetMatchedRouteValues(Uri uri)
        {
            var routeValueDictionary = new RouteValueDictionary();
            if (!_matcher.TryMatch(uri.AbsolutePath, routeValueDictionary))
            {
                throw new ArgumentException($"Uri {uri} does not match template {_template}");
            }

            return routeValueDictionary;
        }

        public object Execute(Uri uri, object input, Auth0InMemoryStore store)
        {
            return _executor.Invoke(store, GetMatchedRouteValues(uri), input);
        }

        private RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
        {
            var result = new RouteValueDictionary();

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    result.Add(parameter.Name, parameter.DefaultValue);
                }
            }

            return result;
        }
    }
}