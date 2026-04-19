# Configuration

Complete reference for all JSON Serialization configuration options.

---

## JsonSerializationOptions {#jsonserializationoptions}

**Namespace:** `AspNetConventions.Configuration.Options`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.Json`{.code-right}

Controls JSON serialization behaviour application-wide, covering both API response output and model serialization. The default underlying serializer is `System.Text.Json`.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables **AspNetConventions** JSON configuration. When `false`, the application's default serializer settings are used unchanged |
| `CaseStyle` | [`CasingStyle`](#casingstyle) | `CamelCase` | JSON property naming style applied globally |
| `CaseConverter` | `ICaseConverter?` | `null` | Custom case converter for property names. Takes precedence over `CaseStyle` when set |
| `ConfigureTypes` | `Action<`{.code-left}[`IJsonTypesConfigurationBuilder`{.code-left .code-right}](#ijsontypesconfigurationbuilder)`>?`{.code-right} | `null` | Delegate for registering per-type and global JSON serialization rules |
| `WriteIndented` | `bool` | `false` | When `true`, JSON output is pretty-printed with indentation |
| `CaseInsensitive` | `bool` | `false` | When `true`, property name matching during deserialization is case-insensitive |
| `AllowTrailingCommas` | `bool` | `false` | When `true`, trailing commas in JSON input are tolerated during deserialization |
| `MaxDepth` | `int` | `0` | Maximum depth of nested JSON objects. `0` uses the serializer's default (64 for `System.Text.Json`) |
| `UseStringEnumConverter` | `bool` | `true` | When `true`, enum values are serialized as strings using the active naming policy. When `false`, enums are serialized as integers |
| `NumberHandling` | [`NumberHandling`](#numberhandling) | `Strict` | Controls how numbers are read and written |
| `IgnoreCondition` | [`IgnoreCondition`](#ignorecondition) | `Never` | Global default ignore condition applied to all properties |
| `Converters` | `IReadOnlyList<object>` | `[]` | Custom `JsonConverter` instances added to the serializer |
| `Hooks` | [`JsonSerializationHooks`](#jsonserializationhooks) | `new()` | Hooks for intercepting the serialization pipeline |

### ScanAssemblies {#scanassemblies}

```csharp
public void ScanAssemblies(params Assembly[] assemblies)
```

Scans the specified assemblies for all concrete, non-generic subclasses of `JsonTypeConfigurationBase` (which includes [`JsonTypeConfiguration<T>`](#jsontypeconfiguration) and [`JsonOpenGenericTypeConfiguration<T>`](#jsonopengenrictypeconfiguration)) and registers their rules automatically.

**Usage:**
```csharp
options.Json.ScanAssemblies(typeof(UserConfiguration).Assembly);
```

### ConfigureAdapter {#configureadapter}

Use `ConfigureAdapter<TAdapter, TOptions>` to swap the active serializer adapter or to reach low-level settings of the current adapter that are not exposed through the standard `options.Json.*` API:

```csharp
public void ConfigureAdapter<TAdapter, TOptions>(Action<TOptions>? configure = null)
    where TAdapter : IJsonSerializerAdapter
