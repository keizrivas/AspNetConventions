using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Provides extension methods for safely invoking nullable asynchronous delegates.
    /// </summary>
    /// <remarks>
    /// These helpers are intended for scenarios where callbacks or hooks are optional
    /// </remarks>
    internal static class AsyncDelegateExtensions
    {
        /// <summary>
        /// Invokes the specified delegate asynchronously, if it is not <see langword="null"/>.
        /// </summary>
        internal static async Task<object?> InvokeAsync(this Delegate? del, params object?[] args)
        {
            if (del is null)
            {
                return null;
            }

            object? result;

            try
            {
                result = del.DynamicInvoke(args);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                // Unwrap the original exception thrown by the delegate for clearer stack traces.
                throw ex.InnerException;
            }

            if (result is null)
            {
                return null;
            }

            if (result is Task task)
            {
                await task.ConfigureAwait(false);

                var taskType = task.GetType();

                // Handle Task<T>
                if (taskType.IsGenericType &&
                    taskType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    return taskType
                        .GetProperty("Result", BindingFlags.Public | BindingFlags.Instance)?
                        .GetValue(task);
                }

                // Non-generic Task
                return null;
            }

            // Delegate did not return a Task; this is allowed but generally not recommended
            // in async pipelines. We return the raw value to avoid hiding behavior.
            return result;
        }

        /// <summary>
        /// Invokes the specified delegate asynchronously, if it is not <see langword="null"/>,
        /// and returns a strongly typed result.
        /// </summary>
        internal static async Task<TResult?> InvokeAsync<TResult>(this Delegate? del, params object?[] args)
        {
            var result = await del.InvokeAsync(args).ConfigureAwait(false);

            if (result is null)
            {
                return default;
            }

            return (TResult)result;
        }
    }
}
