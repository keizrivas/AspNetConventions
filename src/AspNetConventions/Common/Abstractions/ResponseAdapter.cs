using AspNetConventions.Configuration;

namespace AspNetConventions.Common.Abstractions
{
    /// <summary>
    /// Provides a base class for building HTTP responses using ASP.NET convention options.
    /// </summary>
    /// <param name="options">The ASP.NET convention options to use when building responses.</param>
    internal abstract class ResponseAdapter(AspNetConventionOptions options): IResponseAdapter
    {
        public AspNetConventionOptions Options => options;
        
        public abstract bool IsWrappedResponse(object? data);
    }
}
