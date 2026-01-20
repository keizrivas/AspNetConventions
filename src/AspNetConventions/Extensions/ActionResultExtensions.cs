using Microsoft.AspNetCore.Mvc;

namespace AspNetConventions.Extensions
{
    internal static class ActionResultExtensions
    {
        public static object? GetContent(this IActionResult? result)
        {
            return result switch
            {
                ObjectResult e => e.Value,
                ContentResult e => e.Content,
                JsonResult e => e.Value,
                _ => null
            };
        }

        public static T? GetContent<T>(this IActionResult? result)
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
