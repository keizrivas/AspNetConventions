using System;
using System.Collections.Generic;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Provides extension methods for exception analysis and stack trace processing.
    /// </summary>
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Extracts and processes the stack trace from an exception, returning a set of stack frame information.
        /// </summary>
        /// <param name="exception">The exception to extract the stack trace from.</param>
        /// <param name="maxDepth">The maximum number of stack frames to process.</param>
        /// <returns>A <see cref="IReadOnlyList{T}"/> containing string representing the stack trace.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
        internal static IReadOnlyList<string> GetStackTrace(this Exception exception, int maxDepth)
        {
            ArgumentNullException.ThrowIfNull(exception);

            var trace = exception.StackTrace;
            if (string.IsNullOrEmpty(trace))
            {
                return [];
            }

            var result = new List<string>(15);

            int length = trace.Length;
            int i = 0;

            while (i < length)
            {
                if (result.Count == maxDepth)
                {
                    break;
                }

                // Find line start
                int lineStart = i;

                // Find line end
                int lineEnd = trace.IndexOf('\n', i);
                if (lineEnd < 0)
                {
                    lineEnd = length;
                }

                // Trim leading whitespace
                int start = lineStart;
                while (start < lineEnd && trace[start] == ' ')
                {
                    start++;
                }

                // Must start with "at "
                if (start + 3 <= lineEnd &&
                    trace[start] == 'a' &&
                    trace[start + 1] == 't' &&
                    trace[start + 2] == ' ')
                {
                    int methodEnd = lineEnd;

                    int methodStart = start + 3;

                    // Trim trailing '\r'
                    if (methodEnd > methodStart && trace[methodEnd - 1] == '\r')
                    {
                        methodEnd--;
                    }

                    result.Add(trace.Substring(methodStart, methodEnd - methodStart));
                }

                i = lineEnd + 1;
            }

            return result;
        }
    }
}
