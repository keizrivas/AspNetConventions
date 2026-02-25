using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AspNetConventions.Responses.ContentConverter
{
    /// <summary>
    /// Converts ASP.NET Core <see cref="ModelStateDictionary"/> to a standardized API response.
    /// </summary>
    /// <remarks>
    /// Wraps the ModelStateDictionary in a <see cref="ValidationProblemDetails"/> and delegates
    /// conversion to <see cref="ProblemDetailsConverter"/>.
    /// </remarks>
    internal sealed class ModelStateDictionaryConverter : IApiResultConverter
    {
        private readonly ProblemDetailsConverter _converter;

        public ModelStateDictionaryConverter(AspNetConventionOptions options)
        {
            _converter = new ProblemDetailsConverter(options);
        }

        public bool CanConvert(object content)
            => content is ModelStateDictionary;

        public ApiResult Convert(object content, RequestDescriptor requestDescriptor)
        {
            var modelState = (ModelStateDictionary)content;
            var problem = new ValidationProblemDetails(modelState);

            return _converter.Convert(problem, requestDescriptor);
        }
    }
}
