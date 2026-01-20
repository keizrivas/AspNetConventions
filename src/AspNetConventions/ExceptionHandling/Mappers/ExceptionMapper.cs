using System;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http;

namespace AspNetConventions.ExceptionHandling.Mappers
{
    /// <summary>
    /// Provides a base class for strongly-typed exception mappers.
    /// </summary>
    /// <typeparam name="TException">The type of exception this mapper handles.</typeparam>
    public abstract class ExceptionMapper<TException> : IExceptionMapper
        where TException : Exception
    {
        public virtual bool CanMapException(
            ExceptionDescriptor exceptionContext,
            RequestDescriptor httpContext)
        {
            ArgumentNullException.ThrowIfNull(exceptionContext);
            return exceptionContext.Exception is TException;
        }

        ExceptionEnvelope IExceptionMapper.MapException(
            ExceptionDescriptor exceptionContext,
            RequestDescriptor httpContext)
        {
            if (exceptionContext.Exception is not TException)
            {
                throw new InvalidOperationException(
                    $"Cannot map exception of type {exceptionContext.Exception.GetType().Name} " +
                    $"with {GetType().Name}. Always call CanMapException method first.");
            }

            return MapException((TException)exceptionContext.Exception, exceptionContext, httpContext);
        }

        /// <summary>
        /// Maps a strongly-typed exception to a standardized error response.
        /// </summary>
        public abstract ExceptionEnvelope MapException(
            TException exception,
            ExceptionDescriptor exceptionContext,
            RequestDescriptor httpContext);
    }
}
