using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetConventions.Configuration;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Common.Abstractions
{
    public class ConventionOptions(IOptions<AspNetConventionOptions> options)
    {
        private AspNetConventionOptions? _options;

        public AspNetConventionOptions Options => _options ?? throw new InvalidOperationException("Not an option snapshot, please call \"CreateOptionSnapshot\" method.");

        public void CreateOptionSnapshot()
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            _options ??= options.Value;
        }
    }
}
