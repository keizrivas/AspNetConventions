using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetConventions.ExceptionHandling.Filters
{
    /// <summary>
    /// Factory for creating Razor Page exception filters.
    /// </summary>
    internal sealed class RazorPageExceptionFilterFactory : IFilterFactory
    {
        public bool IsReusable => false;

        /// <summary>
        /// Creates an instance of the <see cref="RazorPageExceptionFilter"/> using the provided service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies for the filter.</param>
        /// <returns>An instance of <see cref="RazorPageExceptionFilter"/>.</returns>
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<RazorPageExceptionFilter>();
        }
    }
}
