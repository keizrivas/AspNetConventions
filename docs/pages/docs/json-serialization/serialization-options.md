# Serialization Options

**AspNetConventions** provides a single options surface through which you can access the JSON output settings you already know. All settings are applied to the active adapter at startup.

::: callout info Newtonsoft.Json support
The current built-in adapter targets `System.Text.Json` (included with .NET, no extra dependency). Support for **Newtonsoft.Json** is planned for a future release.
:::

---

## Casing Style {#casing-style}

Set the JSON property naming convention for all serialized types using [`CasingStyle`](./configuration.md#casingstyle):

```csharp
builder.Services
    .AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Json.CaseStyle = CasingStyle.SnakeCase;
    });
```

| `CasingStyle` | JSON property example |
|---|---|
| `CamelCase` *(default)* | `"userId"` |
| `SnakeCase` | `"user_id"` |
| `KebabCase` | `"user-id"` |
| `PascalCase` | `"UserId"` |

This applies to all serialized objects — API response payloads, model objects, and any other JSON your application produces.

---

## Custom Case Converter {#custom-case-converter}

When none of the built-in [`CasingStyle`](./configuration.md#casingstyle) options fit your convention, implement `ICaseConverter` and assign it to [`options.Json.CaseConverter`](./configuration.md#jsonserializationoptions). It overrides [`CaseStyle`](./configuration.md#casingstyle) when set.

The built-in `SnakeCaseConverter` treats digit runs as part of the surrounding word, so `Iso2` serializes as `"iso2"` and `Base64` as `"base64"`. The example below adds **number-aware boundaries**: a transition from letters to digits (or digits to letters) is treated as a word break, just like an uppercase letter after a lowercase one.

```csharp
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Converters;

public class NumberSensitiveSnakeCaseConverter : ICaseConverter
{
    public string Convert(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var words = CaseTokenizer.Tokenize(value.AsSpan(), numberBoundaries: true);
        return string.Join("_", words.Select(w =>
            value.Substring(w.Start, w.Length).ToLowerInvariant()));
    }
}
```

Register it:

```csharp
options.Json.CaseConverter = new NumberSensitiveSnakeCaseConverter();
```

| Input | Output |
|---|---|
| `Iso2` | `"iso_2"` |
| `iso3` | `"iso_3"` |
| `Base64` | `"base_64"` |
| `Sha256Hash` | `"sha_256_hash"` |
| `OAuth2Token` | `"o_auth_2_token"` |
| `UserId` | `"user_id"` |

`CaseConverter` also controls dictionary key casing and route parameter naming, so a single implementation applies consistently across all three.

See [`JsonSerializationOptions.CaseConverter`](./configuration.md#jsonserializationoptions) and [`RouteConventionOptions.CaseConverter`](../route-standardization/configuration.md#routeconventionoptions) for more information.

---

## Output & Parsing Options {#output-and-parsing-options}

The most common JSON settings are surfaced directly on `options.Json`. They are applied to the underlying serializer at startup, so there is no per-request cost:

```csharp
builder.Services
    .AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Json.MaxDepth = 32;
        options.Json.WriteIndented = true;
        options.Json.CaseInsensitive = true;
        options.Json.AllowTrailingCommas = true;
        options.Json.UseStringEnumConverter = true;
        options.Json.NumberHandling = NumberHandling.AllowReadingFromString;
        options.Json.IgnoreCondition = IgnoreCondition.WhenWritingNull;
    });
```

See [JsonSerializationOptions](./configuration.md#jsonserializationoptions) for more information.

### Enum serialization {#enum-serialization}

With `UseStringEnumConverter = true` (the default), enums are serialized as strings using the same naming policy as property names:

```csharp
public enum OrderStatus { New, InProgress, Completed }

// CaseStyle = SnakeCase
{ "status": "in_progress" }

// CaseStyle = CamelCase
{ "status": "inProgress" }

// UseStringEnumConverter = false
{ "status": 1 }
```

### Number handling {#number-handling}

`NumberHandling` is forwarded to `System.Text.Json.Serialization.JsonNumberHandling`:

```csharp
// Accept "42" and 42 interchangeably during deserialization
options.Json.NumberHandling = NumberHandling.AllowReadingFromString;

// Emit numbers as quoted strings — useful for 64-bit integers consumed by JavaScript
options.Json.NumberHandling = NumberHandling.WriteAsString;

// Allow NaN, Infinity, -Infinity for floating-point fields
options.Json.NumberHandling = NumberHandling.AllowNamedFloatingPointLiterals;
```

See [`NumberHandling`](./configuration.md#numberhandling) for all values.

### Global ignore condition {#global-ignore-condition}

`IgnoreCondition` sets the **default** for every property in the application. A common configuration is to drop `null` values across the board:

```csharp
options.Json.IgnoreCondition = IgnoreCondition.WhenWritingNull;
```

Per-type rules and global ignore methods both override this default — see [Type Configuration](./type-configuration.md#per-type-property-configuration) and [Global Ignore Rules](./type-configuration.md#global-ignore-rules).

---

## Custom JSON Converters {#custom-json-converters}

A `JsonConverter<T>` (`System.Text.Json`) controls how a single CLR type is read from and written to JSON. Use one when a value's wire format is fundamentally different from its in-memory shape.

Example: Serializing a `Money` value object as `"USD 19.99"` instead of an object with `Currency` and `Amount` fields.

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

public class MoneyJsonConverter : JsonConverter<Money>
{
    public override Money Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString() ?? throw new JsonException("Expected string");
        var parts = raw.Split(' ', 2);
        return new Money(parts[0], decimal.Parse(parts[1], CultureInfo.InvariantCulture));
    }

    public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{value.Currency} {value.Amount.ToString(CultureInfo.InvariantCulture)}");
    }
}
```

Add the converter to [`options.Json.Converters`](./configuration.md#jsonserializationoptions). At startup, **AspNetConventions** inspects the registered instance, identifies it as a `JsonConverter`, and forwards it to the active adapter:

```csharp
options.Json.Converters.Add(new MoneyJsonConverter());
```

**Result:**
```json
{ "price": "USD 19.99" }
```

::: callout info Built-in converters
Two converters are registered automatically and do not need to be added manually:
- A converter for [`Metadata`](../response-formatting/metadata.md) that integrates with the active naming policy.
- `JsonStringEnumConverter` (when [`UseStringEnumConverter`](#enum-serialization) is `true`).
:::

Custom converters are run **before** the per-type / per-property rules configured in [Type Configuration](./type-configuration.md), so they take full control over the wire shape of the types they target.
