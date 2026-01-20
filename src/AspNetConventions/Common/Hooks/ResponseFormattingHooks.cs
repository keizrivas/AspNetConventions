using System.Threading.Tasks;
using AspNetConventions.Common.Abstractions;
using AspNetConventions.Http;

namespace AspNetConventions.Common.Hooks
{
    public class ResponseFormattingHooks
    {
        public delegate Task<bool> ShouldWrapResponseCallback(IResponseEnvelope? response, RequestDescriptor requestDescriptor);
        public delegate Task BeforeResponseWrapCallback(IResponseEnvelope responseEnvelope, RequestDescriptor requestDescriptor);
        public delegate Task AfterResponseWrapCallback(object? wrappedResponse, IResponseEnvelope responseEnvelope, RequestDescriptor requestDescriptor);

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
