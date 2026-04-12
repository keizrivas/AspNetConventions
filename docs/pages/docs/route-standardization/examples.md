# Examples

Complete working examples demonstrating Route Standardization across MVC Controllers, Minimal APIs, and Razor Pages.

---

## MVC Controller API {#mvc-controller-api}

A complete REST API controller with various HTTP methods and parameter types.

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    // GET /api/user-profile/get-by-id/{user-id}
    [HttpGet("GetById/{UserId}")]
    public ActionResult GetById(int UserId) => Ok(new { UserId });

    // GET /api/user-profile/search?first-name=John&last-name=Doe
    [HttpGet("[action]")]
    public ActionResult Search([FromQuery] UserSearchRequest request) => Ok(request);

    // POST /api/user-profile/create-account
    [HttpPost("CreateAccount")]
    public ActionResult CreateAccount([FromBody] CreateUserRequest request) => Ok();

    // PUT /api/user-profile/update-settings/{user-id}
    [HttpPut("UpdateSettings/{UserId}")]
    public ActionResult UpdateSettings(int UserId, [FromBody] UserSettings settings) => Ok();

    // DELETE /api/user-profile/delete-account/{user-id}
    [HttpDelete("DeleteAccount/{UserId}")]
    public ActionResult DeleteAccount(int UserId) => Ok();
}

public class UserSearchRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class CreateUserRequest
{
    public string UserName { get; set; }
    public string EmailAddress { get; set; }
}

public class UserSettings
{
    public bool EnableNotifications { get; set; }
    public string PreferredLanguage { get; set; }
}
```

### Generated Routes {#generated-routes}

| HTTP Method | Original | Transformed |
|-------------|----------|-------------|
| GET | `/api/UserProfile/GetById/{UserId}` | `/api/user-profile/get-by-id/{user-id}` |
| GET | `/api/UserProfile/Search` | `/api/user-profile/search?first-name=&last-name=` |
| POST | `/api/UserProfile/CreateAccount` | `/api/user-profile/create-account` |
| PUT | `/api/UserProfile/UpdateSettings/{UserId}` | `/api/user-profile/update-settings/{user-id}` |
| DELETE | `/api/UserProfile/DeleteAccount/{UserId}` | `/api/user-profile/delete-account/{user-id}` |

---

## Minimal API {#minimal-api}

A complete Minimal API setup with route groups and exclusions.

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Configure with route prefix
var api = app.UseAspNetConventions("/api");

// GET /api/weather-forecast/{city-name}
api.MapGet("/WeatherForecast/{CityName}", (string CityName) =>
    Results.Ok(new { City = CityName, Temperature = 72 }));

// GET /api/order-history/{customer-id}
api.MapGet("/OrderHistory/{CustomerId}", (int CustomerId) =>
    Results.Ok(new { CustomerId, Orders = new[] { "Order1", "Order2" } }));

// POST /api/create-order
api.MapPost("/CreateOrder", (CreateOrderRequest request) =>
    Results.Created("/orders/123", request));

// GET /api/product-catalog/by-category/{category-id}
api.MapGet("/ProductCatalog/ByCategory/{CategoryId}", (int CategoryId) =>
    Results.Ok(new { CategoryId }));

// Excluded from transformation (tagged as internal)
api.MapGet("/health", () => Results.Ok("Healthy"))
   .WithTags("internal");

api.MapGet("/metrics", () => Results.Ok(new { uptime = "24h" }))
   .WithTags("internal");

app.Run();

public record CreateOrderRequest(string ProductName, int Quantity);
```

### Configuration {#configuration}

```csharp
var api = app.UseAspNetConventions("/api", options =>
{
    options.Route.CaseStyle = CasingStyle.KebabCase;
    options.Route.MinimalApi.ExcludeTags.Add("internal");
});
```

### Generated Routes {#generated-routes}

| HTTP Method | Original | Transformed |
|-------------|----------|-------------|
| GET | `/api/WeatherForecast/{CityName}` | `/api/weather-forecast/{city-name}` |
| GET | `/api/OrderHistory/{CustomerId}` | `/api/order-history/{customer-id}` |
| POST | `/api/CreateOrder` | `/api/create-order` |
| GET | `/api/ProductCatalog/ByCategory/{CategoryId}` | `/api/product-catalog/by-category/{category-id}` |
| GET | `/health` | `/health` (excluded) |
| GET | `/metrics` | `/metrics` (excluded) |

---

## Razor Pages {#razor-pages}

A Razor Pages example with route parameters and form binding.

### Page Model {#page-model}

```csharp
// Pages/UserProfile/EditAddress.cshtml.cs
public class EditAddressModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int AddressId { get; set; }

    [BindProperty]
    public AddressForm Address { get; set; } = new();

    public void OnGet()
    {
        // Load address data
    }

    public ActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        // Save address
        return RedirectToPage("/UserProfile/Index", new { UserId });
    }
}

public class AddressForm
{
    public string StreetAddress { get; set; }
    public string CityName { get; set; }
    public string PostalCode { get; set; }
    public string CountryCode { get; set; }
}
```

### Razor View {#razor-view}

