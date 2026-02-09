using System;
using AspNetConventions.Configuration.Options;
using Microsoft.Extensions.Options;

namespace AspNetConventions.Core.Abstractions.Models
{
    /// <summary>
    /// Provides a wrapper for ASP.NET convention options with snapshot functionality.
    /// </summary>
    /// <remarks>
    /// This class allows for creating a snapshot of AspNetConventions options at a specific point in time,
    /// preventing changes to the options from affecting already-processed requests.
    /// </remarks>
    public class ConventionOptions(IOptions<AspNetConventionOptions> options)
    {
        private AspNetConventionOptions? _options;

        /// <summary>
        /// Gets the AspNetConventions options snapshot.
        /// </summary>
        /// <value>The snapshot of AspNetConventions options created by <see cref="CreateOptionSnapshot"/>.</value>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="CreateOptionSnapshot"/> has not been called.</exception>
        /// <remarks>
        /// This property will throw an exception if accessed before <see cref="CreateOptionSnapshot"/> is called,
        /// ensuring that options are properly initialized before use.
        /// </remarks>
        public AspNetConventionOptions Options => _options ?? throw new InvalidOperationException("Not an option snapshot, please call \"CreateOptionSnapshot\" method.");

        /// <summary>
        /// Creates a snapshot of the current AspNetConventions options.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the provided options are null.</exception>
        /// <returns>The snapshot of AspNetConventions options.</returns>
        /// <remarks>
        /// This method captures the current state of AspNetConventions options and stores them internally.
        /// Subsequent calls to this method will not update the snapshot, ensuring consistency across the
        /// lifetime of this instance. Call this method before accessing the <see cref="Options"/> property.
        /// </remarks>
        public AspNetConventionOptions CreateOptionSnapshot()
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            _options ??= options.Value;
            return _options;
        }
    }
}
