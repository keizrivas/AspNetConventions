using System.Net;
using AspNetConventions.Http;

namespace AspNetConventions.Common.Abstractions
{
    public interface IResponseEnvelope
    {
        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        string? Message { get; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        object? Data { get; }

        /// <summary>
        /// Gets or sets response metadata.
        /// </summary>
        Metadata? Metadata { get; }

        /// <summary>
        /// Sets the data property.
        /// <param name="data">The data object to associate with this instance.</param>
        /// </summary>
        void SetData(object? data);

        /// <summary>
        /// Sets the metadata associated with this instance.
        /// </summary>
        /// <param name="metadata">The metadata object to associate with this instance.</param>
        void SetMetadata(Metadata? metadata);
    }
}
