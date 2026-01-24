using System;
using System.Collections.Generic;
using AspNetConventions.Core.Abstractions.Contracts;

namespace AspNetConventions.Responses.Resolvers
{
    /// <summary>
    /// Provides a resolver that attempts to convert arbitrary data into an IResponseCollection using a set of adapters.
    /// </summary>
    /// <remarks>This resolver delegates conversion to the first adapter that can handle the provided data. If
    /// the data is already an IResponseCollection, it is returned as-is. If no adapter can handle the data, the
    /// resolver returns null.</remarks>
    /// <param name="adapters">The collection of adapters used to convert data to IResponseCollection instances.</param>
    public sealed class ResponseCollectionResolver(IEnumerable<IResponseCollectionAdapter> adapters) : IResponseCollectionResolver
    {
        private readonly IEnumerable<IResponseCollectionAdapter> _adapters = adapters ?? throw new ArgumentNullException(nameof(adapters));

        public IResponseCollection? TryResolve(object data)
        {
            ArgumentNullException.ThrowIfNull(data);

            // Already our standard type
            if (data is IResponseCollection collection)
            {
                return collection;
            }

            // Try adapters
            foreach (var adapter in _adapters)
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
