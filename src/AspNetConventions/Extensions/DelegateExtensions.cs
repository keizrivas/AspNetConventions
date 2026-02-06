using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace AspNetConventions.Extensions
{
    /// <summary>
    /// Provides extension methods for safely invoking nullable asynchronous delegates.
    /// </summary>
    /// <remarks>
    /// These helpers are intended for scenarios where callbacks or hooks are optional
    /// and need to be invoked safely without causing exceptions when null.
    /// </remarks>
    internal static class AsyncDelegateExtensions
    {
        /// <summary>
        /// Invokes the specified delegate asynchronously, if it is not <see langword="null"/>.
        /// </summary>
        /// <param name="del">The delegate to invoke, or null if no action should be taken.</param>
        /// <param name="args">The arguments to pass to the delegate.</param>
        /// <returns>The result of the delegate invocation, or null if the delegate is null.</returns>
        /// <exception cref="TargetInvocationException">Thrown when the delegate throws an exception; the original exception is unwrapped.</exception>
        /// <remarks>
        /// This method safely handles null delegates and automatically unwraps Task-based results.
        /// </remarks>
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
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }

            if (result is null)
            {
                return null;
            }

            if (result is Task task)
            {
                await task.ConfigureAwait(false);

                // Handle Task<T>
                var taskType = task.GetType();
                if (taskType.IsGenericType &&
                    taskType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    return taskType
                        .GetProperty(nameof(Task<object>.Result), BindingFlags.Public | BindingFlags.Instance)!
                        .GetValue(task);
                }

                // Non-generic Task
                return null;
            }

            // Synchronous return value
            return result;
        }

        /// <summary>
        /// Invokes the specified delegate asynchronously, if it is not <see langword="null"/>,
        /// and returns a strongly typed result.
        /// </summary>
        /// <typeparam name="TResult">The expected return type of the delegate.</typeparam>
        /// <param name="del">The delegate to invoke, or null if no action should be taken.</param>
        /// <param name="args">The arguments to pass to the delegate.</param>
        /// <returns>The strongly typed result of the delegate invocation, or default(TResult) if the delegate is null or returns null.</returns>
        /// <exception cref="TargetInvocationException">Thrown when the delegate throws an exception; the original exception is unwrapped.</exception>
        /// <exception cref="InvalidCastException">Thrown when the delegate result cannot be cast to the specified type.</exception>
        /// <remarks>
        /// This method provides a type-safe wrapper around the generic InvokeAsync method.
        /// It automatically handles null delegates and performs type casting for the result.
        /// </remarks>
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
