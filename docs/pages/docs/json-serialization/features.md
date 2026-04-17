# Features

**AspNetConventions** provides a serializer-agnostic, fluent JSON configuration layer. All casing, property rules, and ignore conditions are declared once at startup through the library's own abstractions; the active serializer adapter translates them into its native format at initialization time.

::: callout info Newtonsoft.Json support
The current built-in adapter targets `System.Text.Json` (included with .NET, no extra dependency). Support for **Newtonsoft.Json** is planned for a future release. The public configuration API will remain unchanged — only the adapter needs to be swapped.
:::

---

## Casing Style {#casing-style}

Set the JSON property naming convention for all serialized types:

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

When none of the built-in `CasingStyle` options fit your convention, implement `ICaseConverter` and assign it to `options.Json.CaseConverter`. It overrides `CaseStyle` when set.

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

---

## Per-Type Property Configuration {#per-type-property-configuration}

Use `options.Json.ConfigureTypes` to apply fine-grained rules to specific types using strongly-typed expressions.

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<User>(type =>
    {
        type.Property(x => x.Id).Order(0);
        type.Property(x => x.UserName).Name("username");
        type.Property(x => x.Password).Ignore();
        type.Property(x => x.MiddleName).Ignore(IgnoreCondition.WhenWritingNull);
    });
};
```

### Renaming Properties {#renaming-properties}

Override the serialized name of a specific property. The explicit name takes full precedence over the global `CaseStyle`:

```csharp
cfg.Type<Product>(type =>
{
    type.Property(x => x.InternalSku).Name("sku");
    type.Property(x => x.DisplayName).Name("name");
});
```

**Result:**
```json
{ "sku": "ABC-001", "name": "Widget Pro" }
```

### Ordering Properties {#ordering-properties}

Control the order in which properties appear in the serialized output. Lower values are written first. Properties without an explicit order follow in their natural declaration order:

```csharp
cfg.Type<UserResponse>(type =>
{
    type.Property(x => x.Id).Order(0);
    type.Property(x => x.Name).Order(1);
    type.Property(x => x.Email).Order(2);
    type.Property(x => x.CreatedAt).Order(3);
});
```

### Ignoring Properties {#ignoring-properties}

Exclude a property from serialization using a `IgnoreCondition`:

```csharp
cfg.Type<User>(type =>
{
    // Never serialized
    type.Property(x => x.Password).Ignore();

    // Omitted when null
    type.Property(x => x.MiddleName).Ignore(IgnoreCondition.WhenWritingNull);

    // Omitted when equal to the type's default (0, false, null)
    type.Property(x => x.LoginAttempts).Ignore(IgnoreCondition.WhenWritingDefault);
});
```

`Ignore()` without arguments defaults to `IgnoreCondition.Always`.

| `IgnoreCondition` | Behaviour |
|---|---|
| `Always` *(default for `.Ignore()`)* | Property is never serialized |
| `WhenWritingNull` | Property is omitted when its value is `null` |
| `WhenWritingDefault` | Property is omitted when its value equals the type's default (`0`, `false`, `null`) |
| `Never` | Property is always serialized |

### Chaining Rules {#chaining-rules}

All `IJsonPropertyRuleBuilder` methods return `this` and can be chained:

```csharp
type.Property(x => x.OrderId).Name("id").Order(0);
```

---

## Open Generic Type Configuration {#open-generic-type-configuration}

Rules for open generic types (e.g. `ApiResponse<T>`) apply to every closed variant at runtime. Pass a closed instantiation as the template for expression-based property selection:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    // Rules apply to ApiResponse<string>, ApiResponse<User>, ApiResponse<anything>
    cfg.OpenGenericType<ApiResponse<object>>(type =>
    {
        type.Property(x => x.Data).Order(3);
        type.Property(x => x.InternalToken).Ignore();
    });
};
```

The expression is resolved against the template type; the rule is stored under the open generic definition (`ApiResponse<>`) and matched against all closed variants at serialization time.

---

## Global Ignore Rules {#global-ignore-rules}

When you want to suppress a property type or name across **all** types, use the global ignore methods instead of repeating `cfg.Type<T>()` for every affected type.

### Ignore by Type {#ignore-by-type}

Suppress every property whose **value type** is (or inherits from) `T`:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    // Hides every property of type Metadata (or any subclass) across all response types
    cfg.IgnoreType<Metadata>();

    // Hides nullable or non-nullable DateTimeOffset properties everywhere
    cfg.IgnoreType<DateTimeOffset>(IgnoreCondition.WhenWritingDefault);
};
```

`IgnoreType<T>()` has the **highest priority** — it overrides any per-type per-property rule for the same property.

### Ignore by Property Name {#ignore-by-property-name}

Suppress any property whose name matches a given string, regardless of which type it belongs to:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    // Hides "statusCode" / "StatusCode" on every type that has it
    cfg.IgnorePropertyName("StatusCode");

    // Hides "internalRef" everywhere, only when null
    cfg.IgnorePropertyName("internalRef", IgnoreCondition.WhenWritingNull);
};
```

The match is **case-insensitive** and tries the CLR name first, then the JSON-transformed name (e.g. `StatusCode` → `status_code`). A more specific per-type per-property rule takes precedence over this global rule.

---

## Class-Based Configuration {#class-based-configuration}

