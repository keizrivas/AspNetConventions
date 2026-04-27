# Adapter & Extensions

The casing rules, per-type configuration, and serializer options described in the rest of this guide are translated into the underlying serializer's native format by an **adapter**.

---

## ConfigureAdapter {#configureadapter}

`ConfigureAdapter<TAdapter, TOptions>` selects the active serializer adapter and (optionally) reaches the low-level settings of that serializer that aren't exposed through the standard `options.Json.*` API.

### The default: System.Text.Json {#default-adapter}

Out of the box, **AspNetConventions** uses `SystemTextJsonAdapter`, which is built on `System.Text.Json` (included with .NET — no extra dependency).

You don't need to do anything to opt in. If `ConfigureAdapter` is never called, the framework wires up `SystemTextJsonAdapter` automatically:

```csharp
builder.Services
    .AddControllers()
    .AddAspNetConventions(options =>
    {
        // SystemTextJsonAdapter is used implicitly
        options.Json.CaseStyle = CasingStyle.SnakeCase;
    });
```

### Reaching native System.Text.Json settings {#native-stj-settings}

When you need a `JsonSerializerOptions` setting that isn't surfaced on `options.Json` — a custom encoder, native `DefaultIgnoreCondition`, or any other low-level knob — call `ConfigureAdapter` with the default adapter and configure the options directly:

```csharp
options.Json.ConfigureAdapter<SystemTextJsonAdapter, JsonSerializerOptions>(serializerOptions =>
{
    serializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    serializerOptions.Converters.Add(new MoneyJsonConverter());
});
```

The configure delegate runs **after** **AspNetConventions** has applied its own rules, so any settings you change here win.

---

## Serialization Hooks {#serialization-hooks}

`JsonSerializationHooks` provides four extension points across two phases.

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

---

## Examples {#examples}

Complete working examples demonstrating JSON Serialization configuration.

### Customizing ApiResponse {#customizing-apiresponse}

`ApiResponse` is the abstract base for all standard response envelopes. It exposes `Status`, `StatusCode`, `Message`, and `Metadata`. The example below drops `StatusCode` from the output and moves `Message` to the end, only when present:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<DefaultApiResponse>(type =>
    {
        type.Property(x => x.Status).Order(0);
        type.Property(x => x.StatusCode).Ignore();
        type.Property(x => x.Data).Order(1);
        type.Property(x => x.Pagination).Order(2);
        type.Property(x => x.Metadata).Order(3);
        type.Property(x => x.Message).Order(4).Ignore(IgnoreCondition.WhenWritingNull);
    });
};
```

**Serialized output (success, no message):**
```json
{
  "status": "success",
  "data": { "id": 1, "name": "Alice" }
}
```

---

### Customizing PaginationLinks {#customizing-paginationlinks}

`PaginationLinks` holds the navigation URLs for paginated responses. You can rename the properties to a shorter, client-friendly contract:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<PaginationLinks>(type =>
    {
        type.Property(x => x.FirstPageUrl).Name("first");
        type.Property(x => x.LastPageUrl).Name("last");
        type.Property(x => x.NextPageUrl).Name("next");
        type.Property(x => x.PreviousPageUrl).Name("prev");
    });
};
```

**Serialized output:**
```json
{
  "status": "success",
  "data": [...],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 25,
    "totalPages": 40,
    "totalRecords": 1000,
    "links": {
      "first": "/api/orders?page-number=1&page-size=25",
      "last": "/api/orders?page-number=40&page-size=25",
      "next": "/api/orders?page-number=2&page-size=25",
      "prev": null
    }
  }
}
```
