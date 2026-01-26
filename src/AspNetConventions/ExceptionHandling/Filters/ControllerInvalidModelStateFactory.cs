using System;
using System.Collections.Generic;
using System.Linq;
using AspNetConventions.Configuration.Options;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetConventions.ExceptionHandling.Filters
{
    public sealed class ControllerInvalidModelStateFactory(IOptions<AspNetConventionOptions> options) : IInvalidModelStateFactory
    {
        public IActionResult Create(ActionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var errors = new Dictionary<string, HashSet<string>>(
                context.ModelState.Count,
                StringComparer.Ordinal);

            foreach (var entry in context.ModelState)
            {
                var modelState = entry.Value;
                if (modelState == null || modelState.Errors.Count == 0)
                    continue;

                var messages = new HashSet<string>(StringComparer.Ordinal);

                foreach (var error in modelState.Errors)
                {
                    var message = string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "Invalid value."
                        : error.ErrorMessage;

                    messages.Add(message);
                }

                if (messages.Count > 0)
                {
                    errors[entry.Key] = messages;
                }
            }

            return new BadRequestObjectResult(new ExceptionDescriptor
            {
                Data = errors,
                Message = options.Value.Response.ErrorResponse.DefaultValidationMessage,
                LogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
            });
        }
    }
}
