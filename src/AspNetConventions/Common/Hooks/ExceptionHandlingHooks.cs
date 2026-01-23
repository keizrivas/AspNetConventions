using System;
using System.Threading.Tasks;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http;

namespace AspNetConventions.Common.Hooks
{
    /// <summary>
    /// Provides hooks for customizing the exception handling pipeline by allowing injection of user-defined
    /// logic at stages of exception processing.
    /// </summary>
    /// <remarks>Use this class to register delegates that are invoked before, during, and after exception handling.
    /// </remarks>
    public class ExceptionHandlingHooks
    {
        public delegate Task TryHandleCallbackAsync(Exception exception);
        public delegate Task<bool> ShouldHandleCallbackAsync(Exception exception, RequestDescriptor requestDescriptor);
        public delegate Task<IExceptionMapper> BeforeMappingCallbackAsync(IExceptionMapper mapper, RequestResult requestResult, RequestDescriptor requestDescriptor);
        public delegate Task<ExceptionDescriptor2> AfterMappingCallbackAsync(ExceptionDescriptor2 envelope, IExceptionMapper mapper, RequestResult requestResult, RequestDescriptor requestDescriptor);

        /// <summary>
        /// This method handles the entire pipeline.
        /// </summary>
        public TryHandleCallbackAsync? TryHandleAsync { get; set; }

        /// <summary>
        /// Gets or sets the callback that determines whether a given result or exception should be handled by the
        /// policy.
        /// </summary>
        public ShouldHandleCallbackAsync? ShouldHandleAsync { get; set; }

        /// <summary>
        /// Allows you to override the behavior before the mapper produced a result.
        /// </summary>
        public BeforeMappingCallbackAsync? BeforeMappingAsync { get; set; }

        /// <summary>
        /// Allows you to override the behavior after the mapper produced a result.
        /// </summary>
        public AfterMappingCallbackAsync? AfterMappingAsync { get; set; }
    }
}
