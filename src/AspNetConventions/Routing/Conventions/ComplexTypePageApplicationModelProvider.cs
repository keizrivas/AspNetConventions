using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Routing.ModelBinding;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Conventions
{
    /// <summary>
    /// Scans Razor Pages at startup and pre-fills <see cref="ComplexTypeBindingMetadataProvider"/> cache
    /// with every complex-type parameter found on page handler methods.
    /// </summary>
    internal sealed class ComplexTypePageApplicationModelProvider(
        IOptions<AspNetConventionOptions> options,
        ILogger<ComplexTypeBindingMetadataProvider> logger)
        : ConventionOptions(options), IPageApplicationModelProvider
    {
        private readonly ComplexTypeBindingMetadataProvider _bindingMetadataProvider = new(
            options,
            logger ?? NullLogger<ComplexTypeBindingMetadataProvider>.Instance);

        /// <summary>
        /// Runs after the default Razor Pages model provider
        /// </summary>
        public int Order => 1;

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
            CreateOptionSnapshot();

            if (!Options.Route.IsEnabled)
                return;

            var pageModel = context.PageApplicationModel;

            foreach (var handler in pageModel.HandlerMethods)
            {
                foreach (var parameter in handler.Parameters)
                {
                    // Skip explicit [FromBody] or [FromServices] even on Razor Pages
                    var bindingSource = parameter.BindingInfo?.BindingSource;
                    if (bindingSource == BindingSource.Body ||
                        bindingSource == BindingSource.Services)
                    {
                        continue;
                    }

                    // Only process complex types with a container (Razor page model container)
                    var bindingContext = BindingDescriptor.GetBindingContext(parameter);
                    if (!bindingContext.IsComplexType || bindingContext.ContainerType == null)
                    {
                        continue;
                    }

                    // Cache complex type metadata for this parameter to optimize binding at runtime
                    _bindingMetadataProvider.CacheComplexType(bindingContext);
                }
            }
        }

        public void OnProvidersExecuted(PageApplicationModelProviderContext context) { }
    }
}
