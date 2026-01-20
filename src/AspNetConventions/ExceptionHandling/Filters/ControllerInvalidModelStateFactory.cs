using System;
using System.Linq;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspNetConventions.ExceptionHandling.Filters
{
    public sealed class ControllerInvalidModelStateFactory : IInvalidModelStateFactory
    {
        public IActionResult Create(ActionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e =>
                        string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? "Invalid value."
                            : e.ErrorMessage
                    ).ToArray()
                );

            return new BadRequestObjectResult(new ExceptionEnvelope(errors, "One or more validation errors occurred."));
        }
    }
}
