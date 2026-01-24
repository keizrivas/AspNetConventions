using System;
using Microsoft.Extensions.Logging;

namespace AspNetConventions.Configuration
{
    /// <summary>
    /// Provides configuration options for applying ASP.NET conventions, including route naming, response formatting,
    /// JSON serialization, exception handling and lifecycle hooks.
    /// </summary>
    /// <remarks>Use this class to customize the behavior of convention-based automation in ASP.NET applications.
    /// Each property exposes a set of options for a specific aspect of convention handling, allowing fine-grained
    /// control over routing, response formatting, serialization, and error handling. All option properties are
    /// initialized with default values, but can be modified as needed before use.</remarks>
    public sealed class AspNetConventionOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets the route naming convention options.
        /// </summary>
        public RouteConventionOptions Route { get; set; } = new();

        /// <summary>
        /// Gets or sets the response formatting options.
        /// </summary>
        public ResponseFormattingOptions Response { get; set; } = new();

        /// <summary>
        /// Gets or sets the JSON serialization options.
        /// </summary>
        public JsonSerializationOptions Json { get; set; } = new();

        /// <summary>
        /// Gets or sets the exception handling options.
        /// </summary>
        public ExceptionHandlingOptions ExceptionHandling { get; set; } = new();

        /// <summary>
        /// Creates a deep clone of <see cref="AspNetConventionOptions"/> instance.
        /// </summary>
        public object Clone()
        {
            return new AspNetConventionOptions
            {
                Route = (RouteConventionOptions)Route.Clone(),
                Response = (ResponseFormattingOptions)Response.Clone(),
                Json = (JsonSerializationOptions)Json.Clone(),
                ExceptionHandling = (ExceptionHandlingOptions)ExceptionHandling.Clone(),
            };
        }
    }
}
