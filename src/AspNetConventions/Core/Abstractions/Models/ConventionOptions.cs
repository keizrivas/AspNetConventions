using System;
using AspNetConventions.Configuration.Options;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Core.Abstractions.Models
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
