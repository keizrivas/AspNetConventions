# Configuration

Complete reference for all Route Standardization configuration options.

---

## RouteConventionOptions {#routeconventionoptions}

**Namespace:** `AspNetConventions.Configuration.Options`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.Route`{.code-right}

Controls how route paths and parameters are transformed across all endpoint types.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables all route transformations |
| `CaseStyle` | [`CasingStyle`](#casingstyle) | `KebabCase` | The casing style applied to all route segments and parameters |
| `CaseConverter` | `ICaseConverter?` | `null` | A custom case converter. When set, takes precedence over `CaseStyle` |
| `MaxRouteTemplateLength` | `int` | `2048` | Maximum allowed route template length in characters. Templates exceeding this are rejected at startup |
| `Controllers` | [`ControllerRouteOptions`](#controllerrouteoptions) | `new()` | MVC Controller–specific route options |
| `RazorPages` | [`RazorPagesRouteOptions`](#razorpagesrouteoptions) | `new()` | Razor Pages–specific route options |
| `MinimalApi` | [`MinimalApiRouteOptions`](#minimalapirouteoptions) | `new()` | Minimal API–specific route options |
| `Hooks` | [`RouteConventionHooks`](#routeconventionhooks) | `new()` | Hooks for intercepting the route transformation pipeline |

### Disabling Route Standardization {#disabling-route-standardization}

```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        // Disables all route transformations
        options.Route.IsEnabled = false;
    });
