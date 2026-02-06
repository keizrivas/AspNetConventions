namespace AspNetConventions.Http.Models
{
    /// <summary>
    /// Provides constants for common MIME content types used throughout the application.
    /// </summary>
    /// <remarks>
    /// This class defines standardized content type strings for HTTP responses and requests.
    /// It ensures consistency in content type handling across the application and provides
    /// a centralized location for content type definitions.
    /// </remarks>
    internal static class ContentTypes
    {
        /// <summary>
        /// Represents the wildcard content type that matches any media type.
        /// </summary>
        /// <value>"*/*"</value>
        public const string Any = "*/*";

        /// <summary>
        /// Represents the UTF-8 character set declaration.
        /// </summary>
        /// <value>"charset=utf-8"</value>
        /// <remarks>This is typically appended to other content types to specify UTF-8 encoding.</remarks>
        public const string Utf8 = "charset=utf-8";

        /// <summary>
        /// Represents the HTML content type.
        /// </summary>
        /// <value>"text/html"</value>
        public const string Html = "text/html";

        /// <summary>
        /// Represents the plain text content type.
        /// </summary>
        /// <value>"text/plain"</value>
        public const string Plain = "text/plain";

        /// <summary>
        /// Represents the JSON content type.
        /// </summary>
        /// <value>"application/json"</value>
        public const string Json = "application/json";

        /// <summary>
        /// Represents the JSON content type with UTF-8 encoding.
        /// </summary>
        /// <value>"application/json; charset=utf-8"</value>
        public const string JsonUtf8 = $"application/json; {Utf8}";

        /// <summary>
        /// Represents the JSON Problem Details content type.
        /// </summary>
        /// <value>"application/problem+json"</value>
        /// <remarks>This content type is used for RFC 7807 Problem Details for HTTP APIs.</remarks>
        public const string JsonProblem = "application/problem+json";

        /// <summary>
        /// Represents the JSON Problem Details content type with UTF-8 encoding.
        /// </summary>
        /// <value>"application/problem+json; charset=utf-8"</value>
        public const string jsonProblemUtf8 = $"application/problem+json; {Utf8}";

        /// <summary>
        /// Represents the custom JSON Problem Details content type.
        /// </summary>
        /// <value>"application/json-problem+json;"</value>
        /// <remarks>This is a custom content type for extended problem details format.</remarks>
        public const string jsonProblemJson = "application/json-problem+json;";

        /// <summary>
        /// Represents the custom JSON Problem Details content type with UTF-8 encoding.
        /// </summary>
        /// <value>"application/json-problem+json; charset=utf-8"</value>
        /// <remarks>This is a custom content type for extended problem details format with UTF-8 encoding.</remarks>
        public const string jsonProblemJsonUtf8 = $"application/json-problem+json; {Utf8}";
    }
}
