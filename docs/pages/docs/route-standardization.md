# Route Standardization

**AspNetConventions** automatically transforms and standardizes your ASP.NET Core routes across **MVC Controllers**, **Minimal APIs**, and **Razor Pages**. It eliminates the inconsistency of manually maintaining route naming conventions across your entire application.

---

## Why Route Standardization? {#why-route-standardization}

REST APIs follow conventions where URLs use lowercase words separated by hyphens (kebab-case), but C# uses PascalCase for class and method names. This creates a mismatch:

```csharp
[ApiController]
[Route("/api/[controller]")]
public class UserProfileController : ControllerBase
{
    [HttpGet("[action]/{userId}")]
    public ActionResult GetById(int userId) => Ok(userId);
}
```

**Without AspNetConventions:**
```
GET /api/UserProfile/GetById/{userId}
```

**With AspNetConventions:**
```
GET /api/user-profile/get-by-id/{user-id}
```

---

## Features {#features}

- **Automatic route transformation** — Converts route segments to your preferred casing style
- **Parameter name transformation** — Ensures route parameters follow consistent naming (`{UserId}` → `{user-id}`)
- **Transparent model binding** — Parameters bind correctly without code changes
- **Per-endpoint-type configuration** — Customize behavior for MVC Controllers, Minimal APIs, and Razor Pages separately
- **Exclusion support** — Exclude specific routes, controllers, or pages from transformation
- **Custom hooks** — Extend the transformation behavior with custom logic

---

## Before & After {#before-after}

::: tabs

== tab "MVC Controllers"

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    [HttpGet("GetById/{userId}")]
    public ActionResult GetById(int userId) => Ok(userId);

    [HttpPost("CreateAccount")]
    public ActionResult CreateAccount([FromBody] CreateUserRequest request) => Ok();
}
```

**Routes generated:**
```
GET  /api/user-profile/get-by-id/{user-id}
POST /api/user-profile/create-account
```

== tab "Minimal APIs"

```csharp
var api = app.UseAspNetConventions();

api.MapGet("Inventory/GetOrder/{orderId}", (int orderId) =>
    Results.Ok(new { order = orderId }));

api.MapPost("Inventory/CreateOrder/{storeId}", (int storeId) =>
    Results.Created());
```

**Routes generated:**
```
GET  /inventory/get-order/{orderId}
POST /inventory/create-order/{storeId}
```

== tab "Razor Pages"

```csharp
// Pages/UserProfile/EditAddress.cshtml.cs
public class EditAddressModel : PageModel
{
    public void OnGet(int UserId, int AddressId) { }
}
```
```csharp
// Pages/UserProfile/EditAddress.cshtml
@page "{UserId:int}/{AddressId:int}"
@model EditAddressModel

<h2>Edit Address</h2>
```

**Route generated:**
```
GET /user-profile/edit-address/{user-id}/{address-id}
```

:::

---

## The Transformation Flow {#the-transformation-flow}

When your application starts, **AspNetConventions** processes all registered endpoints and applies transformations in this order:

```
1. Discover endpoints (Controllers, Minimal APIs, Razor Pages)
        ↓
2. Transform route templates (segments and tokens)
        ↓
3. Transform parameter names in route templates
        ↓
4. Register binding aliases for model binding
        ↓
5. Application starts with transformed routes
```

All transformations happen **once at startup** — there's no runtime overhead during request processing.

---

## Route Template Transformation {#route-template-transformation}

Route templates are broken into segments, and each segment is transformed according to your configured casing style.

**Original route template:**
```
api/[controller]/GetUserById/{UserId}
```

**Transformation steps:**
1. Split into segments: `api`, `[controller]`, `GetUserById`, `{UserId}`
2. Transform each segment:
   - `api` → `api` (already lowercase)
   - `[controller]` → resolves to `UserProfile` → `user-profile`
   - `GetUserById` → `get-user-by-id`
   - `{UserId}` → `{user-id}`

**Result:**
```
api/user-profile/get-user-by-id/{user-id}
```

---

## Global Route Prefix {#global-route-prefix}

For **MVC controllers**, you can configure a global prefix that is prepended to every controller route template — useful for centralizing a common base path like `api` or an API version segment such as `api/v{version:apiVersion}`.

**Configuration:**
```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Route.Controllers.RoutePrefix = "api/v{version:apiVersion}";
    });
```

**Controller:**
```csharp
[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult Get(int id) => Ok();
}
```

**Route generated:**
```
GET /api/v{version:apiVersion}/users/{id}
```

The prefix participates in the configured case conversion and is normalized at startup.

::: callout warning <u>Absolute Routes Bypass the Prefix</u>
Controller route templates that start with `/` or `~/` are treated as **absolute paths** and **skip** the global prefix entirely, matching ASP.NET's built-in absolute-route convention.

```csharp
options.Route.Controllers.RoutePrefix = "api";
```
```csharp
[Route("/health")]         // → /health           (prefix skipped)
[Route("~/diagnostics")]   // → ~/diagnostics     (prefix skipped)
[Route("[controller]")]    // → api/[controller]
```

Use this as an opt-out for endpoints that need to live outside the global prefix (health checks, diagnostics, well-known endpoints). For per-controller opt-out by name, use [`ExcludeControllers`](./route-standardization/configuration.md#controllerrouteoptions) instead.
:::

::: callout info <u>MVC Controllers Only</u>
`RoutePrefix` applies to **MVC controllers only**. For Minimal APIs, pass a prefix directly to [`.UseAspNetConventions()`](./getting-started.md#useaspnetconventions) method, it returns a prefixed `RouteGroupBuilder` for all subsequent endpoint registrations.

```csharp
var api = app.UseAspNetConventions("api/");

