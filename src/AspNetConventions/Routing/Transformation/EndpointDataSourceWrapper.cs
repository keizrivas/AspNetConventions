using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace AspNetConventions.Routing.Transformation
{
    /// <summary>
    /// Wraps an endpoint data source with transformed endpoints.
    /// </summary>
    internal sealed class EndpointDataSourceWrapper : EndpointDataSource
    {
        private readonly IReadOnlyList<Endpoint> _endpoints;
        private readonly IChangeToken _changeToken;

        public EndpointDataSourceWrapper(
            List<Endpoint> endpoints,
            IChangeToken changeToken)
        {
            _endpoints = endpoints ?? throw new ArgumentNullException(nameof(endpoints));
            _changeToken = changeToken ?? throw new ArgumentNullException(nameof(changeToken));
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Endpoint> Endpoints => _endpoints;

        /// <inheritdoc/>
        public override IChangeToken GetChangeToken() => _changeToken;
    }
}
