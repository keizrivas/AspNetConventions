# Overview

**AspNetConventions** is designed to be a "plug-and-play" solution that integrates seamlessly into the standard ASP.NET Core. By using these extension methods, you activate automatic route transformation, standardized response formatting, and global exception handling across your entire application.

## Supported Endpoint Types
The library provides consistent standardization across all primary ASP.NET Core endpoint styles:

| Endpoint Type | Supported |
|---|---|
| MVC Controllers | <svg color="#56ce8a" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-circle-check-icon lucide-circle-check"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg> |
| Minimal APIs | <svg color="#56ce8a" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-circle-check-icon lucide-circle-check"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg> |
| Razor Pages | <svg color="#56ce8a" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-circle-check-icon lucide-circle-check"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg> |

## Extension methods

### AddAspNetConventions
This method enables conventions for MVC Controllers and Razor Pages. It attaches to the `IMvcBuilder` to inject global filters and configuration logic.

```csharp
IMvcBuilder AddAspNetConventions(
    this IMvcBuilder builder,
    Action<AspNetConventionOptions>? configure = null)
```

| Parameter | type | Description |
|---|---|---|
| configure | `Action<AspNetConventionsOptions>?` | An optional action to configure conventions via [AspNetConventionsOptions](/docs/options/#aspnetconventionoptions) object. |

---

### UseAspNetConventions
This method enables conventions for Minimal APIs. It attaches to the `WebApplication` to inject global filters and configuration logic.

```csharp
WebApplication UseAspNetConventions(
    this WebApplication app,
    Action<AspNetConventionOptions>? configure = null)
```

| Parameter | type | Description |
|---|---|---|
| app | `WebApplication` | The current web application instance. |
| configure | `Action<[AspNetConventionsOptions]>?` | An optional action to configure conventions via [AspNetConventionsOptions](/docs/options/#aspnetconventionoptions) object. |

## Zero Configuration
**AspNetConventions** is designed with a "sensible defaults" philosophy. You can automatically achieve consistent standardization across your entire project without writing a single line of configuration code.

### The Power of One Line

Turn complex setup into a zero-effort implementation. This is all you need to do:

```csharp
// All conventions applied with default settings
builder.Services.AddControllers().AddAspNetConventions();
```

See [AspNetConventionsOptions](/docs/options/#aspnetconventionoptions) for more info about default settings.