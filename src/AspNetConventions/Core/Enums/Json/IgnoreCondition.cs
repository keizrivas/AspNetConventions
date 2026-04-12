namespace AspNetConventions.Core.Enums.Json
{
    /// <summary>
    /// Specifies when property values should be ignored during JSON serialization.
    /// </summary>
    public enum IgnoreCondition
    {
        /// <summary>
        /// Property values are never ignored during serialization.
        /// </summary>
        Never,

        /// <summary>
        /// Property values are always ignored during serialization.
        /// </summary>
        Always,

        /// <summary>
        /// Property values are ignored when writing null to JSON.
        /// </summary>
        WhenWritingNull,

        /// <summary>
        /// Property values are ignored when writing default values to JSON.
        /// </summary>
        WhenWritingDefault
    }
}
