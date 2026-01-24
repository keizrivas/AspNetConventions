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
        Informational,
        Success,
        Redirection,
        ClientError,
        ServerError
    }
}