api.MapGet("/users/{id}", (int id) => Results.Ok());
// → GET /api/users/{id}
```
:::

---

## Parameter Binding {#parameter-binding}

When a parameter name is transformed in the URL, you might wonder: how does ASP.NET Core still bind the value to your C# parameter?

**AspNetConventions** registers **binding aliases** that map the transformed name back to your original parameter name.

```csharp
[HttpGet("GetOrder/{OrderId}")]
public ActionResult GetOrder(int OrderId)
{
    return Ok(OrderId);
}
```

**What happens:**
1. Route becomes `/get-order/{order-id}`
2. A binding alias is registered: `order-id` → `OrderId`
3. When a request arrives at `/get-order/123`:
   - ASP.NET Core extracts `123` from `{order-id}`
   - The alias maps it to the `OrderId` parameter
   - Your method receives `OrderId = 123`

This works seamlessly for all binding sources, see [Supported Binding Sources](./route-standardization/parameter-binding.md#supported-binding-sources) for more information. 

---

## URL Generation {#url-generation}

**AspNetConventions** also handles **outbound URL generation**. When you generate URLs using ASP.NET Core's built-in helpers, the transformed routes are used automatically.

```csharp
// In a controller
var url = Url.Action("GetOrder", "Orders", new { OrderId = 123 });
// Result: /api/orders/get-order/123

// In Razor
<a asp-page="/Orders/GetOrder" asp-route-order-id="123">View Order</a>
// Result: /orders/get-order/123

```

The route values you pass (`OrderId`) are automatically matched to the transformed parameter names (`order-id`) in the generated URL.

---

## Transformation Hooks {#transformation-hooks}

Hooks provide fine-grained control over the transformation pipeline. They allow you to conditionally skip transformation for specific routes, parameters, or tokens based on custom logic.

### When to Use Hooks {#when-to-use-hooks}

- **Acronyms in `[controller]` / `[action]` tokens** — Keep `JWT`, `OAuth`, `SSO` intact instead of being kebab-cased
- **Specific parameters** — Keep `{id}` unchanged while transforming others
- **Logging/Debugging** — Track all transformations at startup

::: callout info <u>Scope of `ShouldTransformToken`</u>
`ShouldTransformToken` is invoked by the `IOutboundParameterTransformer` pipeline **only for `[controller]`, `[action]`, and `[area]` token-replacement values** at URL generation / matching time. Static route segments (e.g. `api`, `v1`) are transformed at startup and do **not** pass through this hook.
:::

### Quick Example {#quick-example}

```csharp
// Preserve acronyms in resolved [controller] / [action] tokens
// e.g. OAuthController → "OAuth" stays "OAuth" instead of becoming "o-auth"
var preserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "JWT", "OAuth", "SSO", "API"
};
options.Route.Hooks.ShouldTransformToken = token => !preserved.Contains(token);

// Log all transformations
options.Route.Hooks.AfterRouteTransform = (newRoute, originalRoute, model) =>
    Console.WriteLine($"Transformed: {originalRoute} → {newRoute}");
```

For the complete list of available hooks and their signatures, see [`RouteConventionHooks`](./route-standardization/configuration.md#routeconventionhooks).

### Debugging Transformations {#debugging-transformations}

Use the `AfterRouteTransform` hook to log all transformations at startup:

```csharp
options.Route.Hooks.AfterRouteTransform = (newRoute, originalRoute, model) =>
{
    var type = model.Identity.Kind;
    
    _logger.LogDebug(
        "Route Transform: [{type}] {originalRoute} → {newRoute}",
        model.Identity.Kind,
        originalRoute,
        newRoute);
};
```

**Output:**
```
[MvcAction] api/UserProfile/GetById/{UserId} → api/user-profile/get-by-id/{user-id}
[MvcAction] api/UserProfile/CreateAccount → api/user-profile/create-account
[MinimalApi] /WeatherForecast/{CityName} → /weather-forecast/{city-name}
[RazorPage] UserProfile/EditAddress/{UserId} → user-profile/edit-address/{user-id}
```

---

## Custom Case Converter {#custom-case-converter}

If the built-in case styles don't fit your needs, implement `ICaseConverter`:

```csharp
using AspNetConventions.Extensions;
using AspNetConventions.Core.Abstractions.Contracts;

public class UpperSnakeCaseConverter : ICaseConverter
{
    public string Convert(string value)
    {
        // GetUserById → GET_USER_BY_ID
        return value.ToSnakeCase().ToUpperInvariant();
    }
}
```

**Usage:**
```csharp
options.Route.CaseConverter = new UpperSnakeCaseConverter();
// Note: CaseStyle is ignored when CaseConverter is set
```

**Result:**
```
GetUserById     → GET_USER_BY_ID
CreateOrder     → CREATE_ORDER
UserProfile     → USER_PROFILE
```
