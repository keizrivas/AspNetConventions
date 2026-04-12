# Global JSON Config

AspNetConventions provides a unified, implementation-agnostic JSON configuration layer for your entire ASP.NET Core application. Set the serialization casing style, control property ordering, rename properties, and suppress sensitive fields — all in one place, covering both API responses and model serialization.

---

## How It Works {#how-it-works}

The JSON configuration in AspNetConventions is **serializer-agnostic**: it does not depend on `System.Text.Json` or `Newtonsoft.Json` directly. You configure your conventions through the AspNetConventions fluent API, and the library translates those conventions into the appropriate settings for the serializer your application uses, applying them globally at startup.

This means you can switch the underlying serializer without rewriting your conventions configuration.

---

## Casing Style {#casing-style}

Set the JSON property casing style for the entire application:

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
| `KebabCase` | `"user-id"` |
| `SnakeCase` | `"user_id"` |
| `CamelCase` *(default)* | `"userId"` |
| `PascalCase` | `"UserId"` |

This applies to all serialized objects: API response payloads, model objects, and any other JSON produced by the application.

---

## Per-Type Property Configuration {#per-type-property-configuration}

Use `options.Json.ConfigureTypes` to apply fine-grained property-level rules for specific types.

```csharp
builder.Services
    .AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Json.CaseStyle = CasingStyle.SnakeCase;

        options.Json.ConfigureTypes = cfg =>
        {
            cfg.Type<User>(type =>
            {
                type.Property(x => x.Id).Order(0);
                type.Property(x => x.RouteCode).Name("route");
                type.Property(x => x.Password).Ignore(JsonIgnoreCondition.Always);
            });
        };
    });
```

### Renaming Properties {#renaming-properties}

Override the serialized name of a specific property:

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

The `.Name()` override takes precedence over the global `CaseStyle`.

### Ordering Properties {#ordering-properties}

Control the order in which properties appear in the serialized output:

```csharp
cfg.Type<UserResponse>(type =>
{
    type.Property(x => x.Id).Order(0);
    type.Property(x => x.Name).Order(1);
    type.Property(x => x.Email).Order(2);
    type.Property(x => x.CreatedAt).Order(3);
});
```

Properties without an explicit order are placed after those with one, in their natural declaration order.

### Ignoring Properties {#ignoring-properties}

Exclude a property from serialization entirely using `JsonIgnoreCondition`:

```csharp
cfg.Type<User>(type =>
{
    // Always ignore — property never appears in any response
    type.Property(x => x.Password).Ignore(JsonIgnoreCondition.Always);

    // Ignore when null — omit the field if the value is null
    type.Property(x => x.MiddleName).Ignore(JsonIgnoreCondition.WhenWritingNull);

    // Ignore when default — omit 0, false, null, etc.
    type.Property(x => x.Score).Ignore(JsonIgnoreCondition.WhenWritingDefault);
});
```

| `JsonIgnoreCondition` | Behaviour |
|---|---|
| `Always` | Property is never serialized |
| `WhenWritingNull` | Property is omitted when its value is `null` |
| `WhenWritingDefault` | Property is omitted when its value equals the type's default (`0`, `false`, `null`) |
| `Never` | Property is always serialized (default) |

---

## Configuration Reference {#configuration-reference}

```csharp
builder.Services
    .AddControllers()
    .AddAspNetConventions(options =>
    {
        // Global JSON casing
        options.Json.CaseStyle = CasingStyle.SnakeCase;

        // Per-type property rules
        options.Json.ConfigureTypes = cfg =>
        {
            cfg.Type<T>(type =>
            {
                type.Property(x => x.PropertyName).Name("override_name");
                type.Property(x => x.PropertyName).Order(int);
                type.Property(x => x.PropertyName).Ignore(JsonIgnoreCondition);
            });
        };
    });
```

All calls inside `ConfigureTypes` are scoped to the type specified in `cfg.Type<T>()`. Multiple `.Type<T>()` calls for different types can be chained within the same `ConfigureTypes` delegate.

---

## Advanced Examples {#advanced-examples}

### Multiple types with different rules {#multiple-types-with-different-rules}

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<User>(type =>
    {
        type.Property(x => x.Id).Order(0);
        type.Property(x => x.Username).Order(1);
        type.Property(x => x.Password).Ignore(JsonIgnoreCondition.Always);
        type.Property(x => x.RefreshToken).Ignore(JsonIgnoreCondition.Always);
    });

    cfg.Type<Order>(type =>
    {
        type.Property(x => x.OrderId).Order(0).Name("id");
        type.Property(x => x.InternalReference).Ignore(JsonIgnoreCondition.Always);
        type.Property(x => x.Notes).Ignore(JsonIgnoreCondition.WhenWritingNull);
    });

    cfg.Type<Product>(type =>
    {
        type.Property(x => x.CostPrice).Ignore(JsonIgnoreCondition.Always);
        type.Property(x => x.DisplayName).Name("name");
    });
};
```

### Snake case globally with a renamed field {#snake-case-globally-with-a-renamed-field}

```csharp
options.Json.CaseStyle = CasingStyle.SnakeCase;

