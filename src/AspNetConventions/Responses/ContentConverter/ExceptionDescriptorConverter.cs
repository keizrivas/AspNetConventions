using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;

namespace AspNetConventions.Responses.ContentConverter
{
    /// <summary>
    /// Converts <see cref="ExceptionDescriptor"/> instances to standardized API responses.
    /// </summary>
    /// <remarks>
    /// Extracts the exception's value, type, message, and status code to create
    /// a consistent error response format.
    /// </remarks>
    internal sealed class ExceptionDescriptorConverter : IApiResultConverter
    {
        public bool CanConvert(object content)
            => content is ExceptionDescriptor;

        public ApiResult Convert(object content, RequestDescriptor requestDescriptor)
        {
            var exception = (ExceptionDescriptor)content;

            var statusCode = exception.StatusCode ?? requestDescriptor.StatusCode;
            if (statusCode != requestDescriptor.StatusCode)
            {
                requestDescriptor.SetStatusCode(statusCode);
            }

            return new ApiResult<object>(
                value: exception.Value,
                type: exception.Type,
                message: exception.Message,
                statusCode: statusCode);
        }
    }
}
