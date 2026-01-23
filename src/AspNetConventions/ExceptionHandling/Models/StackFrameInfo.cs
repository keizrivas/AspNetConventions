namespace AspNetConventions.ExceptionHandling.Models
{
    /// <summary>
    /// Represents a single frame in a stack trace.
    /// </summary>
    public record StackFrameInfo(
        string Method,
        string? File,
        int? Line
    );
}
