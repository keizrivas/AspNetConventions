using Microsoft.AspNetCore.Mvc;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Provides extension methods for extracting content from ASP.NET Core action results.
    /// </summary>
    /// <remarks>
    /// These methods simplify the process of retrieving the underlying data from various
    /// types of action results, providing a unified interface for content extraction.
    /// </remarks>
    internal static class ActionResultExtensions
    {
        /// <summary>
        /// Extracts the content from an action result, regardless of its specific type.
        /// </summary>
        /// <param name="result">The action result to extract content from, or null.</param>
        /// <returns>The content object if the result contains extractable content; otherwise, null.</returns>
        internal static object? GetContent(this IActionResult? result)
        {
            return result switch
            {
                ObjectResult e => e.Value,
                ContentResult e => e.Content,
                JsonResult e => e.Value,
                _ => null
            };
        }

        /// <summary>
        /// Extracts and casts the content from an action result to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to cast the content to.</typeparam>
        /// <param name="result">The action result to extract content from, or null.</param>
        /// <returns>The content cast to type T if successful; otherwise, the default value of T.</returns>
        internal static T? GetContent<T>(this IActionResult? result)
        {
            return result switch
            {
                ObjectResult { Value: T value } => value,
                ContentResult { Content: T value } => value,
                JsonResult { Value: T value } => value,
                _ => default
            };
        }
    }
}
