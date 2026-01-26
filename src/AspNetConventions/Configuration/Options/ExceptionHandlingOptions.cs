using System;
using System.Collections.Generic;
using System.Net;
using AspNetConventions.Core.Hooks;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Mappers;
using AspNetConventions.Http.Services;

namespace AspNetConventions.Configuration.Options
{
    /// <summary>
    /// Provides configuration options for exception handling.
    /// </summary>
    public sealed class ExceptionHandlingOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets whether exception handling is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of custom exception mappers.
        /// </summary>
        public HashSet<IExceptionMapper> Mappers { get; private set; } = [];

        /// <summary>
        /// Gets or sets the HTTP status codes that shouldn't be handled.
        /// </summary>
        public HashSet<HttpStatusCode> ExcludeStatusCodes { get; private set; } = [];

        /// <summary>
        /// Gets or sets the Exception that shouldn't be handled.
        /// </summary>
        public HashSet<Type> ExcludeException { get; private set; } = [];

        /// <summary>
        /// Gets or sets the collection of hooks for customizing exception handling behavior.
        /// </summary>
        public ExceptionHandlingHooks Hooks { get; set; } = new();

        /// <summary>
        /// Creates a deep clone of <see cref="ExceptionHandlingOptions"/> instance.
        /// </summary>
        public object Clone()
        {
            var cloned = (ExceptionHandlingOptions)MemberwiseClone();
            cloned.Mappers = [.. Mappers];
            cloned.ExcludeException = [.. ExcludeException];
            cloned.ExcludeStatusCodes = [.. ExcludeStatusCodes];
            cloned.Hooks = (ExceptionHandlingHooks)Hooks.Clone();

            return cloned;
        }

        /// <summary>
        /// Gets the appropriate exception mapper for the given context.
        /// </summary>
        internal IExceptionMapper GetExceptionMapper(
            Exception exception,
            RequestDescriptor requestDescriptor)
        {

            // Try custom mappers first
            foreach (var mapper in Mappers)
            {
                if (mapper.CanMapException(exception, requestDescriptor))
                {
                    return mapper;
                }
            }

            // Fall back to standard mapper
            return new DefaultExceptionMapper();
        }
    }
}
