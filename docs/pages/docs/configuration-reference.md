# Configuration Reference

Configuration options are set through the `.AddAspNetConventions()` callback on `IMvcBuilder`, or via `.UseAspNetConventions()` for `WebApplication` setups.

```csharp
builder.Services
    .AddControllers()
    .AddAspNetConventions(options =>
    {
        // Custom options for MVC here...
    });
```
```csharp
app.UseAspNetConventions(options =>
    {
        // Convention options for Minimal API
    });
```

---

## AspNetConventionOptions

**Namespace:** `AspNetConventions.Configuration.Options`

The root configuration object passed to the [.AddAspNetConventions()](/docs/getting-started/#addaspnetconventions) and [.UseAspNetConventions()](/docs/getting-started/#useaspnetconventions) callbacks. All module options are accessed as properties of this object.

| Property | Type | Default | Description |
|---|---|---|---|
| `Route` | [`RouteConventionOptions`](/docs/route-standardization/route-convention-options) | `new()` | Route naming convention settings |
| `Response` | [`ResponseFormattingOptions`](/) | `new()` | Response formatting settings |
| `Json` | [`JsonSerializationOptions`](/) | `new()` | JSON serialization settings |
| `ExceptionHandling` | [`ExceptionHandlingOptions`](/) | `new()` | Exception handling settings |

**Example — configuring multiple modules at once:**

```csharp
builder.Services
    .AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Response.IncludeMetadata = false;
        options.Json.CaseStyle = CasingStyle.SnakeCase;
        options.Route.CaseStyle = CasingStyle.SnakeCase;
        options.ExceptionHandling.Mappers.Add(new CustomExceptionMapper());
    });
```

---

## RouteConventionOptions

**Namespace:** `AspNetConventions.Configuration.Options`
**Accessed via:** `options.Route`

Controls how route paths and parameters are transformed across all endpoint types.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables all route transformations |
| `CaseStyle` | `CasingStyle` | `KebabCase` | The casing style applied to all route segments and parameters |
| `CaseConverter` | `ICaseConverter?` | `null` | A custom case converter. When set, takes precedence over `CaseStyle` |
| `MaxRouteTemplateLength` | `int` | `2048` | Maximum allowed route template length in characters. Templates exceeding this are rejected at startup |
| `Controllers` | `ControllerRouteOptions` | `new()` | MVC Controller–specific route options |
| `RazorPages` | `RazorPagesRouteOptions` | `new()` | Razor Pages–specific route options |
| `MinimalApi` | `MinimalApiRouteOptions` | `new()` | Minimal API–specific route options |
| `Hooks` | `RouteConventionHooks` | `new()` | Hooks for intercepting the route transformation pipeline |

### CasingStyle values

| Value | Route example | Parameter example |
|---|---|---|
| `KebabCase` *(default)* | `/get-user-by-id` | `{user-id}` |
| `SnakeCase` | `/get_user_by_id` | `{user_id}` |
| `CamelCase` | `/getUserById` | `{userId}` |
| `PascalCase` | `/GetUserById` | `{UserId}` |

### Custom case converter

If none of the built-in `CasingStyle` values fit your needs, implement `ICaseConverter` and assign it to `CaseConverter`:

```csharp
public class MyConverter : ICaseConverter
{
    public string Convert(string value) => value.ToUpperInvariant();
}

options.Route.CaseConverter = new MyConverter();
// CaseStyle is ignored when CaseConverter is set
```

### Examples

```csharp
// Snake case, raise the template length cap for complex APIs
options.Route.CaseStyle              = CasingStyle.SnakeCase;
options.Route.MaxRouteTemplateLength = 4096;

// Disable route transformation entirely (opt-out)
options.Route.IsEnabled = false;
```

---

## ResponseFormattingOptions

**Namespace:** `AspNetConventions.Configuration.Options`
**Accessed via:** `options.Response`

Controls how successful and error responses are wrapped and formatted.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables response formatting. When `false`, responses pass through unmodified |
| `IncludeMetadata` | `bool` | `true` | When `true`, responses include a `metadata` block with HTTP method, path, timestamp, and trace ID |
| `Pagination` | `PaginationOptions` | `new()` | Controls how paginated/collection responses are formatted |
| `ErrorResponse` | `ErrorResponseOptions` | `new()` | Controls error-specific response formatting behaviour |
| `CollectionResultAdapters` | `HashSet<ICollectionResultAdapter>` | `[]` | Custom adapters for resolving and formatting specific collection types in paginated responses |
| `ResponseBuilder` | `IResponseBuilder?` | `null` | Custom builder for successful responses. Falls back to `DefaultApiResponseBuilder` when `null` |
| `ErrorResponseBuilder` | `IErrorResponseBuilder?` | `null` | Custom builder for error responses. Falls back to `DefaultApiErrorResponseBuilder` when `null` |
| `Hooks` | `ResponseFormattingHooks` | `new()` | Hooks for intercepting the response formatting pipeline |

### Replacing the response envelope

Assign a custom builder to replace the default response shape entirely:

```csharp
options.Response.ResponseBuilder      = new MyResponseBuilder();
options.Response.ErrorResponseBuilder = new MyErrorResponseBuilder();
```

Both can be replaced independently. See [Response Formatting](response-formatting.md) for full `IResponseBuilder` and `IErrorResponseBuilder` documentation.

### Disabling metadata

```csharp
// Responses will not include the metadata block
options.Response.IncludeMetadata = false;
```

### Disabling response formatting

```csharp
// All responses pass through as-is — no envelope wrapping
options.Response.IsEnabled = false;
```

---

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

---

## ExceptionHandlingOptions

**Namespace:** `AspNetConventions.Configuration.Options`
**Accessed via:** `options.ExceptionHandling`

Controls how unhandled exceptions are caught, mapped, and formatted across the application.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables global exception handling. When `false`, exceptions propagate normally |
| `Mappers` | `HashSet<IExceptionMapper>` | `[]` | Registered custom exception mappers. Evaluated before the built-in default mapper |
| `ExcludeStatusCodes` | `HashSet<HttpStatusCode>` | `[]` | HTTP status codes that should not be intercepted or reformatted by exception handling |
| `ExcludeException` | `HashSet<Type>` | `[]` | Exception types that should not be caught or handled — they propagate as normal |
| `Hooks` | `ExceptionHandlingHooks` | `new()` | Hooks for intercepting the exception handling pipeline |

### Registering custom mappers

Add instances of your `ExceptionMapper<T>` implementations directly to the `Mappers` set:

```csharp
options.ExceptionHandling.Mappers.Add(new NotFoundExceptionMapper());
options.ExceptionHandling.Mappers.Add(new PaymentFailedExceptionMapper());
```

Custom mappers are evaluated first, in insertion order. If no custom mapper matches the exception type, the built-in `DefaultExceptionMapper` is used as a fallback.

See [Exception Handling](exception-handling.md) for full `ExceptionMapper<T>` documentation.

### Excluding status codes

Prevent specific HTTP status codes from being intercepted — useful when other middleware already handles certain codes:

```csharp
options.ExceptionHandling.ExcludeStatusCodes.Add(HttpStatusCode.NotFound);
options.ExceptionHandling.ExcludeStatusCodes.Add(HttpStatusCode.Unauthorized);
```

### Excluding exception types

Allow specific exception types to bypass the handler entirely and propagate up the pipeline:

```csharp
options.ExceptionHandling.ExcludeException.Add(typeof(OperationCanceledException));
options.ExceptionHandling.ExcludeException.Add(typeof(TaskCanceledException));
```

### Disabling exception handling

```csharp
options.ExceptionHandling.IsEnabled = false;
```

### Mapper resolution order

When an exception is thrown, AspNetConventions resolves the mapper in this order:

1. Custom mappers in `Mappers`, evaluated in insertion order — first match wins.
2. Built-in `DefaultExceptionMapper` as the final fallback (handles `ArgumentNullException`, `ArgumentException`, `ValidationException`, and all other exceptions as `500`).
