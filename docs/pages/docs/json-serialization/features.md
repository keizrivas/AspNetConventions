# Features

**AspNetConventions** provides a unified, fluent JSON configuration layer on top of `System.Text.Json`. You configure casing, property rules, and ignore conditions in one place at startup; the library translates them into a `DefaultJsonTypeInfoResolver` that is applied globally.

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
        type.Property(x => x.MiddleName).Ignore(JsonIgnoreCondition.WhenWritingNull);
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

Exclude a property from serialization using a `JsonIgnoreCondition`:

```csharp
cfg.Type<User>(type =>
{
    // Never serialized
    type.Property(x => x.Password).Ignore();

    // Omitted when null
    type.Property(x => x.MiddleName).Ignore(JsonIgnoreCondition.WhenWritingNull);

    // Omitted when equal to the type's default (0, false, null)
    type.Property(x => x.LoginAttempts).Ignore(JsonIgnoreCondition.WhenWritingDefault);
});
```

`Ignore()` without arguments defaults to `JsonIgnoreCondition.Always`.

| `JsonIgnoreCondition` | Behaviour |
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
    cfg.IgnoreType<DateTimeOffset>(JsonIgnoreCondition.WhenWritingDefault);
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
    cfg.IgnorePropertyName("internalRef", JsonIgnoreCondition.WhenWritingNull);
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
        rule.Property(x => x.Notes).Ignore(JsonIgnoreCondition.WhenWritingNull);
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

By default, **AspNetConventions** uses `SystemTextJsonAdapter` backed by `System.Text.Json`. Use `ConfigureAdapter<TAdapter, TOptions>` to plug in a different serializer or to access low-level `System.Text.Json` settings not exposed by the standard API:

```csharp
options.Json.ConfigureAdapter<SystemTextJsonAdapter, JsonSerializerOptions>(serializerOptions =>
{
    // Direct access to JsonSerializerOptions
    serializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
```

`ConfigureAdapter` is an escape hatch for settings outside the **AspNetConventions** abstraction. Prefer the typed `options.Json.*` properties for anything available through the standard API.

---

## Serialization Hooks {#serialization-hooks}

`JsonSerializationHooks` provides four extension points across two phases:

**Startup-time hooks** (run once per type, then cached by `System.Text.Json`):

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

Property **names** are baked into the cached `JsonTypeInfo` at startup, so they cannot be changed per request through hooks. For per-request name changes, use a custom `ICaseConverter` backed by a scoped service.

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
