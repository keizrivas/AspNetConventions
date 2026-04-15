using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetConventions.Http.Models;

namespace AspNetConventions.Serialization.Converters
{
    /// <summary>
    /// A JSON converter for <see cref="Metadata"/> that applies the configured naming policy to
    /// dictionary keys, guaranteeing consistent casing even when
    /// <see cref="JsonSerializerOptions.DictionaryKeyPolicy"/> is not set globally.
    /// </summary>
    internal sealed class MetadataJsonConverter : JsonConverter<Metadata>
    {
        // Pre-computed transformed names for built-in keys — resolved once, reused forever.
        private readonly string _requestTypeKey;
        private readonly string _timestampKey;
        private readonly string _traceIdKey;
        private readonly string _pathKey;
        private readonly string _exceptionKey;

        // Cache for user-added custom keys. Computed once per unique key, thread-safe.
        private readonly ConcurrentDictionary<string, string> _customKeyCache =
            new(StringComparer.Ordinal);

        // Cached delegate
        private readonly Func<string, string> _transformKey;

        /// <summary>
        /// Initializes the converter with the naming policy that will be applied to all keys.
        /// </summary>
        /// <param name="policy">
        /// The naming policy to apply from the enclosing <see cref="JsonSerializerOptions"/>. 
        /// When <see langword="null"/>, keys are written as-is.
        /// </param>
        public MetadataJsonConverter(JsonNamingPolicy? policy)
        {
            _transformKey = key => policy?.ConvertName(key) ?? key;
            _requestTypeKey = _transformKey(Metadata.RequestTypeKey);
            _timestampKey = _transformKey(Metadata.TimestampKey);
            _traceIdKey = _transformKey(Metadata.TraceIdKey);
            _pathKey = _transformKey(Metadata.PathKey);
            _exceptionKey = _transformKey(Metadata.ExceptionKey);
        }

        /// <inheritdoc />
        public override Metadata? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            var metadata = new Metadata();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                var key = reader.GetString()!;
                reader.Read();
                metadata[key] = JsonSerializer.Deserialize<object?>(ref reader, options);
            }

            return metadata;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Metadata value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var (key, val) in value)
            {
                writer.WritePropertyName(GetJsonKey(key));
                JsonSerializer.Serialize(writer, val, options);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Returns the transformed JSON key for a given dictionary key.
        /// </summary>
        private string GetJsonKey(string key) => key switch
        {
            Metadata.RequestTypeKey => _requestTypeKey,
            Metadata.TimestampKey => _timestampKey,
            Metadata.TraceIdKey => _traceIdKey,
            Metadata.PathKey => _pathKey,
            Metadata.ExceptionKey => _exceptionKey,
            _ => _customKeyCache.GetOrAdd(key, _transformKey)
        };
    }
}
