using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetConventions.Core.Abstractions.Contracts
{
    /// <summary>
    /// Defines an adapter interface for JSON serialization operations.
    /// </summary>
    public interface IJsonSerializerAdapter
    {
        /// <summary>
        /// Gets the underlying serializer options.
        /// </summary>
        /// <returns>The serializer options object.</returns>
        object GetOptions();

        /// <summary>
        /// Serializes the specified value to a JSON string.
        /// </summary>
        /// <typeparam name="TValue">The type of value to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <returns>A JSON string representation of the value.</returns>
        string Serialize<TValue>(TValue value);

        /// <summary>
        /// Asynchronously serializes the specified value to a stream.
        /// </summary>
        /// <typeparam name="TValue">The type of value to serialize.</typeparam>
        /// <param name="stream">The stream to write the JSON to.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        ValueTask SerializeAsync<TValue>(Stream stream, TValue value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserializes a JSON string to the specified type.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the JSON into.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized value, or <see langword="null"/> if deserialization fails.</returns>
        TValue? Deserialize<TValue>(string json);

        /// <summary>
        /// Asynchronously deserializes the data from the specified stream into an object of type TValue.
        /// </summary>
        /// <typeparam name="TValue">The type of the object to deserialize from the stream.</typeparam>
        /// <param name="stream">The stream containing the serialized data to be deserialized. Must be readable and positioned at the start
        /// of the data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation. The result contains the deserialized object of type
        /// TValue, or null if the stream does not contain valid data.</returns>
        ValueTask<TValue?> DeserializeAsync<TValue>(Stream stream, CancellationToken cancellationToken = default);
    }
}
