using System;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;

namespace AspNetConventions.Responses.Resolvers
{
    /// <summary>
    /// Provides a resolver that attempts to convert arbitrary data into an <see cref="ICollectionResult"/> using a set of adapters.
    /// </summary>
    /// <remarks>This resolver delegates conversion to the first adapter that can handle the provided data. If
    /// the data is already an <see cref="ICollectionResult"/>, it is returned as-is. If no adapter can handle the data, the
    /// resolver returns null.</remarks>
    internal sealed class CollectionResultResolver(AspNetConventionOptions options)
    {
        /// <summary>
        /// The AspNetConventions configuration options containing response collection adapters.
        /// </summary>
        private readonly AspNetConventionOptions _options = options ?? throw new ArgumentNullException(nameof(options));

        /// <summary>
        /// Attempts to convert the provided data into a standardized response collection.
        /// </summary>
        /// <param name="data">The data to convert into a response collection.</param>
        /// <returns>An <see cref="ICollectionResult"/> if conversion succeeds; otherwise, null.</returns>
        public ICollectionResult? TryResolve(object? data)
        {
            if (data == null)
            {
                return null;
            }

            // Already our standard type
            if (data is ICollectionResult collection)
            {
                return collection;
            }

            // Try adapters
            foreach (var adapter in _options.Response.CollectionResultAdapters)
            {
                if (adapter.CanHandle(data))
                {
                    return adapter.Convert(data);
                }
            }

            return null;
        }
    }
}
