using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetConventions.Configuration;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Routing.Providers
{
    public sealed class QueryValueProviderFactory(IOptions<AspNetConventionOptions> options) : IValueProviderFactory
    {
        public async Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var request = context.ActionContext.HttpContext.Request;

            if (request.Query.Count == 0)
            {
                return;
            }

            var provider = new QueryValueProvider(options, request.Query);
            context.ValueProviders.Insert(0, provider);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }

}
