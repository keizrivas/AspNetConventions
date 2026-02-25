using System;
using System.Collections.Generic;
using System.Net;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetConventions.Responses.ContentConverter
{
    /// <summary>
    /// Converts ASP.NET Core <see cref="ProblemDetails"/> instances to standardized API responses.
    /// </summary>
    /// <remarks>
    /// Handles both standard <see cref="ProblemDetails"/> and <see cref="HttpValidationProblemDetails"/>,
    /// extracting appropriate data and messages based on configured options.
    /// </remarks>
    internal sealed class ProblemDetailsConverter(AspNetConventionOptions options) : IApiResultConverter
    {
        private readonly AspNetConventionOptions _options = options ?? throw new ArgumentNullException(nameof(options));

        public bool CanConvert(object? content)
            => content is ProblemDetails;

        public ApiResult Convert(object content, RequestDescriptor requestDescriptor)
        {
            var problem = (ProblemDetails)content;

            var statusCode = problem.Status.HasValue
                ? (HttpStatusCode)problem.Status.Value
                : requestDescriptor.StatusCode;

            if (statusCode != requestDescriptor.StatusCode)
            {
                requestDescriptor.SetStatusCode(statusCode);
            }

            return new ApiResult<object>(
                    value: GetProblemData(problem),
                    message: ResolveMessage(problem),
                    statusCode: statusCode,
                    type: problem is HttpValidationProblemDetails
                        ? _options.Response.ErrorResponse.DefaultValidationType
                        : _options.Response.ErrorResponse.DefaultErrorType);
        }

        /// <summary>
        /// Resolves the most appropriate message to use in the response based on the provided ProblemDetails instance and configuration options.
        /// </summary>
        /// <param name="problem">The ProblemDetails instance from which to resolve the message.</param>
        /// <returns>A string message to be included in the response.</returns>
        private string ResolveMessage(ProblemDetails problem)
        {
            if (problem is HttpValidationProblemDetails)
            {
                return _options.Response.ErrorResponse.DefaultValidationMessage;
            }

            if (!string.IsNullOrWhiteSpace(problem.Detail))
            {
                return problem.Detail;
            }

            if (!string.IsNullOrWhiteSpace(problem.Title))
            {
                return problem.Title;
            }

            return _options.Response.ErrorResponse.DefaultErrorMessage;
        }

        /// <summary>
        /// Extracts relevant data from ProblemDetails extensions based on the allowed keys configured in ErrorResponseOptions.
        /// </summary>
        /// <param name="problem">The ProblemDetails instance from which to extract data.</param>
        /// <returns>An object containing the extracted data, or null if no relevant data is found.</returns>
        private object? GetProblemData(ProblemDetails problem)
        {
            if (problem is HttpValidationProblemDetails httpValidation)
            {
                return httpValidation.Errors;
            }

            Dictionary<string, object?>? result = null;
            var allowedExtensions = _options.Response.ErrorResponse.AllowedProblemDetailsExtensions;

            // Filter extensions based on allowed list
            foreach (var extension in problem.Extensions)
            {
                if (!allowedExtensions.Contains(extension.Key))
                {
                    continue;
                }

                result ??= [];
                result[extension.Key] = extension.Value;
            }

            return result;
        }
    }
}
