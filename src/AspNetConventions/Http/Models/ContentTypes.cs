namespace AspNetConventions.Http.Models
{
    /// <summary>
    /// Internal content types constants
    /// </summary>
    internal static class ContentTypes
    {
        public const string Any = "*/*";
        public const string Utf8 = "charset=utf-8";
        public const string Html = "text/html";
        public const string Plain = "text/plain";
        public const string Json = "application/json";
        public const string JsonUtf8 = $"application/json; {Utf8}";
        public const string JsonProblem = "application/problem+json";
        public const string jsonProblemUtf8 = $"application/problem+json; {Utf8}";
        public const string jsonProblemJson = "application/json-problem+json;";
        public const string jsonProblemJsonUtf8 = $"application/json-problem+json; {Utf8}";
    }
}
