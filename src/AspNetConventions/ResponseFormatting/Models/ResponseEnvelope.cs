using System;
using System.Net;
using AspNetConventions.Common.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http;

namespace AspNetConventions.ResponseFormatting.Models
{
    /// <summary>
    /// Encapsulates a standard response structure for responses.
    /// </summary>
    /// <remarks>Use this class to provide a consistent envelope for responses.</remarks>
    public sealed class ResponseEnvelope : IResponseEnvelope
    {
        public ResponseEnvelope() { }

        public ResponseEnvelope(object? data)
        {
            Data = data;
        }

        public ResponseEnvelope(object? data, string? message)
            : this(data)
        {
            Message = message;
        }

        public ResponseEnvelope(object? data, string? message, HttpStatusCode statusCode)
            : this(data, message)
        {
            StatusCode = statusCode;
        }


        public ResponseEnvelope(object? data, HttpStatusCode statusCode)
            : this(data, null, statusCode)
        {
        }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public object? Data { get; private set; }

        /// <summary>
        /// Gets or sets response metadata.
        /// </summary>
        public Metadata? Metadata { get; private set; }

        /// <summary>
        /// Gets or sets pagination metadata.
        /// </summary>
        public PaginationMetadata? Pagination { get; private set; }

        /// <summary>
        /// Sets the data property.
        /// <param name="data">The data object to associate with this instance.</param>
        /// </summary>
        public void SetData(object? data) => Data = data;

        /// <summary>
        /// Sets the metadata associated with this instance.
        /// </summary>
        /// <param name="metadata">The metadata object to associate with this instance.</param>
        public void SetMetadata(Metadata? metadata) => Metadata = metadata;

        /// <summary>
        /// Sets the pagination associated with this instance.
        /// </summary>
        /// <param name="pagination">The pagination object to associate with this instance.</param>
        public void SetPagination(PaginationMetadata? pagination) => Pagination = pagination;

        /// <summary>
        /// Converts the specified <see cref="ExceptionEnvelope"/> to an <see cref="ResponseEnvelope"/> instance.
        /// </summary>
        /// <param name="exceptionEnvelope">Exception envelope to convert.</param>
        /// <returns>An <see cref="ResponseEnvelope"/> representing the provided exception envelope.</returns>
        public static explicit operator ResponseEnvelope(ExceptionEnvelope exceptionEnvelope)
        {
            ArgumentNullException.ThrowIfNull(exceptionEnvelope);

            var response = new ResponseEnvelope(
                exceptionEnvelope.Data,
                exceptionEnvelope.Message,
                exceptionEnvelope.StatusCode);

            response.SetMetadata(exceptionEnvelope.Metadata);

            return response;
        }

        /// <summary>
        /// Converts the specified <see cref="ExceptionEnvelope"/> to an <see cref="ResponseEnvelope"/> instance.
        /// </summary>
        /// <param name="exceptionEnvelope">Exception envelope to convert.</param>
        /// <returns>An <see cref="ResponseEnvelope"/> representing the provided exception envelope.</returns>
        public static ResponseEnvelope ToResponseEnvelope(ExceptionEnvelope exceptionEnvelope)
        {
            return (ResponseEnvelope)exceptionEnvelope;
        }
    }
}
