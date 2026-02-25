using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;

namespace AspNetConventions.Core.Abstractions.Contracts
{
    /// <summary>
    /// Defines a contract for converting response content types into standardized <see cref="ApiResult"/> objects.
    /// </summary>
    internal interface IApiResultConverter
    {
        /// <summary>
        /// Determines whether this converter can handle the specified content.
        /// </summary>
        /// <param name="content">The content to check.</param>
        /// <returns>true if this converter can handle the content; otherwise, false.</returns>
        bool CanConvert(object content);

        /// <summary>
        /// Converts the specified content into a standardized <see cref="ApiResult"/>.
        /// </summary>
        /// <param name="content">The content to convert.</param>
        /// <param name="requestDescriptor">The request descriptor containing context information.</param>
        /// <returns>A <see cref="ApiResult"/> containing the standardized response data.</returns>
        ApiResult Convert(object content, RequestDescriptor requestDescriptor);
    }
}
