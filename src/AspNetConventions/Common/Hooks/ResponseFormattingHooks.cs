using System.Threading.Tasks;
using AspNetConventions.Common.Abstractions;
using AspNetConventions.Http;

namespace AspNetConventions.Common.Hooks
{
    public class ResponseFormattingHooks
    {
        public delegate Task<bool> ShouldWrapResponseCallback(RequestResult? requestResult, RequestDescriptor requestDescriptor);
        public delegate Task BeforeResponseWrapCallback(RequestResult requestResult, RequestDescriptor requestDescriptor);
        public delegate Task AfterResponseWrapCallback(object? wrappedResponse, RequestResult requestResult, RequestDescriptor requestDescriptor);

        /// <summary>
        /// Return false to skip wrapping for this response.
        /// </summary>
        public ShouldWrapResponseCallback? ShouldWrapResponseAsync { get; set; }

        /// <summary>
        /// Called before your response wrapper executes.
        /// </summary>
        public BeforeResponseWrapCallback? BeforeResponseWrapAsync { get; set; }

        /// <summary>
        /// Called before your response wrapper executes.
        /// </summary>
        public AfterResponseWrapCallback? AfterResponseWrapAsync { get; set; }
    }
}
