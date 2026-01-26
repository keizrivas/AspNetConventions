using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AspNetConventions.Core.Enums;
using Microsoft.AspNetCore.Http;

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