For applications with many types to configure, inline delegates in `ConfigureTypes` can grow unwieldy. **AspNetConventions** supports a class-based approach using `JsonTypeConfiguration<T>`:

```csharp
public class UserConfiguration : JsonTypeConfiguration<User>
{
    public override void Configure(IJsonTypeRuleBuilder<User> rule)
    {
        rule.Property(x => x.Id).Order(0);
        rule.Property(x => x.UserName).Order(1);
        rule.Property(x => x.Password).Ignore();
        rule.Property(x => x.RefreshToken).Ignore();
    }
}

public class OrderConfiguration : JsonTypeConfiguration<Order>
{
    public override void Configure(IJsonTypeRuleBuilder<Order> rule)
    {
        rule.Property(x => x.OrderId).Name("id").Order(0);
        rule.Property(x => x.InternalReference).Ignore();
        rule.Property(x => x.Notes).Ignore(IgnoreCondition.WhenWritingNull);
    }
}
```

Register configurations individually via `ConfigureTypes`:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<User>(type => new UserConfiguration().Configure(type));
};
```

Or, preferably, register them all at once via **assembly scanning**.

---

## Assembly Scanning {#assembly-scanning}

`ScanAssemblies` discovers all non-abstract, non-generic subclasses of `JsonTypeConfigurationBase` (which includes both `JsonTypeConfiguration<T>` and `JsonOpenGenericTypeConfiguration<T>`) and registers their rules automatically:

```csharp
options.Json.ScanAssemblies(typeof(UserConfiguration).Assembly);

// Multiple assemblies
options.Json.ScanAssemblies(
    typeof(UserConfiguration).Assembly,
    typeof(ExternalTypeConfiguration).Assembly
);
```

No manual registration needed. Add a new `JsonTypeConfiguration<T>` class in a scanned assembly and it is picked up at the next startup.

---

## Open Generic Type Configuration (Class-Based) {#open-generic-type-configuration-class-based}

Use `JsonOpenGenericTypeConfiguration<T>` to define class-based rules for open generic types:

```csharp
// T must be a closed generic — used only as a property-selection template
public class ApiResponseConfiguration : JsonOpenGenericTypeConfiguration<ApiResponse<object>>
{
    public override void Configure(IJsonTypeRuleBuilder<ApiResponse<object>> rule)
    {
        rule.Property(x => x.Data).Order(3);
        rule.Property(x => x.TraceId).Ignore();
    }
}
```

Rules are stored under `ApiResponse<>` (the open generic definition) and matched against `ApiResponse<User>`, `ApiResponse<Order>`, etc. at runtime.

---

## Custom Serializer Adapter {#custom-serializer-adapter}

Use `ConfigureAdapter<TAdapter, TOptions>` to swap the active serializer adapter or to reach low-level settings of the current adapter that are not exposed through the standard `options.Json.*` API:

```csharp
// Reach native System.Text.Json settings not covered by the standard API
options.Json.ConfigureAdapter<SystemTextJsonAdapter, JsonSerializerOptions>(serializerOptions =>
{
    serializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
```

`ConfigureAdapter` is an escape hatch. Prefer the typed `options.Json.*` properties for anything available through the standard API; fall back to `ConfigureAdapter` only for adapter-specific settings.

---

## Serialization Hooks {#serialization-hooks}

`JsonSerializationHooks` provides four extension points across two phases:

**Startup-time hooks** (run once per type at initialization, then cached by the adapter):

```csharp
// Skip rule processing for internal types
options.Json.Hooks.ShouldSerializeType = type =>
    !type.Namespace?.StartsWith("MyApp.Internal") ?? true;

// Override property names programmatically
options.Json.Hooks.ResolvePropertyName = (clrName, type) =>
    clrName == "Id" ? "identifier" : null;

// Log resolved types and their final JSON property names
options.Json.Hooks.OnTypeResolved = (type, jsonNames) =>
    logger.LogDebug("[JSON] {Type}: {Props}", type.Name, string.Join(", ", jsonNames));
```

**Per-serialization hook** (called on every serialization — keep it fast):

```csharp
// Suppress empty strings from any type
options.Json.Hooks.ShouldSerializeProperty = (instance, value, clrName, type) =>
    value is not string s || s.Length > 0;
```

Property **names** are resolved at startup and cached by the adapter, so they cannot be changed per request through hooks. For per-request name changes, use a custom `ICaseConverter` backed by a scoped service.

See [`JsonSerializationHooks`](./configuration.md#jsonserializationhooks) for the full reference.

---

## Framework Defaults {#framework-defaults}

**AspNetConventions** applies a set of built-in rules to its own response types to ensure sensible defaults out of the box. These rules run before your `ConfigureTypes` delegate, so your configuration can always override them:

| Type | Property | Default rule |
|---|---|---|
| `ApiResponse` | `Metadata` | `WhenWritingNull` |
| `DefaultApiResponse` | `Pagination` | `WhenWritingNull` |
| `PaginationMetadata` | `Links` | `WhenWritingNull` |
| `PaginationMetadata` | `HasNextPage` | `WhenWritingNull` |
| `PaginationMetadata` | `HasPreviousPage` | `WhenWritingNull` |

These defaults keep response payloads lean: navigation links and pagination flags are only included when the response is actually paginated, and metadata blocks are omitted on responses that don't populate them.
