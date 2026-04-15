using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Abstractions.Models;
using AspNetConventions.Core.Converters;
using AspNetConventions.Core.Enums;
using AspNetConventions.Core.Enums.Json;
using AspNetConventions.Http.Models;
using AspNetConventions.Responses.Models;
using AspNetConventions.Serialization.Configuration;
using AspNetConventions.Serialization.Converters;
using AspNetConventions.Serialization.Policies;
using AspNetConventions.Serialization.Resolvers;

namespace AspNetConventions.Serialization.Adapters
{
    /// <summary>
    /// An adapter that provides JSON serialization using System.Text.Json.
    /// </summary>
    /// <remarks>
    /// This adapter implements <see cref="IJsonSerializerAdapter"/> to provide JSON serialization
    /// capabilities using the System.Text.Json (<see cref="JsonSerializer"/>) library.
    /// </remarks>
    public class SystemTextJsonAdapter : IJsonSerializerAdapter
    {
        private readonly JsonSerializationOptions _options;
        private readonly Action<JsonSerializerOptions>? _configure;
        private JsonSerializerOptions? _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemTextJsonAdapter"/> class.
        /// </summary>
        /// <param name="options">The JSON serialization options to use.</param>
        /// <param name="configure">An optional delegate to further configure <see cref="JsonSerializerOptions"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
        public SystemTextJsonAdapter(
            JsonSerializationOptions options,
            Action<JsonSerializerOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(options);
            _options = options;
            _configure = configure;
        }

        object IJsonSerializerAdapter.GetOptions()
        {
            return GetOptions();
        }

        /// <summary>
        /// Gets the configured <see cref="JsonSerializerOptions"/> instance.
        /// </summary>
        /// <returns>The configured <see cref="JsonSerializerOptions"/>.</returns>
        /// <remarks>
        /// This method caches the serializer options after first creation for performance.
        /// </remarks>
        public JsonSerializerOptions GetOptions()
        {
            if (_jsonSerializerOptions != null)
            {
                return _jsonSerializerOptions;
            }

            // Framework defaults + user-defined rules in one unified builder.
            var typeBuilder = new JsonTypesConfigurationBuilder();

            // Internal framework defaults
            typeBuilder
                .Type<ApiResponse>(t => t.Property(x => x.Metadata)
                    .Ignore(JsonIgnoreCondition.WhenWritingNull))
                .Type<DefaultApiResponse>(t => t.Property(x => x.Pagination)
                    .Ignore(JsonIgnoreCondition.WhenWritingNull))
                .Type<PaginationMetadata>(t =>
                {
                    t.Property(x => x.Links).Ignore(JsonIgnoreCondition.WhenWritingNull);
                    t.Property(x => x.HasNextPage).Ignore(JsonIgnoreCondition.WhenWritingNull);
                    t.Property(x => x.HasPreviousPage).Ignore(JsonIgnoreCondition.WhenWritingNull);
                });

            // User-defined rules (applied after defaults so they can override)
            _options.ConfigureTypes?.Invoke(typeBuilder);

            foreach (var assembly in _options.AssembliesToScan)
            {
                typeBuilder.ScanAssembly(assembly);
            }

            var namingPolicy = GetNamingPolicy();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = namingPolicy,
                DictionaryKeyPolicy  = namingPolicy,
                PropertyNameCaseInsensitive = _options.CaseInsensitive,
                AllowTrailingCommas = _options.AllowTrailingCommas,
                DefaultIgnoreCondition = MapIgnoreCondition(),
                WriteIndented = _options.WriteIndented,
                NumberHandling = MapNumberHandling(),
                MaxDepth = Math.Max(_options.MaxDepth, 0),
                TypeInfoResolver = new JsonTypeInfoResolver(typeBuilder.CreateSnapshot())
            };

            // Add metadata converter to ensure Metadata keys always follow PropertyNamingPolicy,
            // regardless of how DictionaryKeyPolicy is configured for other types.
            options.Converters.Add(new MetadataJsonConverter(options.PropertyNamingPolicy));

            // Add string enum converter with naming policy if enabled
            if (_options.UseStringEnumConverter)
            {
                options.Converters.Add(new JsonStringEnumConverter(namingPolicy));
            }

            // Add custom converters from ConverterTypes
            foreach (var converter in _options.Converters)
            {
                if (converter is JsonConverter jsonConverter)
                {
                    options.Converters.Add(jsonConverter);
                }
            }

            _jsonSerializerOptions = options;
            return _jsonSerializerOptions;
        }

