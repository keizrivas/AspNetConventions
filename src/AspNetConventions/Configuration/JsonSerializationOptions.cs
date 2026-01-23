using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetConventions.Common.Abstractions;
using AspNetConventions.Common.Converters;
using AspNetConventions.Common.Enums;
using AspNetConventions.Common.Hooks;
using AspNetConventions.Http;
using AspNetConventions.ResponseFormatting.Models;
using AspNetConventions.Serialization.Policies;
using AspNetConventions.Serialization.Resolvers;

namespace AspNetConventions.Configuration
{
    /// <summary>
    /// Provides configuration options for JSON serialization.
    /// </summary>
    public sealed class JsonSerializationOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets whether json serialization is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the casing style for JSON property names.
        /// </summary>
        public CasingStyle CaseStyle { get; set; } = CasingStyle.CamelCase;

        /// <summary>
        /// Gets or sets a custom case converter.
        /// </summary>
        public ICaseConverter? CaseConverter { get; set; }

        /// <summary>
        /// Gets or sets an action to configure JSON ignore rules.
        /// </summary>
        public Action<JsonIgnoreRules>? ConfigureIgnoreRules { get; set; }

        /// <summary>
        /// Gets or sets the default ignore condition.
        /// </summary>
        public JsonIgnoreCondition DefaultIgnoreCondition { get; set; } = JsonIgnoreCondition.Never;

        /// <summary>
        /// Gets or sets whether property name comparison is case-insensitive.
        /// </summary>
        public bool PropertyNameCaseInsensitive { get; set; }

        /// <summary>
        /// Gets or sets whether to write indented JSON (pretty-print).
        /// </summary>
        public bool WriteIndented { get; set; }

        /// <summary>
        /// Gets or sets whether to allow trailing commas in JSON.
        /// </summary>
        public bool AllowTrailingCommas { get; set; }

        /// <summary>
        /// Gets or sets how numbers are handled during serialization.
        /// </summary>
        public JsonNumberHandling NumberHandling { get; set; }

        /// <summary>
        /// Gets or sets the maximum depth for JSON objects.
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Gets or sets custom JSON converters.
        /// </summary>
        public IList<JsonConverter> Converters { get; private set; } = [];

        /// <summary>
        /// Gets or sets custom serializer options.
        /// When set, all other properties are ignored.
        /// </summary>
        public JsonSerializerOptions? CustomSerializerOptions { get; set; }

        /// <summary>
        /// Gets or sets the collection of hooks used to customize json serialization behavior.
        /// </summary>
        public JsonSerializationHooks Hooks { get; set; } = new();

        /// <summary>
        /// Creates a deep clone of <see cref="JsonSerializationOptions"/> instance.
        /// </summary>
        public object Clone()
        {
            return new JsonSerializationOptions
            {
                CaseStyle = CaseStyle,
                CaseConverter = CaseConverter,
                ConfigureIgnoreRules = ConfigureIgnoreRules,
                DefaultIgnoreCondition = DefaultIgnoreCondition,
                PropertyNameCaseInsensitive = PropertyNameCaseInsensitive,
                WriteIndented = WriteIndented,
                AllowTrailingCommas = AllowTrailingCommas,
                NumberHandling = NumberHandling,
                MaxDepth = MaxDepth,
                Converters = Converters,
                CustomSerializerOptions = CustomSerializerOptions,
                Hooks = Hooks,
            };
        }

        /// <summary>
        /// Builds JsonSerializerOptions from this configuration.
        /// </summary>
        internal JsonSerializerOptions BuildSerializerOptions()
        {
            if (CustomSerializerOptions != null)
            {
                return CustomSerializerOptions;
            }

            var ignoreRules = new JsonIgnoreRules();

            // Default ignore rules
            ignoreRules.IgnoreProperty<ApiResponse>(e => e.Metadata, JsonIgnoreCondition.WhenWritingNull);
            ignoreRules.IgnoreProperty<DefaultApiResponse>(e => e.Pagination, JsonIgnoreCondition.WhenWritingNull);
            ignoreRules.IgnoreProperty<Metadata>(e => e.ExceptionType, JsonIgnoreCondition.WhenWritingNull);
            ignoreRules.IgnoreProperty<Metadata>(e => e.StackTrace, JsonIgnoreCondition.WhenWritingNull);

            ConfigureIgnoreRules?.Invoke(ignoreRules);

            var namingPolicy = GetNamingPolicy();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = namingPolicy,
                PropertyNameCaseInsensitive = PropertyNameCaseInsensitive,
                AllowTrailingCommas = AllowTrailingCommas,
                DefaultIgnoreCondition = DefaultIgnoreCondition,
                WriteIndented = WriteIndented,
                NumberHandling = NumberHandling,
                MaxDepth = MaxDepth,
                TypeInfoResolver = new JsonTypeInfoResolver(ignoreRules.CreateSnapshot())
            };

            // Add enum converter with naming policy
            options.Converters.Add(new JsonStringEnumConverter(namingPolicy));

            // Add custom converters
            foreach (var converter in Converters)
            {
                options.Converters.Add(converter);
            }

            return options;
        }

        /// <summary>
        /// Get the JsonNamingPolicy from this configuration.
        /// </summary>
        private JsonNamingPolicy? GetNamingPolicy()
        {
            var defaultJsonNamingPolicy = JsonNamingPolicy.CamelCase;
            return CaseConverter != null
                ? new CustomJsonNamingPolicy(CaseConverter)
                : CaseStyle switch
                {
                    CasingStyle.CamelCase => defaultJsonNamingPolicy,
                    CasingStyle.SnakeCase => JsonNamingPolicy.SnakeCaseLower,
                    CasingStyle.KebabCase => JsonNamingPolicy.KebabCaseLower,
                    CasingStyle.PascalCase => new CustomJsonNamingPolicy(
                        CaseConverterFactory.CreatePascalCase()),
                    _ => defaultJsonNamingPolicy
                };
        }
    }
}