options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<ApiKey>(type =>
    {
        // Global snake_case would produce "api_key_value"
        // Override to expose as "key" instead
        type.Property(x => x.ApiKeyValue).Name("key");
        type.Property(x => x.HashedSecret).Ignore(JsonIgnoreCondition.Always);
    });
};
```

**Serialized output:**
```json
{
  "key_id": "abc123",
  "key": "sk-live-...",
  "created_at": "2024-01-15T10:30:00Z"
}
```

(`key_id` and `created_at` follow the global snake_case; `key` is the explicit override.)

### Ordering the standard API response fields {#ordering-the-standard-api-response-fields}

If you use a custom `IResponseBuilder` that returns a concrete class, you can control the field order in the envelope:

```csharp
public class ApiResponse
{
    public string Status     { get; set; }
    public int    StatusCode { get; set; }
    public string Message    { get; set; }
    public object Data       { get; set; }
}
```

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<ApiResponse>(type =>
    {
        type.Property(x => x.StatusCode).Order(0);
        type.Property(x => x.Status).Order(1);
        type.Property(x => x.Message).Order(2);
        type.Property(x => x.Data).Order(3);
    });
};
```

---

## API Reference {#api-reference}

### `JsonConventionOptions` {#jsonconventionoptions}

| Member | Type | Description |
|---|---|---|
| `CaseStyle` | `CasingStyle` | Global JSON property casing style. Defaults to `CamelCase`. |
| `ConfigureTypes` | `Action<JsonTypeConfigurator>?` | Delegate for per-type property configuration |

### `JsonTypeConfigurator` {#jsontypeconfigurator}

| Method | Description |
|---|---|
| `Type<T>(Action<TypeBuilder<T>>)` | Opens a configuration scope for the given type `T` |

### `TypeBuilder<T>` {#typebuilder}

| Method | Returns | Description |
|---|---|---|
| `Property(Expression<Func<T, object>>)` | `PropertyBuilder` | Selects a property for configuration |

### `PropertyBuilder` {#propertybuilder}

| Method | Returns | Description |
|---|---|---|
| `Name(string name)` | `PropertyBuilder` | Overrides the serialized property name |
| `Order(int order)` | `PropertyBuilder` | Sets the position of the property in the serialized output |
| `Ignore(JsonIgnoreCondition condition)` | `PropertyBuilder` | Sets the ignore condition for this property |

All `PropertyBuilder` methods are fluent and can be chained:
```csharp
type.Property(x => x.Id).Order(0).Name("identifier");
```

### `CasingStyle` enum {#casingstyle-enum}

```csharp
namespace AspNetConventions.Core.Enums;

public enum CasingStyle
{
    KebabCase,
    SnakeCase,
    CamelCase,
    PascalCase,
}
```

---

## Architecture / How It Works Internally {#architecture-how-it-works-internally}

At startup, AspNetConventions builds a **type metadata registry** from your `ConfigureTypes` configuration. This registry maps each `Type` to a set of property descriptors (name overrides, order, ignore conditions).

When the application initializes the serializer, AspNetConventions translates the registry into the appropriate serializer-specific options. For `System.Text.Json`, this means registering custom `JsonConverter` instances or `JsonSerializerOptions` policies. For `Newtonsoft.Json`, equivalent contract resolvers are produced.

Because the registry is built once at startup and applied to the serializer options object, **there is no per-serialization lookup overhead**.

```
Startup
  │
  ├─ ConfigureTypes delegate runs → TypeMetadataRegistry populated
  │
  ├─ Serializer detected (System.Text.Json / Newtonsoft.Json)
  │
  └─ Registry translated → JsonSerializerOptions / JsonSerializerSettings
        └─ Applied globally to all serialization in the application
```

---

## FAQ & Troubleshooting {#faq-troubleshooting}

**Q: I set `CaseStyle = SnakeCase` but my response properties are still camelCase.**

Make sure `.AddAspNetConventions()` is called *after* `.AddControllers()` or `.AddControllersWithViews()` on the same `IMvcBuilder` chain. Calling it on a separate `AddControllers()` call will not apply the options to the correct builder.

---

**Q: I used `.Name("my_name")` on a property but it's still being transformed by the global `CaseStyle`.**

Explicit `.Name()` overrides take full precedence over the global casing — the named value is used as-is, with no further transformation applied.

---

**Q: Does `ConfigureTypes` apply to types returned inside collections (e.g. `List<User>`)?**

Yes. The configuration applies to the type itself wherever it appears — directly, inside a list, inside another wrapper, or nested within a parent object.

---

**Q: Can I configure the same type in multiple `cfg.Type<T>()` calls?**

Each call to `cfg.Type<T>()` for the same type merges with previous calls. Later property rules override earlier ones for the same property. It is recommended to keep all rules for a given type in a single `cfg.Type<T>()` block for clarity.

---

**Q: Does this affect `System.Text.Json` attributes already on my models (e.g. `[JsonPropertyName]`)?**

Attributes on your model classes take the lowest priority. AspNetConventions conventions (`.Name()`, `.Ignore()`, `.Order()`) override attribute-based settings when there is a conflict. The global `CaseStyle` is applied after attribute discovery, so attributes without an explicit `.Name()` override will have the casing style applied to their declared or attribute-specified name.
