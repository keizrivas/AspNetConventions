using System;
using System.Text.RegularExpressions;

namespace AspNetConventions.Routing.Parsers
{
    /// <summary>
    /// Provides compiled regex patterns for parsing route parameters.
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?#parameter-transformers">Route parameter transformers</see>.
    /// </summary>
    internal static partial class RouteParameterPatterns
    {
        /// <summary>
        /// Matches route parameter names.
        /// </summary>
        [GeneratedRegex(
            @"\{(?<raw>\*{0,2}(?<name>[a-zA-Z_][a-zA-Z0-9_-]*))\??(?:[^{}]|\{\{|}})*\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            matchTimeoutMilliseconds: 100)]
        public static partial Regex ExtractParameterNames();

        /// <summary>
        /// Matches route parameter names with catch-all (*) and optional (?) markers.
        /// </summary>
        [GeneratedRegex(
            @"\{(?<name>\*{0,2}[a-zA-Z_][a-zA-Z0-9_-]*\??)(?:[^{}]|\{\{|}})*\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            matchTimeoutMilliseconds: 100)]
        public static partial Regex ExtractParameterNameWithMarkers();

        /// <summary>
        /// Matches route parameter names with markers and constraints.
        /// </summary>
        [GeneratedRegex(
            @"\{(?<name>\*{0,2}[a-zA-Z_][a-zA-Z0-9_-]*\??)(?<constraint>(?:[^{}]|\{\{|}})*)\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            matchTimeoutMilliseconds: 100)]
        public static partial Regex ExtractParameterNameWithMarkersAndConstraints();

        /// <summary>
        /// Remove catch-all (*) and optional (?) markers from route parameter name.
        /// </summary>
        [GeneratedRegex(
            @"^(?:\*{0,2})?(?<name>[A-Za-z_][A-Za-z0-9_-]*)\??$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            matchTimeoutMilliseconds: 100)]
        public static partial Regex RemoveParameterNameMarkers();

        /// <summary>
        /// Cleans the parameter name by removing catch-all (*) and optional (?) markers.
        /// </summary>
        /// <param name="parameter">The parameter template</param>
        /// <returns>the parameter without markers</returns>
        public static string CleanParameterName(string parameter)
        {
            var match = RemoveParameterNameMarkers().Match(parameter);

            return match.Success
                ? match.Groups["name"].Value
                : parameter;
        }

        /// <summary>
        /// Iterate through the matches route parameter names with markers and constraints.
        /// </summary>
        /// <param name="template">Template with parameters</param>
        /// <param name="callback">Callback to call in each match</param>
        /// <returns>result template with parameters</returns>
        public static string ForEachParam(string template, Func<string, string, string> callback)
        {
            return ExtractParameterNameWithMarkersAndConstraints()
            .Replace(template, m =>
            {
                var name = m.Groups["name"].Value;
                var constraint = m.Groups["constraint"].Value;

                return callback.Invoke(name, constraint);
            });
        }
    }
}
