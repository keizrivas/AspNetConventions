using System;
using System.Threading.Tasks;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http.Services;

namespace AspNetConventions.Core.Hooks
{
    /// <summary>
    /// Provides hooks for customizing the exception handling pipeline by allowing injection of user-defined
    /// logic at stages of exception processing.
    /// </summary>
    /// <remarks>
    /// Use this class to register delegates that are invoked before, during, and after exception handling.
    /// </remarks>
    public class ExceptionHandlingHooks : ICloneable
    {
        /// <summary>
        /// Represents an asynchronous callback method to attempt handling an exception.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        public delegate Task TryHandleCallbackAsync(Exception exception);

        /// <summary>
        /// Represents an asynchronous callback method to determine whether an exception should be handled.
        /// </summary>
        /// <param name="exception">The exception to evaluate.</param>
        /// <param name="requestDescriptor">The request descriptor for the current request.</param>
        /// <returns>A task that returns true if the exception should be handled; otherwise, false.</returns>
        public delegate Task<bool> ShouldHandleCallbackAsync(Exception exception, RequestDescriptor requestDescriptor);

        /// <summary>
        /// Represents an asynchronous callback method called before mapping an exception.
        /// </summary>
        /// <param name="mapper">The exception mapper to be used.</param>
        /// <param name="requestDescriptor">The request descriptor for the current request.</param>
        /// <returns>A task that returns the (possibly modified) exception mapper.</returns>
        public delegate Task<IExceptionMapper> BeforeMappingCallbackAsync(IExceptionMapper mapper, RequestDescriptor requestDescriptor);

        /// <summary>
        /// Represents an asynchronous callback method called after mapping an exception.
        /// </summary>
        /// <param name="exceptionDescriptor">The exception descriptor produced by the mapper.</param>
        /// <param name="mapper">The exception mapper that was used.</param>
        /// <param name="requestDescriptor">The request descriptor for the current request.</param>
        /// <returns>A task that returns the (possibly modified) exception descriptor.</returns>
        public delegate Task<ExceptionDescriptor> AfterMappingCallbackAsync(ExceptionDescriptor exceptionDescriptor, IExceptionMapper mapper, RequestDescriptor requestDescriptor);

        /// <summary>
        /// This method handles the entire pipeline.
        /// </summary>
        /// <value>The callback that attempts to handle an exception.</value>
        public TryHandleCallbackAsync? TryHandleAsync { get; set; }

        /// <summary>
        /// Gets or sets the callback that determines whether a given result or exception should be handled by the
        /// policy.
        /// </summary>
        /// <value>The callback that returns false to skip handling for the specified exception.</value>
        public ShouldHandleCallbackAsync? ShouldHandleAsync { get; set; }

        /// <summary>
        /// Allows you to override the behavior before the mapper produced a result.
        /// </summary>
        /// <value>The callback invoked before mapping an exception.</value>
        public BeforeMappingCallbackAsync? BeforeMappingAsync { get; set; }

        /// <summary>
        /// Allows you to override the behavior after the mapper produced a result.
        /// </summary>
        /// <value>The callback invoked after mapping an exception.</value>
        public AfterMappingCallbackAsync? AfterMappingAsync { get; set; }

        /// <summary>
        /// Creates a deep clone of <see cref="ExceptionHandlingHooks"/> instance.
        /// </summary>
        /// <returns>A new <see cref="ExceptionHandlingHooks"/> instance with all nested objects cloned.</returns>
        public object Clone()
        {
            return new ExceptionHandlingHooks
            {
                TryHandleAsync = TryHandleAsync,
                ShouldHandleAsync = ShouldHandleAsync,
                BeforeMappingAsync = BeforeMappingAsync,
                AfterMappingAsync = AfterMappingAsync,
            };
        }
    }
}
