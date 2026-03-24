# Route Standardization

AspNetConventions automatically transforms route paths and route parameters across your entire ASP.NET Core application at startup — with zero runtime overhead and no per-controller or per-endpoint attributes required.

---

## Table of Contents

- [How It Works](#how-it-works)
- [Supported Endpoint Types](#supported-endpoint-types)
- [Casing Styles](#casing-styles)
- [Configuration](#configuration)
- [Examples](#examples)
  - [MVC Controllers](#mvc-controllers)
  - [Minimal APIs](#minimal-apis)
  - [Razor Pages](#razor-pages)
- [Parameter Binding](#parameter-binding)
- [Advanced Examples](#advanced-examples)
- [API Reference](#api-reference)
- [Architecture / How It Works Internally](#architecture--how-it-works-internally)
- [FAQ & Troubleshooting](#faq--troubleshooting)

---

## How It Works

Route standardization is applied at application startup using ASP.NET Core's built-in convention system. No middleware intercepts requests at runtime — routes are rewritten once when the application starts, which means there is no performance cost per request.

Both the **route path segments** and **route parameter names** (e.g. `{UserId}`) are transformed. The original parameter names are preserved internally so model binding continues to work without any changes to your action method signatures.

---

## Supported Endpoint Types

| Endpoint Type | Supported |
|---|---|
| MVC Controllers (`ControllerBase`) | ✅ |
| Minimal APIs (`app.MapGet`, etc.) | ✅ |
| Razor Pages | ✅ |

---

## Casing Styles

| Style | Example route | Example parameter |
|---|---|---|
| `KebabCase` *(default)* | `/get-user-by-id` | `{user-id}` |
| `SnakeCase` | `/get_user_by_id` | `{user_id}` |
| `CamelCase` | `/getUserById` | `{userId}` |
| `PascalCase` | `/GetUserById` | `{UserId}` |

> **Note:** `PascalCase` is effectively a no-op since ASP.NET Core routes are already PascalCase by convention. It is included for completeness when you need to be explicit about the convention.

---

## Configuration

### MVC Controllers & Razor Pages

Call `.AddAspNetConventions()` on your `IMvcBuilder`:

```csharp
// Default (kebab-case)
builder.Services
    .AddControllers()
    .AddAspNetConventions();

// Custom casing style
builder.Services
    .AddControllersWithViews()
    .AddAspNetConventions(options =>
    {
        options.Routes.CaseStyle = CasingStyle.SnakeCase;
    });
```

### Minimal APIs

Call `app.UseAspNetConventions()` after `app.Build()`:

```csharp
var app = builder.Build();

app.UseAspNetConventions();

app.MapGet("/WeatherForecast/{CityName}", (string CityName) =>
    Results.Ok(new { city = CityName }));
// ✅ Result: GET /weather-forecast/{city-name}

app.Run();
```

### Combining Both

```csharp
builder.Services
    .AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Routes.CaseStyle = CasingStyle.SnakeCase;
    });

var app = builder.Build();

app.UseAspNetConventions();  // applies the same casing to Minimal APIs
```

---

## Examples

### MVC Controllers

**Before:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    [HttpGet("GetById/{UserId}")]
    public IActionResult GetById(int UserId) => Ok(UserId);

    [HttpPost("CreateUserAccount")]
    public IActionResult CreateUserAccount([FromBody] CreateUserRequest request) => Ok();
}
```

**After (kebab-case):**
```
GET  /api/user-profile/get-by-id/{user-id}
POST /api/user-profile/create-user-account
```

The `UserId` parameter binding still works — you don't rename anything in your method signature.

---

### Minimal APIs

```csharp
app.UseAspNetConventions();

app.MapGet("/WeatherForecast/{CityName}", (string CityName) =>
    Results.Ok(new { city = CityName }));
// ✅ GET /weather-forecast/{city-name}

app.MapPost("/CreateOrder/{StoreId}", (int StoreId, CreateOrderRequest body) =>
    Results.Created($"/orders/{StoreId}", body));
// ✅ POST /create-order/{store-id}
```

---

### Razor Pages

Route transformation applies to page paths and any route parameters defined via `@page` directives or `OnGet` / `OnPost` parameters.

```csharp
// Pages/UserProfile/EditAddress.cshtml.cs
public class EditAddressModel : PageModel
{
    public void OnGet(int UserId, int AddressId) { }
}
// ✅ Result: /user-profile/edit-address/{user-id}/{address-id}
```

---

## Parameter Binding

Route parameter names are transformed in the URL, but **model binding is not broken**. AspNetConventions registers a binding alias under the hood so ASP.NET Core maps the incoming `{user-id}` parameter back to the `UserId` argument on your action method without any extra work on your part.

```csharp
// Route exposed as: GET /api/orders/get-by-user/{user-id}
[HttpGet("GetByUser/{UserId}")]
public IActionResult GetByUser(int UserId)  // ← still binds correctly
{
    return Ok(UserId);
}
```

---

## Advanced Examples

### Snake case across the board

```csharp
builder.Services
    .AddControllersWithViews()
    .AddAspNetConventions(options =>
    {
        options.Routes.CaseStyle = CasingStyle.SnakeCase;
    });
```

```
GET  /api/user_profile/get_by_id/{user_id}
POST /api/user_profile/create_user_account
```

### Multiple controllers with mixed depth

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class OrderManagementController : ControllerBase
{
    [HttpGet("GetPendingOrders/{CustomerId}/{StoreId}")]
    public IActionResult GetPendingOrders(int CustomerId, int StoreId) => Ok();
}
// ✅ GET /api/v1/order-management/get-pending-orders/{customer-id}/{store-id}
```

Segments that are already lowercase (like `api`, `v1`) are left unchanged.

---

## API Reference

### `CasingStyle` enum

```csharp
namespace AspNetConventions.Core.Enums;

public enum CasingStyle
{
    KebabCase,   // default
    SnakeCase,
    CamelCase,
    PascalCase,
}
```

### `RouteConventionOptions`

| Property | Type | Default | Description |
|---|---|---|---|
| `CaseStyle` | `CasingStyle` | `KebabCase` | The casing style applied to all route segments and parameters |

### Extension methods

| Method | Description |
|---|---|
| `IMvcBuilder.AddAspNetConventions()` | Enables conventions for MVC Controllers and Razor Pages |
| `IMvcBuilder.AddAspNetConventions(Action<AspNetConventionsOptions>)` | Same, with configuration callback |
| `WebApplication.UseAspNetConventions()` | Enables conventions for Minimal APIs |

---

## Architecture / How It Works Internally

Route standardization hooks into ASP.NET Core's **application model** for MVC/Razor Pages and the **endpoint data source** for Minimal APIs.

For MVC Controllers, AspNetConventions registers an `IApplicationModelConvention` that iterates over all `ControllerModel`, `ActionModel`, and `ParameterModel` objects at startup and rewrites their route templates. For Minimal APIs, it wraps the endpoint data source and rewrites the route patterns before the routing middleware processes them.

Because all transformation happens before the first request is served, there is **no per-request overhead**.

```
Startup
  │
  ├─ MVC: IApplicationModelConvention
  │     └─ Rewrites ControllerModel / ActionModel / ParameterModel routes
  │
  └─ Minimal APIs: Endpoint data source wrapper
        └─ Rewrites RoutePattern segments and parameter names
```

---

## FAQ & Troubleshooting

**Q: My custom route attribute (`[Route("my-custom/path")]`) is being transformed — I don't want that.**

Route transformation applies to all route templates by default. If you have a segment that should remain unchanged, use all-lowercase for that segment already — the transformer does not touch segments that are already in the target casing.

---

**Q: Does this work with versioned APIs (e.g. `Asp.Versioning`)?**

Yes. Version segments like `v1`, `v2` are already lowercase and are left untouched by the transformer.

---

**Q: Route parameters with numbers (e.g. `{Address1}`) — what happens?**

Alphanumeric segments are handled correctly: `{Address1}` → `{address-1}` in kebab-case, `{address_1}` in snake_case.

---

**Q: I'm using Razor Pages with area routing. Does it work?**

Yes. Area prefixes are transformed the same way as page path segments.

---

**Q: Does transformation apply to `[HttpGet]` constraint strings like `{id:int}`?**

Yes. Parameter name constraints (e.g. `{UserId:int}`) are preserved — only the parameter name portion is transformed: `{UserId:int}` → `{user-id:int}`.
