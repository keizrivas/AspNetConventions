using System;
using System.Threading;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetConventions.ExceptionHandling.Mappers
{
    /// <summary>
    /// Provides a base class for strongly-typed exception mappers.
    /// </summary>
    /// <typeparam name="TException">The type of exception this mapper handles.</typeparam>
    public abstract class ExceptionMapper<TException> : IExceptionMapper
        where TException : Exception
    {
        public virtual bool CanMapException(Exception exception, RequestDescriptor requestDescriptor)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception is TException;
        }

        ExceptionDescriptor2 IExceptionMapper.MapException(Exception exception, RequestDescriptor requestDescriptor)
        {
            ArgumentNullException.ThrowIfNull(exception);

            if (exception is not TException)
            {
                throw new InvalidOperationException(
                    $"Cannot map exception of type {exception.GetType().Name} " +
                    $"with {GetType().Name}. Always call CanMapException method first.");
            }

            return MapException((TException)exception, requestDescriptor);
        }

        /// <summary>
        /// Maps a strongly-typed exception to a standardized error response.
        /// </summary>
        public abstract ExceptionDescriptor2 MapException(TException exception, RequestDescriptor httpContext);
    }
}
