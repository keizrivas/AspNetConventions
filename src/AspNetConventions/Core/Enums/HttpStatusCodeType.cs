namespace AspNetConventions.Core.Enums
{
    /// <summary>
    /// Specifies the classification of HTTP status codes according to their response type.
    /// </summary>
    /// <remarks>Use this enumeration to categorize HTTP status codes into informational, success,
    /// redirection, client error, or server error types. This can help in handling responses based on their general
    /// meaning rather than specific numeric values.</remarks>
    public enum HttpStatusCodeType
    {
        /// <summary>
        /// Informational responses (100-199) indicating that the request was received and understood.
        /// </summary>
        Informational,

        /// <summary>
        /// Successful responses (200-299) indicating that the request was successfully received, understood, and accepted.
        /// </summary>
        Success,

        /// <summary>
        /// Redirection messages (300-399) indicating that further action needs to be taken to complete the request.
        /// </summary>
        Redirection,

        /// <summary>
        /// Client error responses (400-499) indicating that the client seems to have made an error.
        /// </summary>
        ClientError,

        /// <summary>
        /// Server error responses (500-599) indicating that the server failed to fulfill a valid request.
        /// </summary>
        ServerError
    }
}