```

Configures a custom serializer adapter. The built-in default is `SystemTextJsonAdapter`.

**Usage:**
```csharp
// Reach native System.Text.Json settings not covered by the standard API
options.Json.ConfigureAdapter<SystemTextJsonAdapter, JsonSerializerOptions>(serializerOptions =>
{
    serializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
```

### GetSerializerOptions {#getserializeroptions}

```csharp
public object GetSerializerOptions()
```

Returns the underlying serializer options object from the configured adapter. Useful for directly inspecting the final serializer options at startup.

---

## IJsonTypesConfigurationBuilder {#ijsontypesconfigurationbuilder}

**Namespace:** `AspNetConventions.Serialization.Configuration`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Json`{.code-left .code-right}](#jsonserializationoptions)`.ConfigureTypes = cfg => { ... }`{.code-right}

Provides the fluent API for registering rules across multiple types.

| Method | Returns | Description |
|---|---|---|
| `Type<T>(Action<`{.code-left}[`IJsonTypeRuleBuilder`{.code-left .code-right}](#ijsontyperulebuilder)`<T>>)`{.code-right} | [`IJsonTypesConfigurationBuilder`](#ijsontypesconfigurationbuilder) | Registers per-property rules for a specific closed type |
| `OpenGenericType<T>(Action<`{.code-left}[`IJsonTypeRuleBuilder`{.code-left .code-right}](#ijsontyperulebuilder)`<T>>)`{.code-right} | [`IJsonTypesConfigurationBuilder`](#ijsontypesconfigurationbuilder) | Registers per-property rules for an open generic type; `T` must itself be a generic type |
| `IgnoreType<T>(`{.code-left}[`IgnoreCondition`{.code-left .code-right}](#ignorecondition)`)`{.code-right} | [`IJsonTypesConfigurationBuilder`](#ijsontypesconfigurationbuilder) | Suppresses every property whose **value type** is `T` (or a subtype). Defaults to `Always`. Highest priority rule |
| `IgnorePropertyName(string, `{.code-left}[`IgnoreCondition`{.code-left .code-right}](#ignorecondition)`)`{.code-right} | [`IJsonTypesConfigurationBuilder`](#ijsontypesconfigurationbuilder) | Suppresses any property matching the given name across all types. Case-insensitive. Defaults to `Always` |

All methods return [`IJsonTypesConfigurationBuilder`](#ijsontypesconfigurationbuilder) for chaining:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg
        .Type<User>(t => t.Property(x => x.Password).Ignore())
        .Type<Order>(t => t.Property(x => x.InternalRef).Ignore())
        .IgnorePropertyName("StatusCode")
        .IgnoreType<AuditTrail>();
};
```

---

## IJsonTypeRuleBuilder\<T\> {#ijsontyperulebuilder}

**Namespace:** `AspNetConventions.Serialization.Configuration`

Provides property selection for a specific type `T`.

| Method | Returns | Description |
|---|---|---|
| `Property<TProp>(Expression<Func<T, TProp>>)` | [`IJsonPropertyRuleBuilder`](#ijsonpropertyrulebuilder) | Selects a property of `T` using a strongly-typed lambda expression |

```csharp
cfg.Type<User>(type =>
{
    type.Property(x => x.Id);          // selects User.Id
    type.Property(x => x.UserName);    // selects User.UserName
});
```

---

## IJsonPropertyRuleBuilder {#ijsonpropertyrulebuilder}

**Namespace:** `AspNetConventions.Serialization.Configuration`

Configures the selected property. All methods return [`IJsonPropertyRuleBuilder`](#ijsonpropertyrulebuilder) for chaining.

| Method | Returns | Description |
|---|---|---|
| `Ignore(`{.code-left}[`IgnoreCondition`{.code-left .code-right}](#ignorecondition)`)`{.code-right} | [`IJsonPropertyRuleBuilder`](#ijsonpropertyrulebuilder) | Sets the ignore condition. Defaults to [`IgnoreCondition`{.code-left}](#ignorecondition)`.Always`{.code-right} |
| `Name(string)` | [`IJsonPropertyRuleBuilder`](#ijsonpropertyrulebuilder) | Overrides the serialized JSON property name. Takes precedence over [`CaseStyle`](#casingstyle) |
| `Order(int)` | [`IJsonPropertyRuleBuilder`](#ijsonpropertyrulebuilder) | Sets the serialization order. Lower values are written first |

**Chaining example:**
```csharp
type.Property(x => x.OrderId).Name("id").Order(0);
```

---

## JsonTypeConfiguration\<T\> {#jsontypeconfiguration}

**Namespace:** `AspNetConventions.Serialization.Configuration`

Abstract base class for class-based JSON configuration for a single closed type.

```csharp
public abstract class JsonTypeConfiguration<T> : JsonTypeConfigurationBase
{
    public abstract void Configure(IJsonTypeRuleBuilder<T> rule);
}
```

**Usage:**
```csharp
public class ProductConfiguration : JsonTypeConfiguration<Product>
{
    public override void Configure(IJsonTypeRuleBuilder<Product> rule)
    {
        rule.Property(x => x.CostPrice).Ignore();
        rule.Property(x => x.DisplayName).Name("name");
    }
}
```

Register via [`ScanAssemblies`](#scanassemblies) or inline:
```csharp
// Via assembly scanning (recommended)
options.Json.ScanAssemblies(typeof(ProductConfiguration).Assembly);

// Inline — inside the ConfigureTypes delegate
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<Product>(type => new ProductConfiguration().Configure(type));
};
```

---

## JsonOpenGenericTypeConfiguration\<T\> {#jsonopengenrictypeconfiguration}

**Namespace:** `AspNetConventions.Serialization.Configuration`

Abstract base class for class-based JSON configuration for open generic types. `T` must be a closed instantiation of the generic type used as a property-selection template.

```csharp
public abstract class JsonOpenGenericTypeConfiguration<T> : JsonTypeConfigurationBase
{
    public abstract void Configure(IJsonTypeRuleBuilder<T> rule);
}
```

**Usage:**
```csharp
// Rules are applied to MyApiResponse<string>, MyApiResponse<User>, MyApiResponse<anything>, etc.
public class MyApiResponseConfiguration : JsonOpenGenericTypeConfiguration<MyApiResponse<object>>
{
    public override void Configure(IJsonTypeRuleBuilder<MyApiResponse<object>> rule)
    {
        rule.Property(x => x.Data).Order(3);
        rule.Property(x => x.DebugInfo).Ignore();
    }
}
```

---

## CasingStyle {#casingstyle}

**Namespace:** `AspNetConventions.Core.Enums`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Json`{.code-left .code-right}](#jsonserializationoptions)`.CaseStyle`{.code-right}

| Value | Index | JSON property example |
|---|---|---|
| `CamelCase` *(default)* | `2` | `"userId"` |
| `SnakeCase` | `1` | `"user_id"` |
| `KebabCase` | `0` | `"user-id"` |
| `PascalCase` | `3` | `"UserId"` |

---

## NumberHandling {#numberhandling}

**Namespace:** `AspNetConventions.Core.Enums.Json`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Json`{.code-left .code-right}](#jsonserializationoptions)`.NumberHandling`{.code-right}

| Value | Description |
|---|---|
| `Strict` *(default)* | Numbers must be JSON number tokens. Strings are not accepted |
| `AllowReadingFromString` | Numbers can be read from quoted JSON strings (e.g. `"42"`) |
| `WriteAsString` | Numbers are written as quoted JSON strings |
| `AllowNamedFloatingPointLiterals` | Allows `NaN`, `Infinity`, and `-Infinity` as named floating-point values |

---

## IgnoreCondition {#ignorecondition}

**Namespace:** `AspNetConventions.Core.Enums.Json`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Json`{.code-left .code-right}](#jsonserializationoptions)`.IgnoreCondition`{.code-right}

This is the **global** default ignore condition that applies to all properties unless overridden by a more specific rule.

| Value | Description |
|---|---|
| `Never` *(default)* | All properties are always serialized |
| `WhenWritingNull` | Properties with `null` values are omitted from output |
| `WhenWritingDefault` | Properties with default values (`null`, `0`, `false`, etc.) are omitted |
| `Always` | All properties are always ignored (not useful as a global default) |

---

## JsonSerializationHooks {#jsonserializationhooks}

**Namespace:** `AspNetConventions.Core.Hooks`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Json`{.code-left .code-right}](#jsonserializationoptions)`.Hooks`{.code-right}

| Property | Delegate signature | When called | Description |
|---|---|---|---|
| `ShouldSerializeType` | `(Type) → bool` | Once per type at startup | Return `false` to skip all rule processing for a type |
| `ResolvePropertyName` | `(string clrName, Type) → string?` | Once per property at startup | Return a non-null string to override the serialized JSON name. Return `null` to keep the default |
| `ShouldSerializeProperty` | `(object instance, object? value, string clrName, Type) → bool` | Every serialization | Return `false` to suppress a property from the output. Called synchronously by the adapter on every serialization — keep it fast |
| `OnTypeResolved` | `(Type, IReadOnlyList<string> jsonNames) → void` | Once per type at startup | Receives the final list of JSON property names. Use for logging or diagnostics |

::: callout info Startup vs. per-serialization
`ShouldSerializeType`, `ResolvePropertyName`, and `OnTypeResolved` run once per type at initialization, then cached by the adapter.

`ShouldSerializeProperty` runs on **every serialization**. It is the only hook that can make per-call decisions (e.g. based on the current value or instance state). Property **names** cannot be changed per request because they are resolved at startup and cached by the adapter. For per-request name changes, use a custom [`ICaseConverter`](../configuration-reference.md) backed by a scoped service.
:::

**Examples:**

```csharp
// Suppress all properties on internal audit types at startup
options.Json.Hooks.ShouldSerializeType = type =>
    !type.Namespace?.StartsWith("MyApp.Internal") ?? true;

// Rename any property called "Id" to "identifier" across all types
options.Json.Hooks.ResolvePropertyName = (clrName, type) =>
    clrName == "Id" ? "identifier" : null;

// Suppress any property whose value is an empty string
options.Json.Hooks.ShouldSerializeProperty = (instance, value, clrName, type) =>
    value is not string s || s.Length > 0;

// Log all resolved types and their JSON property names at startup
options.Json.Hooks.OnTypeResolved = (type, jsonNames) =>
    Console.WriteLine($"[JSON] {type.Name}: {string.Join(", ", jsonNames)}");
```

---

## Default Values Reference {#default-values-reference}

| Option | Default |
|---|---|
| `Json.IsEnabled` | `true` |
| `Json.CaseStyle` | [`CamelCase`](#casingstyle) |
| `Json.CaseConverter` | `null` |
| `Json.ConfigureTypes` | `null` |
| `Json.WriteIndented` | `false` |
| `Json.CaseInsensitive` | `false` |
| `Json.AllowTrailingCommas` | `false` |
| `Json.MaxDepth` | `0` (serializer default: 64) |
| `Json.UseStringEnumConverter` | `true` |
| `Json.NumberHandling` | [`Strict`](#numberhandling) |
| `Json.IgnoreCondition` | [`Never`](#ignorecondition) |
| `Json.Converters` | `[]` |
