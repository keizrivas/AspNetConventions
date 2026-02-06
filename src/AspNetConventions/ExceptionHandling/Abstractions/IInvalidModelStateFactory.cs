using System;
using Microsoft.AspNetCore.Mvc;

namespace AspNetConventions.ExceptionHandling.Abstractions
{
    /// <summary>
    /// Defines a contract for creating standardized responses for invalid model state scenarios.
    /// </summary>
    /// <remarks>
    /// This interface provides a factory pattern for generating consistent error responses when
    /// model validation fails, ensuring that validation errors are formatted according to
    /// AspNetConventions standards across different endpoint types.
    /// </remarks>
    public interface IInvalidModelStateFactory
    {
        /// <summary>
        /// Creates an action result representing an invalid model state response.
        /// </summary>
        /// <param name="context">The action context containing model state information.</param>
        /// <returns>An <see cref="IActionResult"/> containing the formatted validation error response.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        /// <remarks>
        /// The implementation should extract validation errors from the model state and format them
        /// according to the configured error response conventions, including proper status codes
        /// and error message structure.
        /// </remarks>
        IActionResult Create(ActionContext context);
    }
}
