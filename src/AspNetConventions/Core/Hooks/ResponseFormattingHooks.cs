using System;
using System.Threading.Tasks;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;

namespace AspNetConventions.Core.Hooks
{
    /// <summary>
    /// Provides hooks for customizing response formatting behavior during response wrapping.
    /// </summary>
    /// <remarks>
    /// This class provides fine-grained control over response formatting by offering asynchronous callbacks 
    /// at different stages of the response wrapping process.
    /// </remarks>
    public class ResponseFormattingHooks : ICloneable
    {
        /// <summary>
        /// Represents an asynchronous callback method to determine whether a response should be wrapped.
        /// </summary>
        /// <param name="requestResult">The request result containing response data.</param>
        /// <param name="requestDescriptor">The request descriptor containing context information.</param>
        /// <returns>A task that returns true if the response should be wrapped; otherwise, false.</returns>
        public delegate Task<bool> ShouldWrapResponseCallback(RequestResult requestResult, RequestDescriptor requestDescriptor);

        /// <summary>
        /// Represents an asynchronous callback method called before response wrapping.
        /// </summary>
        /// <param name="requestResult">The request result containing response data.</param>
        /// <param name="requestDescriptor">The request descriptor containing context information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public delegate Task BeforeResponseWrapCallback(RequestResult requestResult, RequestDescriptor requestDescriptor);

        /// <summary>
        /// Represents an asynchronous callback method called after response wrapping.
        /// </summary>
        /// <param name="wrappedResponse">The wrapped response object.</param>
        /// <param name="requestResult">The original request result before wrapping.</param>
        /// <param name="requestDescriptor">The request descriptor containing context information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public delegate Task AfterResponseWrapCallback(object? wrappedResponse, RequestResult requestResult, RequestDescriptor requestDescriptor);

        /// <summary>
        /// Gets or sets the asynchronous callback to determine whether a response should be wrapped.
        /// </summary>
        /// <value>A callback that returns false to skip wrapping for the specified response.</value>
        public ShouldWrapResponseCallback? ShouldWrapResponseAsync { get; set; }

        /// <summary>
        /// Gets or sets the asynchronous callback called before response wrapping.
        /// </summary>
        /// <value>A callback that allows pre-processing of responses before wrapping.</value>
        public BeforeResponseWrapCallback? BeforeResponseWrapAsync { get; set; }

        /// <summary>
        /// Gets or sets the asynchronous callback called after response wrapping.
        /// </summary>
        /// <value>A callback that allows post-processing of wrapped responses.</value>
        public AfterResponseWrapCallback? AfterResponseWrapAsync { get; set; }

        /// <summary>
        /// Creates a deep clone of <see cref="ResponseFormattingHooks"/> instance.
        /// </summary>
        /// <returns>A new <see cref="ResponseFormattingHooks"/> instance with all nested objects cloned.</returns>
        public object Clone()
        {
            return new ResponseFormattingHooks
            {
                ShouldWrapResponseAsync = ShouldWrapResponseAsync,
                BeforeResponseWrapAsync = BeforeResponseWrapAsync,
                AfterResponseWrapAsync = AfterResponseWrapAsync,
            };
        }
    }
}
