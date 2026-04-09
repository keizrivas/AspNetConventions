
## JsonSerializationOptions

**Namespace:** `AspNetConventions.Configuration.Options`
**Accessed via:** `options.Json`

Controls JSON serialization behaviour application-wide, covering both API response output and model serialization. The default underlying serializer is `System.Text.Json`.

### Core properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables AspNetConventions JSON configuration. When `false`, the application's default serializer settings are used unchanged |
| `CaseStyle` | `CasingStyle` | `CamelCase` | JSON property naming style applied globally |
| `CaseConverter` | `ICaseConverter?` | `null` | Custom case converter for property names. Takes precedence over `CaseStyle` when set |

### Serializer behaviour properties

| Property | Type | Default | Description |
|---|---|---|---|
| `WriteIndented` | `bool` | `false` | When `true`, JSON output is pretty-printed with indentation |
| `CaseInsensitive` | `bool` | `false` | When `true`, property name matching during deserialization is case-insensitive |
| `AllowTrailingCommas` | `bool` | `false` | When `true`, trailing commas in JSON input are tolerated during deserialization |
| `MaxDepth` | `int` | `0` | Maximum depth of nested JSON objects. `0` uses the serializer's default (64 for `System.Text.Json`) |
| `UseStringEnumConverter` | `bool` | `true` | When `true`, enum values are serialized as strings using the active naming policy. When `false`, enums are serialized as integers |
| `NumberHandling` | `NumberHandling` | `Strict` | Controls how numbers are read and written. See [NumberHandling values](#numberhandling-values) |
| `IgnoreCondition` | `IgnoreCondition` | `Never` | Global default ignore condition applied to all properties. See [IgnoreCondition values](#ignorecondition-values) |

### Advanced properties

| Property | Type | Default | Description |
|---|---|---|---|
| `ConfigureIgnoreRules` | `Action<JsonIgnoreRules>?` | `null` | Delegate for configuring per-type, per-property ignore rules |
| `Converters` | `IReadOnlyList<object>` | `[]` | Custom `JsonConverter` instances added to the serializer |
| `Hooks` | `JsonSerializationHooks` | `new()` | Hooks for intercepting the serialization pipeline |

### NumberHandling values

| Value | Description |
|---|---|
| `Strict` *(default)* | Numbers must be JSON number tokens. Strings are not accepted |
| `AllowReadingFromString` | Numbers can be read from quoted JSON strings (e.g. `"42"`) |
| `WriteAsString` | Numbers are written as quoted JSON strings |
| `AllowNamedFloatingPointLiterals` | Allows `NaN`, `Infinity`, and `-Infinity` as named floating-point values |

### IgnoreCondition values

| Value | Description |
|---|---|
| `Never` *(default)* | All properties are always serialized |
| `Always` | All properties are always ignored (not useful as a global default) |
| `WhenWritingNull` | Properties with `null` values are omitted from output |
| `WhenWritingDefault` | Properties with default values (`null`, `0`, `false`, etc.) are omitted |

### ConfigureAdapter

By default, AspNetConventions uses `SystemTextJsonAdapter` backed by `System.Text.Json`. Use `ConfigureAdapter<TAdapter, TOptions>()` to swap in a different serializer adapter:

```csharp
/// <summary>
/// Configures a custom serializer adapter.
/// </summary>
/// <typeparam name="TAdapter">The IJsonSerializerAdapter implementation to use.</typeparam>
/// <typeparam name="TOptions">The adapter-specific options type.</typeparam>
/// <param name="configure">Optional delegate to configure adapter-specific options.</param>
public void ConfigureAdapter<TAdapter, TOptions>(Action<TOptions>? configure = null)
```

The built-in adapter is `SystemTextJsonAdapter`, which accepts `JsonSerializerOptions` as its options type:

```csharp
options.Json.ConfigureAdapter<SystemTextJsonAdapter, JsonSerializerOptions>(serializerOptions =>
{
    // Direct access to JsonSerializerOptions for anything not covered
    // by the AspNetConventions fluent API
    serializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});
```

> `ConfigureAdapter` is an escape hatch for serializer-specific settings that fall outside the AspNetConventions abstraction. Prefer the typed properties above for settings that are available through the standard API.

### Examples

```csharp
// Snake case JSON, pretty-printed, enums as strings, nulls omitted
options.Json.CaseStyle            = CasingStyle.SnakeCase;
options.Json.WriteIndented        = true;
options.Json.UseStringEnumConverter = true;
options.Json.IgnoreCondition      = IgnoreCondition.WhenWritingNull;

// Accept numbers written as strings from clients
options.Json.NumberHandling = NumberHandling.AllowReadingFromString;

// Register a custom JsonConverter
options.Json.Converters = [new MyDateTimeConverter()];

// Per-type ignore rules
options.Json.ConfigureIgnoreRules = rules =>
{
    rules.IgnoreProperty<User>(u => u.Password, JsonIgnoreCondition.Always);
    rules.IgnoreProperty<User>(u => u.RefreshToken, JsonIgnoreCondition.Always);
};
```