```

---

## CasingStyle {#casingstyle}

**Namespace:** `AspNetConventions.Core.Enums`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Route`{.code-left .code-right}](#routeconventionoptions)`.CaseStyle`{.code-right}

| Value | Index | Route example | Parameter example |
|---|---|---|---|
| `KebabCase` *(default)* | `0` | `/get-user-by-id` | `{user-id}` |
| `SnakeCase` | `1` | `/get_user_by_id` | `{user_id}` |
| `CamelCase` | `2` | `/getUserById` | `{userId}` |
| `PascalCase` | `3` | `/GetUserById` | `{UserId}` |


See [Custom Case Converter](./index.md#custom-case-converter) for more information about override the default `CasingStyle` options.

---

## ControllerRouteOptions {#controllerrouteoptions}

**Namespace:** `AspNetConventions.Configuration.Options.Route`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Route`{.code-left .code-right}](#routeconventionoptions)`.Controllers`{.code-right}

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables route transformations for MVC controllers |
| `TransformParameterNames` | `bool` | `true` | Transforms route parameter names to the configured casing style |
| `TransformRouteTokens` | `bool` | `true` | Transforms route segment tokens  (e.g. `"[area]"`, `"[action]"`, `"[controller]"`, etc.) to the configured casing style |
| `PreserveExplicitBindingNames` | `bool` | `false` | When `true`, parameters with an explicit binding name are not transformed |
| `RemoveActionPrefixes` | `HashSet<string>` | `[]` | Action name prefixes stripped before the route is generated (e.g. `"Get"`, `"Post"`) |
| `RemoveActionSuffixes` | `HashSet<string>` | `[]` | Action name suffixes stripped before the route is generated (e.g. `"Async"`) |
| `ExcludeControllers` | `HashSet<string>` | `[]` | Controller names (without `"Controller"` suffix) excluded from transformation |
| `ExcludeAreas` | `HashSet<string>` | `[]` | Area names excluded from transformation |

**Example:**
```csharp
options.Route.Controllers.RemoveActionPrefixes.Add("Get");
options.Route.Controllers.RemoveActionPrefixes.Add("Post");
options.Route.Controllers.RemoveActionSuffixes.Add("Async");
options.Route.Controllers.ExcludeControllers.Add("Health");
options.Route.Controllers.ExcludeAreas.Add("Admin");
```
```
// Before: GET /api/profile/get-user/{user-id}
// After (prefix removed): GET /api/profile/user/{user-id}
```

---

## MinimalApiRouteOptions {#minimalapirouteoptions}

**Namespace:** `AspNetConventions.Configuration.Options.Route`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Route`{.code-left .code-right}](#routeconventionoptions)`.MinimalApi`{.code-right}

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables route transformations for Minimal API endpoints |
| `TransformRouteParameters` | `bool` | `false` | Transforms route parameter names to the configured casing style |
| `PreserveExplicitBindingNames` | `bool` | `false` | When `true`, parameters with an explicit `[FromRoute(Name = "...")]` binding name are not transformed |
| `ExcludeRoutePatterns` | `HashSet<string>` | `[]` | Route patterns excluded from transformation. Supports wildcards |
| `ExcludeTags` | `HashSet<string>` | `[]` | Endpoint tags excluded from transformation |

::: callout warning <u>Important:</u> Parameter Binding in Minimal APIs 
Minimal APIs handle parameter binding strictly by name. If the library automatically transforms a route parameter like `{userId}` to `{user-id}`, the underlying .NET binder will fail to locate the value unless explicitly told where to look.

**Enabling Parameter Transformation** 
To enable automatic casing for Minimal API route parameters, toggle this feature in your configuration:

```csharp
var api = app.UseAspNetConventions(options => {
    // Default is "false" to prevent binding breaks
    options.Route.MinimalApi.TransformRouteParameters = true;
});
```

**Option 1: Explicit Binding** 
Use the `[FromRoute(Name = "...")]` attribute to manually map the transformed segment name back to your C# parameter:

```csharp
// The URL becomes: /user-account/{user-id}
api.MapGet("/UserAccount/{userId}", ([FromRoute(Name = "user-id")] int userId) => {
    // Parameter binding works
    return Results.Ok(new { userId });
});
```

**Option 2: Preserve Explicit Binding Names**
Enable `PreserveExplicitBindingNames` to automatically skip transformation for any parameter that already has an explicit `[FromRoute(Name = "...")]`:

```csharp
var api = app.UseAspNetConventions(options => {
    options.Route.MinimalApi.TransformRouteParameters = true;
    options.Route.MinimalApi.PreserveExplicitBindingNames = true;
});
```
:::

**Example:**
```csharp
options.Route.MinimalApi.TransformRouteParameters = true;
options.Route.MinimalApi.PreserveExplicitBindingNames = true;
options.Route.MinimalApi.ExcludeRoutePatterns.Add("/health");
options.Route.MinimalApi.ExcludeRoutePatterns.Add("/metrics/*");
options.Route.MinimalApi.ExcludeTags.Add("internal");
```

---

## RazorPagesRouteOptions {#razorpagesrouteoptions}

**Namespace:** `AspNetConventions.Configuration.Options.Route`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Route`{.code-left .code-right}](#routeconventionoptions)`.RazorPages`{.code-right}

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables route transformations for Razor Pages |
| `TransformParameterNames` | `bool` | `true` | Transforms route parameter names to the configured casing style |
| `TransformPropertyNames` | `bool` | `true` | Transforms page model property names used in binding to the configured casing style |
| `PreserveExplicitBindingNames` | `bool` | `false` | When `true`, parameters with an explicit binding name are not transformed |
| `ExcludeFolders` | `HashSet<string>` | `[]` | Folder names excluded from route transformation |
| `ExcludePages` | `HashSet<string>` | `[]` | Page names (without extension) excluded from route transformation |

**Example:**
```csharp
options.Route.RazorPages.ExcludeFolders.Add("Admin");
options.Route.RazorPages.ExcludePages.Add("Privacy");
options.Route.RazorPages.PreserveExplicitBindingNames = true;
```

---

## RouteConventionHooks {#routeconventionhooks}

**Namespace:** `AspNetConventions.Core.Hooks`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Route`{.code-left .code-right}](#routeconventionoptions)`.Hooks`{.code-right}

Hooks provide fine-grained control over the transformation pipeline. Each hook is optional — set only the ones you need.

| Property | Delegate signature | Description |
|---|---|---|
| `ShouldTransformRoute` | `(string template, `{.code-left}[`RouteModelContext`{.code-left .code-right}](#routemodelcontext)` model) → bool`{.code-right} | Return `false` to skip transformation of a specific route template |
| `ShouldTransformParameter` | `(`{.code-left}[`RouteParameterContext`{.code-left .code-right}](#routeparametercontext)` model) → bool`{.code-right} | Return `false` to skip transformation of a specific route parameter |
| `ShouldTransformToken` | `(string token) → bool` | Return `false` to skip transformation of a specific route token/segment |
| `BeforeRouteTransform` | `(string route, `{.code-left}[`RouteModelContext`{.code-left .code-right}](#routemodelcontext)` model) → void`{.code-right} | Called before a route is transformed. Use for logging or pre-processing |
| `AfterRouteTransform` | `(string route, string originalRoute, `{.code-left}[`RouteModelContext`{.code-left .code-right}](#routemodelcontext)` model) → void`{.code-right} | Called after a route is transformed. Receives both the new and original template |

**Example — skip transformation for versioned or internal routes:**
```csharp
options.Route.Hooks.ShouldTransformRoute = (template, model) =>
{
    // Skip any route that starts with /internal
    return !template.StartsWith("/internal");
};

options.Route.Hooks.ShouldTransformToken = token =>
{
    // Preserve version tokens as-is (v1, v2, ...)
    return !System.Text.RegularExpressions.Regex.IsMatch(token, @"^v\d+$");
};

options.Route.Hooks.AfterRouteTransform = (route, originalRoute, model) =>
{
    Console.WriteLine($"Transformed: {originalRoute} → {route}");
};
```

### RouteModelContext {#routemodelcontext}

The `RouteModelContext` provides information about the endpoint being transformed:

| Property | Description |
|----------|-------------|
| `Identity.Kind` | Endpoint type: `MvcAction`, `RazorPage`, or `MinimalApi` |
| `DisplayName` | Human-readable route name |
| `Action` | MVC action model (null for others) |
| `Page` | Razor page model (null for others) |
| `RouteEndpointBuilder` | Minimal API endpoint builder (null for others) |

### RouteParameterContext {#routeparametercontext}

The `RouteParameterContext` provides information about the parameter being transformed:

| Property | Description |
|----------|-------------|
| [`RouteModelContext`](#routemodelcontext) | The route model context containing information about the parent route |
| `ParameterName` | The name of the route parameter |
| `DisplayName` | Human-readable parameter name |

---

## Default Values Reference {#default-values-reference}

| Option | Default |
|---|---|
| `Route.IsEnabled` | `true` |
| `Route.CaseStyle` | `KebabCase` |
| `Route.MaxRouteTemplateLength` | `2048` |
| `Controllers.IsEnabled` | `true` |
| `Controllers.TransformParameterNames` | `true` |
| `Controllers.TransformRouteTokens` | `true` |
| `Controllers.PreserveExplicitBindingNames` | `false` |
| `RazorPages.IsEnabled` | `true` |
| `RazorPages.TransformParameterNames` | `true` |
| `RazorPages.TransformPropertyNames` | `true` |
| `RazorPages.PreserveExplicitBindingNames` | `false` |
| `MinimalApi.IsEnabled` | `true` |
| `MinimalApi.TransformRouteParameters` | `false` |
| `MinimalApi.PreserveExplicitBindingNames` | `false` |