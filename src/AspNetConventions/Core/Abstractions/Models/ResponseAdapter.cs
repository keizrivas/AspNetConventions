using System;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;

namespace AspNetConventions.Core.Abstractions.Models
{
    /// <summary>
    /// Provides a base class for building HTTP responses using ASP.NET convention options.
    /// </summary>
    /// <param name="options">The ASP.NET convention options to use when building responses.</param>
    internal abstract class ResponseAdapter(AspNetConventionOptions options) : IResponseAdapter
    {
        public AspNetConventionOptions Options => options ?? throw new ArgumentNullException(nameof(options));

        public abstract bool IsWrappedResponse(object? data);
    }
}
