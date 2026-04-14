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

- **Versioned routes** — Preserve `/v1/`, `/v2/` segments as-is
- **Internal endpoints** — Skip transformation for admin or debug routes
- **Specific parameters** — Keep `{id}` unchanged while transforming others
- **Logging/Debugging** — Track all transformations at startup

### Quick Example {#quick-example}

```csharp
// Skip transformation for routes containing "/internal"
options.Route.Hooks.ShouldTransformRoute = (template, model) =>
    !template.Contains("/internal");

// Preserve version tokens (v1, v2, v3...)
options.Route.Hooks.ShouldTransformToken = token =>
    !Regex.IsMatch(token, @"^v\d+$");

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
using AspNetConventions.Core.Abstractions.Contracts;

public class UpperSnakeCaseConverter : ICaseConverter
{
    public string Convert(string value)
    {
        // GetUserById → GET_USER_BY_ID
        return string.Concat(
            value.Select((c, i) =>
                i > 0 && char.IsUpper(c) ? "_" + c : c.ToString())
        ).ToUpperInvariant();
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
