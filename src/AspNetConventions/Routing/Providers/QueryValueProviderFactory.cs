using System;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Providers
{
    /// <summary>
    /// Factory for creating query string value providers that apply naming conventions to query parameters.
    /// </summary>
    /// <remarks>
    /// This factory creates a <see cref="QueryValueProvider"/> that transforms query parameter names
    /// according to the configured case conventions.
    /// </remarks>
    public sealed class QueryValueProviderFactory(IOptions<AspNetConventionOptions> options) : IValueProviderFactory
    {
        /// <summary>
        /// Creates a query string value provider with naming convention support.
        /// </summary>
        /// <param name="context">The context for creating the value provider.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public async Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var request = context.ActionContext.HttpContext.Request;

            if (request.Query.Count == 0)
            {
                return;
            }

            var provider = new QueryValueProvider(options, request.Query);
            context.ValueProviders.Insert(0, provider);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }

}
