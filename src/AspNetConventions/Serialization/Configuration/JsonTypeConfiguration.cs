namespace AspNetConventions.Serialization.Configuration
{
    /// <summary>
    /// Base class for class-based JSON serialization configuration for a closed type.
    /// </summary>
    /// <typeparam name="T">The type to configure.</typeparam>
    public abstract class JsonTypeConfiguration<T> : JsonTypeConfigurationBase
    {
        /// <summary>
        /// Configures JSON serialization rules for <typeparamref name="T"/>.
        /// </summary>
        /// <param name="rule">The type rule builder.</param>
        public abstract void Configure(IJsonTypeRuleBuilder<T> rule);

        internal override void ApplyCore(JsonTypesConfigurationBuilder builder)
        {
            builder.Type<T>(Configure);
        }
    }
}
