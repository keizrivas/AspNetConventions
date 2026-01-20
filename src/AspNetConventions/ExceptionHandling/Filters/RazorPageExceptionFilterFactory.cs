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

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<RazorPageExceptionFilter>();
        }
    }
}