        string IJsonSerializerAdapter.Serialize<TValue>(TValue value)
        {
            return Serialize(value);
        }

        /// <summary>
        /// Serializes the specified value to a JSON string.
        /// </summary>
        /// <typeparam name="TValue">The type of value to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <returns>A JSON string representation of the value.</returns>
        public string Serialize<TValue>(TValue value)
        {
            var options = GetOptions();
            return JsonSerializer.Serialize(value, options);
        }

        ValueTask IJsonSerializerAdapter.SerializeAsync<TValue>(Stream stream, TValue value, CancellationToken cancellationToken)
        {
           return SerializeAsync(stream, value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously serializes the specified value to a stream.
        /// </summary>
        /// <typeparam name="TValue">The type of value to serialize.</typeparam>
        /// <param name="stream">The stream to write the JSON to.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public async ValueTask SerializeAsync<TValue>(
            Stream stream,
            TValue value,
            CancellationToken cancellationToken = default)
        {
            var options = GetOptions();
            await JsonSerializer.SerializeAsync(
                stream,
                value,
                options,
                cancellationToken);
        }

        TValue? IJsonSerializerAdapter.Deserialize<TValue>(string json) where TValue : default
        {
            return Deserialize<TValue>(json);
        }

        /// <summary>
        /// Deserializes a JSON string to the specified type.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize the JSON into.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized value, or <see langword="null"/> if deserialization fails.</returns>
        public TValue? Deserialize<TValue>(string json)
        {
            var options = GetOptions();
            return JsonSerializer.Deserialize<TValue>(json, options);
        }


        ValueTask<TValue?> IJsonSerializerAdapter.DeserializeAsync<TValue>(Stream stream, CancellationToken cancellationToken) where TValue : default
        {
            return DeserializeAsync<TValue>(stream, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deserializes the JSON content from the specified stream into an object of type TValue.
        /// </summary>
        /// <typeparam name="TValue">The type of the object to deserialize from the JSON content.</typeparam>
        /// <param name="stream">The stream containing the JSON data to deserialize. The stream must be readable and positioned at the start
        /// of the JSON content.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation. The result contains the deserialized object of type
        /// TValue, or null if the JSON content is empty or cannot be deserialized to type TValue.</returns>
        public async ValueTask<TValue?> DeserializeAsync<TValue>(Stream stream, CancellationToken cancellationToken = default)
        {
            var options = GetOptions();
            return await JsonSerializer.DeserializeAsync<TValue>(
                stream,
                options,
                cancellationToken
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the JsonNamingPolicy from this configuration.
        /// </summary>
        private JsonNamingPolicy? GetNamingPolicy()
        {
            // Use custom converter if provided
            if (_options.CaseConverter != null)
            {
                return new CustomJsonNamingPolicy(_options.CaseConverter);
            }

            // Map CasingStyle to System.Text.Json naming policy
            var defaultJsonNamingPolicy = JsonNamingPolicy.CamelCase;
            return _options.CaseStyle switch
            {
                CasingStyle.CamelCase => defaultJsonNamingPolicy,
                CasingStyle.SnakeCase => JsonNamingPolicy.SnakeCaseLower,
                CasingStyle.KebabCase => JsonNamingPolicy.KebabCaseLower,
                CasingStyle.PascalCase => new CustomJsonNamingPolicy(
                    CaseConverterFactory.CreatePascalCase()),
                _ => defaultJsonNamingPolicy
            };
        }

        private JsonIgnoreCondition MapIgnoreCondition()
        {
            return _options.IgnoreCondition switch
            {
                IgnoreCondition.Never => JsonIgnoreCondition.Never,
                IgnoreCondition.Always => JsonIgnoreCondition.Always,
                IgnoreCondition.WhenWritingNull => JsonIgnoreCondition.WhenWritingNull,
                IgnoreCondition.WhenWritingDefault => JsonIgnoreCondition.WhenWritingDefault,
                _ => JsonIgnoreCondition.Never
            };
        }

        private JsonNumberHandling MapNumberHandling()
        {
            return _options.NumberHandling switch
            {
                NumberHandling.Strict => JsonNumberHandling.Strict,
                NumberHandling.AllowReadingFromString => JsonNumberHandling.AllowReadingFromString,
                NumberHandling.WriteAsString => JsonNumberHandling.WriteAsString,
                NumberHandling.AllowNamedFloatingPointLiterals => JsonNumberHandling.AllowNamedFloatingPointLiterals,
                _ => JsonNumberHandling.Strict
            };
        }
    }
}
