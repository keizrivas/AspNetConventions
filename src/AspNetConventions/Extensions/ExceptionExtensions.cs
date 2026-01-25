using System;
using System.Collections.Generic;
using AspNetConventions.ExceptionHandling.Models;

namespace AspNetConventions.Extensions
{
    internal static class ExceptionExtensions
    {
        internal static HashSet<StackFrameInfo> GetStackTrace(this Exception ex, int maxDepth = 50)
        {
            ArgumentNullException.ThrowIfNull(ex);

            var trace = new System.Diagnostics.StackTrace(ex, true);
            var frames = trace.GetFrames();

            if (frames == null || frames.Length == 0)
            {
                return [];
            }

            var result = new HashSet<StackFrameInfo>(frames.Length);
            var seenMethods = new HashSet<object>();

            int count = 0;
            for (int i = 0; i < frames.Length && count < maxDepth; i++)
            {
                var frame = frames[i];
                var file = frame.GetFileName();
                var method = frame.GetMethod()?.ToString();
                var lineNumber = frame.GetFileLineNumber();

                var key = method ?? $"{file}:{lineNumber}";

                // skip duplicates
                if (!seenMethods.Add(key))
                {
                    continue;
                }

                var info = new StackFrameInfo(
                    Method: method ?? "Unknown",
                    File: file,
                    Line: lineNumber != 0 ? lineNumber : null
                );

                result.Add(info);
                count++;
            }

            return result;
        }
    }
}
