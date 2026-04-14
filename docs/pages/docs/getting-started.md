# Overview

**AspNetConventions** is designed to be a "plug-and-play" solution that integrates seamlessly into the standard ASP.NET Core. By using these extension methods, you activate automatic route transformation, standardized response formatting, and global exception handling across your entire application.

## Supported Endpoint Types {#supported-endpoint-types}
The library provides consistent standardization across all primary ASP.NET Core endpoint styles:

| Endpoint Type | Supported |
|---|---|
| MVC Controllers | <svg color="#56ce8a" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-circle-check-icon lucide-circle-check"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg> |
| Minimal APIs | <svg color="#56ce8a" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-circle-check-icon lucide-circle-check"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg> |
| Razor Pages | <svg color="#56ce8a" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-circle-check-icon lucide-circle-check"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg> |

## Extension methods {#extension-methods}

### AddAspNetConventions {#addaspnetconventions}
This method enables conventions for MVC Controllers and Razor Pages. It attaches to the `IMvcBuilder` to inject global filters and configuration logic.

```csharp
IMvcBuilder AddAspNetConventions(
    this IMvcBuilder builder,
    Action<AspNetConventionOptions>? configure = null)
```

| Parameter | type | Description |
|---|---|---|
| configure | `Action<AspNetConventionsOptions>?` | An optional action to configure conventions via [AspNetConventionsOptions](./configuration-reference.md#aspnetconventionoptions) object. |

---

### UseAspNetConventions {#useaspnetconventions}
This method enables conventions for Minimal APIs. It returns a `RouteGroupBuilder` — map your endpoints on the returned group so that response formatting, route transformation, and exception handling are applied to them.

```csharp
RouteGroupBuilder UseAspNetConventions(
    this WebApplication app,
    string prefix = "",
    Action<AspNetConventionOptions>? configure = null)
```

| Parameter | Type | Description |
|---|---|---|
| `app` | `WebApplication` | The current web application instance. |
| `prefix` | `string` | An optional route prefix applied to all endpoints registered on the returned group. Defaults to `""` (root, no prefix). |
| `configure` | `Action<AspNetConventionsOptions>?` | An optional action to configure conventions via [AspNetConventionsOptions](./configuration-reference.md#aspnetconventionoptions). |

**Usage:**
```csharp
var api = app.UseAspNetConventions();

api.MapGet("/users", GetUsers);
api.MapPost("/users", CreateUser);

app.Run();
```

## Zero Configuration {#zero-configuration}
**AspNetConventions** is designed with a "sensible defaults" philosophy. You can automatically achieve consistent standardization across your entire project without writing a single line of configuration code.

### The Power of One Line {#the-power-of-one-line}

Turn complex setup into a zero-effort implementation. This is all you need to do:

```csharp
// All conventions applied with default settings
builder.Services.AddControllers().AddAspNetConventions();
```

See [AspNetConventionsOptions](./configuration-reference.md#aspnetconventionoptions) for more info about default settings.
