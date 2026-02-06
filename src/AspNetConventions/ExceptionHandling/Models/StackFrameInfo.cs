namespace AspNetConventions.ExceptionHandling.Models
{
    /// <summary>
    /// Represents a single frame in a stack trace.
    /// </summary>
    /// <param name="Method">The method name where the stack frame is located.</param>
    /// <param name="File">The source file name where the stack frame is located, if available.</param>
    /// <param name="Line">The line number in the source file, if available.</param>
    public record StackFrameInfo(
        string Method,
        string? File,
        int? Line
    );
}
