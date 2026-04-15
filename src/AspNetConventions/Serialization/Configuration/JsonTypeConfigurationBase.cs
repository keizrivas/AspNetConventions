namespace AspNetConventions.Serialization.Configuration
{
    /// <summary>
    /// Non-generic base class for all JSON type configuration classes.
    /// Enables the assembly scanner to discover and apply configurations without
    /// knowing the generic type parameter at scan time.
    /// </summary>
    public abstract class JsonTypeConfigurationBase
    {
        /// <summary>
        /// Applies the configuration to the given builder.
        /// Called internally by the assembly scanner and by <see cref="JsonTypesConfigurationBuilder"/>.
        /// </summary>
        internal abstract void ApplyCore(JsonTypesConfigurationBuilder builder);
    }
}
