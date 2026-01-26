using System;
using System.Net;
using AspNetConventions.Core.Enums;

namespace AspNetConventions.Extensions
{
    internal static class HttpStatusCodeExtensions
    {
        internal static HttpStatusCodeType GetHttpStatusCodeType(this HttpStatusCode statusCode)
        {
            var code = (int)statusCode;
            return code switch
            {
                >= 100 and <= 199 => HttpStatusCodeType.Informational,
                >= 200 and <= 299 => HttpStatusCodeType.Success,
                >= 300 and <= 399 => HttpStatusCodeType.Redirection,
                >= 400 and <= 499 => HttpStatusCodeType.ClientError,
                >= 500 and <= 599 => HttpStatusCodeType.ServerError,
                _ => throw new InvalidOperationException("Invalid HTTP status code.")
            };
        }
    }
}