```html
<!-- Pages/UserProfile/EditAddress.cshtml -->
@page "{UserId:int}/{AddressId:int}"
@model EditAddressModel

<h1>Edit Address</h1>

<form method="post">
    <div>
        <label for="street-address">Street Address</label>
        <input type="text" name="street-address" value="@Model.Address.StreetAddress" />
    </div>
    <div>
        <label for="city-name">City</label>
        <input type="text" name="city-name" value="@Model.Address.CityName" />
    </div>
    <div>
        <label for="postal-code">Postal Code</label>
        <input type="text" name="postal-code" value="@Model.Address.PostalCode" />
    </div>
    <div>
        <label for="country-code">Country</label>
        <input type="text" name="country-code" value="@Model.Address.CountryCode" />
    </div>
    <button type="submit">Save</button>
</form>
```

### Generated Routes {#generated-routes}

| Action | Original | Transformed |
|--------|----------|-------------|
| GET | `/UserProfile/EditAddress/{UserId}/{AddressId}` | `/user-profile/edit-address/{user-id}/{address-id}` |
| POST | `/UserProfile/EditAddress/{UserId}/{AddressId}` | `/user-profile/edit-address/{user-id}/{address-id}` |

### Form Fields {#form-fields}

| Original Property | Transformed Field Name |
|-------------------|------------------------|
| `StreetAddress` | `street-address` |
| `CityName` | `city-name` |
| `PostalCode` | `postal-code` |
| `CountryCode` | `country-code` |

---

## Mixed Endpoint Configuration {#mixed-endpoint-configuration}

Configure MVC Controllers and Minimal APIs with different settings in the same application.

```csharp
var builder = WebApplication.CreateBuilder(args);

// MVC Controllers with kebab-case and prefix removal
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Route.CaseStyle = CasingStyle.KebabCase;
        options.Route.Controllers.RemoveActionPrefixes.Add("Get");
        options.Route.Controllers.RemoveActionPrefixes.Add("Post");
        options.Route.Controllers.RemoveActionSuffixes.Add("Async");
        options.Route.Controllers.ExcludeControllers.Add("Health");
    });

var app = builder.Build();

// Minimal APIs with snake_case for a different API version
var apiV2 = app.UseAspNetConventions("/api/v2", options =>
{
    options.Route.CaseStyle = CasingStyle.SnakeCase;
});

app.MapControllers();

// MVC Controller endpoints (kebab-case, prefix removed)
// GET /api/user-profile/by-id/{user-id}  (was GetById)
// POST /api/user-profile/account         (was PostAccount)

// Minimal API endpoints (snake_case)
apiV2.MapGet("/WeatherForecast/{CityName}", handler);
// GET /api/v2/weather_forecast/{city_name}

apiV2.MapPost("/CreateOrder/{StoreId}", handler);
// POST /api/v2/create_order/{store_id}

app.Run();
```

### Result Comparison {#result-comparison}

| Endpoint Type | Original | Transformed |
|---------------|----------|-------------|
| MVC (kebab) | `GetUserById/{UserId}` | `user-by-id/{user-id}` |
| MVC (kebab) | `PostAccount` | `account` |
| Minimal (snake) | `WeatherForecast/{CityName}` | `weather_forecast/{city_name}` |
| Minimal (snake) | `CreateOrder/{StoreId}` | `create_order/{store_id}` |

---

## Complete Application Setup {#complete-application-setup}

A full `Program.cs` example with all features configured.

```csharp
using AspNetConventions;
using AspNetConventions.Core.Enums;

var builder = WebApplication.CreateBuilder(args);

// Configure MVC with AspNetConventions
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        // Global settings
        options.Route.IsEnabled = true;
        options.Route.CaseStyle = CasingStyle.KebabCase;

        // MVC Controllers
        options.Route.Controllers.TransformParameterNames = true;
        options.Route.Controllers.TransformRouteTokens = true;
        options.Route.Controllers.RemoveActionPrefixes.Add("Get");
        options.Route.Controllers.ExcludeControllers.Add("Health");

        // Hooks for debugging
        options.Route.Hooks.AfterRouteTransform = (newRoute, originalRoute, model) =>
        {
            if (builder.Environment.IsDevelopment())
            {
                Console.WriteLine($"[{model.Identity.Kind}] {originalRoute} → {newRoute}");
            }
        };
    });

// Configure Razor Pages
builder.Services.AddRazorPages()
    .AddAspNetConventions(options =>
    {
        options.Route.RazorPages.TransformParameterNames = true;
        options.Route.RazorPages.TransformPropertyNames = true;
        options.Route.RazorPages.ExcludeFolders.Add("Admin");
    });

var app = builder.Build();

// Configure Minimal APIs
var api = app.UseAspNetConventions("/api/v1", options =>
{
    options.Route.MinimalApi.ExcludeRoutePatterns.Add("/health");
    options.Route.MinimalApi.ExcludeTags.Add("internal");
});

// Health check (excluded)
api.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithTags("internal");

// API endpoints
api.MapGet("/Products/{ProductId}", (int ProductId) =>
    Results.Ok(new { ProductId }));

api.MapPost("/Orders/Create", (CreateOrderDto order) =>
    Results.Created($"/orders/{order.OrderId}", order));

app.MapControllers();
app.MapRazorPages();

app.Run();

public record CreateOrderDto(Guid OrderId, string ProductName, int Quantity);
```

### Output in Development {#output-in-development}

```
[MvcAction] api/UserProfile/GetById/{UserId} → api/user-profile/by-id/{user-id}
[MvcAction] api/UserProfile/CreateAccount → api/user-profile/create-account
[MinimalApi] /api/v1/Products/{ProductId} → /api/v1/products/{product-id}
[MinimalApi] /api/v1/Orders/Create → /api/v1/orders/create
[RazorPage] UserProfile/Edit/{UserId} → user-profile/edit/{user-id}
```

