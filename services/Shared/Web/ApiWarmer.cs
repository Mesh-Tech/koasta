using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Koasta.Shared.Web
{
    public class ApiWarmer : IDisposable
    {
        private readonly HttpClient httpClient;

        public ApiWarmer()
        {
            httpClient = new HttpClient();
        }

        public async Task WarmUp(Type startupType, IUrlHelper urlHelper, HttpRequest request, params string[] ignoredControllers)
        {
            var controllerTypes =
                startupType.GetTypeInfo().Assembly
                    .GetTypes()
                    .Where(t => t.GetTypeInfo().IsClass
                        && (t.Name.EndsWith("Controller", StringComparison.Ordinal)
                        || t.GetTypeInfo().BaseType == typeof(Controller))
                        && !ignoredControllers.Any(i => t.Name.StartsWith(i, StringComparison.Ordinal)))
                    .ToList();

            var getEndpoints =
                controllerTypes
                    .SelectMany(c => c.GetMethods()
                        .Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(HttpGetAttribute))));

            foreach (var method in getEndpoints)
            {
                var routeValues = new RouteValueDictionary();

                method.GetParameters()
                    .ToDictionary(
                        p => p.Name,
                        p => GetDefault(p.ParameterType))
                    .ToList()
                    .ForEach(rv => routeValues.Add(rv.Key, rv.Value));

                var url = GetFullUrl(
                    request,
                    urlHelper.Action(
                        method.Name,
                        GetControllerName(method.DeclaringType.Name),
                        routeValues
                ));

                Console.WriteLine($"Warming up endpoint: {url}");

                await httpClient.GetAsync(new Uri(url)).ConfigureAwait(false);
            }
        }

        private string GetControllerName(string className) =>
            className.EndsWith("Controller", StringComparison.Ordinal) ? className.Substring(0, className.Length - "Controller".Length) : className;

        public static object GetDefault(Type type) =>
            type == typeof(string) ?
                "foo" : type.GetTypeInfo().IsValueType ?
                Activator.CreateInstance(type) : null;

        private string GetFullUrl(HttpRequest request, string relativeUrl) =>
            $"{request.Scheme}://{request.Host}{relativeUrl}";

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                httpClient?.Dispose();
            }
        }
    }
}
