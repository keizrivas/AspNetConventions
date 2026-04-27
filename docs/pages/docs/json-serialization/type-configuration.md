# Type Configuration

**AspNetConventions** lets you describe how each type should serialize using strongly-typed expressions, instead of decorating models with attributes. Per-type rules, global ignores, and reusable configuration classes all live behind one fluent API.

---

## Per-Type Property Configuration {#per-type-property-configuration}

Use [`options.Json.ConfigureTypes`](./configuration.md#ijsontypesconfigurationbuilder) to apply fine-grained rules to specific types using strongly-typed expressions.

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<User>(type =>
    {
        type.Property(x => x.Id).Order(0);
        type.Property(x => x.UserName).Name("username").Order(1);
        type.Property(x => x.Password).Ignore();
        type.Property(x => x.MiddleName).Ignore(IgnoreCondition.WhenWritingNull);
    });
};
```

### Renaming Properties {#renaming-properties}

Override the serialized name of a specific property. The explicit name takes full precedence over the global [`CaseStyle`](./serialization-options.md#casing-style):

```csharp
cfg.Type<Product>(type =>
{
    type.Property(x => x.InternalSku).Name("sku");
    type.Property(x => x.DisplayName).Name("name");
    type.Property(x => x.ListPriceCents).Name("price");
});
```

**Result:**
```json
{ "sku": "ABC-001", "name": "Widget Pro", "price": 2999 }
```

### Ordering Properties {#ordering-properties}

Control the order in which properties appear in the serialized output. Lower values are written first:

```csharp
cfg.Type<UserResponse>(type =>
{
    type.Property(x => x.Id).Order(0);
    type.Property(x => x.Name).Order(1);
    type.Property(x => x.Email).Order(2);
    type.Property(x => x.CreatedAt).Order(3);
});
```

**Best Practice: Order Every Property**
This behaviour is inherited from `System.Text.Json` / [`SystemTextJsonAdapter`](./configuration.md#configureadapter) (default adapter).
For **deterministic, predictable output,** assign an explicit `Order()` to every property:

| Scenario | Recommendation |
| --- | --- |
| Full control over property order | Assign explicit order to every property |
| Pin one property to the top | Use `Order(-1)` for that property |
| Pin one property to the bottom | Use `Order(int.MaxValue)` |
| Unordered, rely on declaration order | Avoid mixing ordered and unordered properties |

### Ignoring Properties {#ignoring-properties}

Exclude a property from serialization using an [`IgnoreCondition`](./configuration.md#ignorecondition):

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

`Ignore()` without arguments defaults to [`IgnoreCondition`{.code-left}](./configuration.md#ignorecondition)`.Always`{.code-right}.

### Chaining Rules {#chaining-rules}

All [`IJsonPropertyRuleBuilder`](./configuration.md#ijsonpropertyrulebuilder) methods return `this` and can be chained in any combination:

```csharp
cfg.Type<OrderSummary>(type =>
{
    type.Property(x => x.OrderId).Name("id").Order(0);
    type.Property(x => x.CustomerName).Order(1);
    type.Property(x => x.TotalAmount).Name("total").Order(2);
    type.Property(x => x.Notes).Order(3).Ignore(IgnoreCondition.WhenWritingNull);
    type.Property(x => x.InternalReference).Ignore();
});
```

---

## Open Generic Type Configuration {#open-generic-type-configuration}

Rules for open generic types (e.g. `MyApiResponse<T>`) apply to every closed variant at runtime. Pass a closed instantiation as the template for expression-based property selection:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    // Rules apply to MyApiResponse<string>, MyApiResponse<User>, MyApiResponse<anything>
    cfg.OpenGenericType<MyApiResponse<object>>(type =>
    {
        type.Property(x => x.Data).Order(3);
        type.Property(x => x.InternalToken).Ignore();
    });
};
```

The expression is resolved against the template type; the rule is stored under the open generic definition (`MyApiResponse<>`) and matched against all closed variants at serialization time.

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

For applications with many types to configure, inline delegates in `ConfigureTypes` can grow unwieldy. **AspNetConventions** supports a class-based approach using [`JsonTypeConfiguration<T>`](./configuration.md#jsontypeconfiguration):

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

Or, preferably, register them all at once via [**assembly scanning**](#assembly-scanning).

---

## Assembly Scanning {#assembly-scanning}

`ScanAssemblies` discovers all non-abstract, non-generic subclasses of `JsonTypeConfigurationBase` (which includes both [`JsonTypeConfiguration<T>`](./configuration.md#jsontypeconfiguration) and [`JsonOpenGenericTypeConfiguration<T>`](./configuration.md#jsonopengenrictypeconfiguration)) and registers their rules automatically:

```csharp
options.Json.ScanAssemblies(typeof(UserConfiguration).Assembly);

// Multiple assemblies
options.Json.ScanAssemblies(
    typeof(UserConfiguration).Assembly,
    typeof(ExternalTypeConfiguration).Assembly
);
```

No manual registration needed. Add a new [`JsonTypeConfiguration<T>`](./configuration.md#jsontypeconfiguration) class in a scanned assembly and it is picked up at the next startup.

---

## Open Generic Type Configuration (Class-Based) {#open-generic-type-configuration-class-based}

Use [`JsonOpenGenericTypeConfiguration<T>`](./configuration.md#jsonopengenrictypeconfiguration) to define class-based rules for open generic types:

```csharp
// T must be a closed generic — used only as a property-selection template
public class MyApiResponseConfiguration : JsonOpenGenericTypeConfiguration<MyApiResponse<object>>
{
    public override void Configure(IJsonTypeRuleBuilder<MyApiResponse<object>> rule)
    {
        rule.Property(x => x.Data).Order(3);
        rule.Property(x => x.TraceId).Ignore();
    }
}
```

Rules are stored under `MyApiResponse<>` (the open generic definition) and matched against `MyApiResponse<User>`, `MyApiResponse<Order>`, etc. at runtime.

---

## Resolution Priority {#resolution-priority}

When multiple rules could apply to the same property, **AspNetConventions** resolves them in this order (highest wins):

| Priority | Rule source | API |
|---|---|---|
| 1 (highest) | Type-level ignore | `cfg.IgnoreType<T>()` |
| 2 | Per-type per-property rule | `cfg.Type<T>(t => t.Property(x => x.Prop)...)` |
| 3 (lowest) | Global property-name ignore | `cfg.IgnorePropertyName("name")` |
