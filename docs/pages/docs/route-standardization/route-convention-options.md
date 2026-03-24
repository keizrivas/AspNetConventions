---
title: "RouteConventionOptions"
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

### CasingStyle
**Namespace:** `AspNetConventions.Core.Enums`
**Accessed via:** [`RouteConventionOptions`](#routeconventionoptions)`.CaseStyle`

| Value | Index
|---|---|
| `KebabCase` *(default)* | `0` |
| `SnakeCase` | `1` |
| `CamelCase` | `2` |
| `PascalCase` | `3` |

### ControllerRouteOptions
**Namespace:** `AspNetConventions.Configuration.Options.Route`
**Accessed via:** [`RouteConventionOptions`](#routeconventionoptions)`.Controllers`

| Property | Type | Default | Description |
|---|---|---|---|

### MinimalApiRouteOptions
**Namespace:** `AspNetConventions.Configuration.Options.Route`
**Accessed via:** [`RouteConventionOptions`](#routeconventionoptions)`.RazorPages`

| Property | Type | Default | Description |
|---|---|---|---|

### RazorPagesRouteOptions
**Namespace:** `AspNetConventions.Configuration.Options.Route`
**Accessed via:** [`RouteConventionOptions`](#routeconventionoptions)`.MinimalApi`

| Property | Type | Default | Description |
|---|---|---|---|

### RouteConventionHooks
**Namespace:** `AspNetConventions.Core.Hooks`
**Accessed via:** [`RouteConventionOptions`](#routeconventionoptions)`.Hooks`

| Property | Type | Default | Description |
|---|---|---|---|

