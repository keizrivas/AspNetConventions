# JSON Serialization

**AspNetConventions** provides a unified JSON configuration layer that applies consistently across your entire ASP.NET Core application — API responses, model serialization, and everything in between.

---

## Why JSON Serialization? {#why-json-serialization}

ASP.NET Core's JSON configuration is scattered: casing policies live on `JsonSerializerOptions`, per-property behavior requires `[JsonIgnore]` or custom converters, and property ordering is controlled attribute-by-attribute. There's no single, coherent place to describe how your types should serialize.

**Without AspNetConventions:**
```csharp
// Casing policy on AddControllers
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

// Property behavior scattered across model classes
public class User
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonIgnore]
    public string? Password { get; set; }

    [JsonPropertyName("user_name")]
    public string UserName { get; set; }
}
```

**With AspNetConventions:**
```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Json.CaseStyle = CasingStyle.SnakeCase;

        options.Json.ConfigureTypes = cfg =>
        {
            cfg.Type<User>(type =>
            {
                type.Property(x => x.Id).Order(0);
                type.Property(x => x.Password).Ignore();
                type.Property(x => x.UserName).Name("user_name");
            });
        };
    });
```

No attributes on your models. No scattered options. One place.

---

## Features {#features}

- **Global casing style** — Apply camelCase, snake_case, kebab-case, or PascalCase to all JSON property names application-wide
- **Per-type property rules** — Rename, reorder, or ignore specific properties on specific types using strongly-typed expressions
- **Open generic type support** — Define rules once for e.g. `MyClass<T>`, apply them to all closed variants at runtime
- **Class-based configuration** — Separate type configurations into dedicated classes, keeping `Program.cs` clean
- **Assembly scanning** — Auto-discover and register all configuration classes in one or more assemblies.
- **Global ignore rules** — Suppress a property type (`IgnoreType<T>`) or a property name (`IgnorePropertyName`) across all types at once
- **Zero per-request overhead** — All rules are compiled into a snapshot at startup.
- **Pluggable adapter** — Swap the underlying serializer via `ConfigureAdapter<TAdapter, TOptions>`

---

## Before & After {#before-after}

::: tabs

== tab "Casing"

**Without AspNetConventions:**
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
```

**With AspNetConventions:**
```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Json.CaseStyle = CasingStyle.SnakeCase;
    });
```

== tab "Per-Type Rules"

**Without AspNetConventions:**
```csharp
public class OrderSummary
{
    [JsonPropertyName("id")]
    [JsonPropertyOrder(0)]
    public Guid OrderId { get; set; }

    [JsonPropertyOrder(1)]
    public string CustomerName { get; set; }

    [JsonIgnore]
    public string InternalReference { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Notes { get; set; }
}
```

**With AspNetConventions:**
```csharp
cfg.Type<OrderSummary>(type =>
{
    type.Property(x => x.OrderId).Name("id").Order(0);
    type.Property(x => x.CustomerName).Order(1);
    type.Property(x => x.InternalReference).Ignore();
    type.Property(x => x.Notes).Ignore(IgnoreCondition.WhenWritingNull);
});
```

Your model class stays clean.

== tab "Class-Based Config"

**Inline (Program.cs gets crowded):**
```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<User>(...);
    cfg.Type<Order>(...);
    cfg.Type<Product>(...);
    // grows without bound
};
```

**Class-based (scales cleanly):**
```csharp
public class UserConfiguration : JsonTypeConfiguration<User>
{
    public override void Configure(IJsonTypeRuleBuilder<User> rule)
    {
        rule.Property(x => x.Id).Order(0);
        rule.Property(x => x.Password).Ignore();
    }
}

// Register individually or via assembly scanning
options.Json.ScanAssemblies(typeof(UserConfiguration).Assembly);
```

:::

---

## Startup Flow {#startup-flow}

All configuration happens **once at startup** — there's no per-request overhead:

```
Startup
  │
  ├─ ConfigureTypes delegate runs → rules collected
  │
  ├─ ScanAssemblies → discovers JsonTypeConfiguration<T> subclasses → rules collected
  │
  ├─ JsonTypesConfigurationBuilder.CreateSnapshot() → immutable JsonTypeRulesSnapshot
  │
  └─ JsonTypeInfoResolver(snapshot) registered as DefaultJsonTypeInfoResolver
        └─ Zero per-request cost
```

---

## Resolution Priority {#resolution-priority}

When multiple rules could apply to the same property, **AspNetConventions** resolves them in this order (highest wins):

| Priority | Rule source | API |
|---|---|---|
| 1 (highest) | Type-level ignore | `cfg.IgnoreType<T>()` |
| 2 | Per-type per-property rule | `cfg.Type<T>(t => t.Property(x => x.Prop)...)` |
| 3 (lowest) | Global property-name ignore | `cfg.IgnorePropertyName("name")` |
