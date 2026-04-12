# Parameter Binding

How AspNetConventions handles parameter transformation and model binding across different binding sources.

---

## Basic Parameter Binding {#basic-parameter-binding}

Parameter names are transformed in the URL, but **model binding always uses the original name**. AspNetConventions registers internal binding aliases, making the resolution transparent.

```csharp
// Route: GET /api/orders/get-by-user/{user-id}
[HttpGet("GetByUser/{userId}")]
public ActionResult GetByUser(int userId)   // binds from {user-id}
{
    return Ok(userId);
}
```

### Supported Binding Sources {#supported-binding-sources}

| Attribute | Binding Source | Example |
|-----------|---------------|---------|
| `[FromRoute]` | Route values | `/orders/{order-id}` |
| `[FromQuery]` | Query string | `/orders?order-id=123` |
| `[FromHeader]` | HTTP headers | `X-Order-Id: 123` |
| `[FromForm]` | Form values | `form-data; name="order-id"` |
| `[FromBody]` | Request body | JSON property names |

**Example with multiple binding sources:**

```csharp
// Route: POST /api/account/create/{tenant-id}?referral-code=
[HttpPost("{tenantId}")]
public ActionResult Create(
    [FromRoute] Guid tenantId,      // binds from {tenant-id}
    [FromQuery] string? referralCode, // binds from ?referral-code=
    [FromBody] UserDto request)      // binds from JSON body
{
    return Ok(request);
}
```

### Route Constraints {#route-constraints}

Constraints survive transformation — only the parameter name is rewritten:

```
{UserId:int}       →  {user-id:int}
{Slug:regex(...)}  →  {slug:regex(...)}
{Id:guid}          →  {id:guid}
```

---

## Complex Types {#complex-types}

When you bind a complex type (a class with multiple properties), AspNetConventions transforms **all property names recursively** to match your configured case style.

### How It Works {#how-it-works}

1. **Property Discovery** — Scans all public properties of the complex type
2. **Recursive Transformation** — Nested objects have their properties transformed as well
3. **Binding Alias Registration** — Internal aliases map transformed names back to original property names
4. **Transparent Resolution** — Your C# code continues using original property names

### Example Model {#example-model}

```csharp
public class ProductSearchRequest
{
    public string CategoryName { get; set; }      // → category-name
    public string ProductCode { get; set; }       // → product-code
    public PriceRange PriceFilter { get; set; }   // → price-filter (nested)
}

public class PriceRange
{
    public decimal MinPrice { get; set; }         // → min-price
    public decimal MaxPrice { get; set; }         // → max-price
}
```

### Binding Source Examples {#binding-source-examples}

::: tabs

== tab "[FromQuery]"

Query string parameters use dot notation for nested properties:

```csharp
[HttpGet("[action]")]
public ActionResult Search([FromQuery] ProductSearchRequest request)
{
    return Ok(request);
}
```

**Request URL:**
```
GET /search?category-name=Electronics&product-code=SKU123&price-filter.min-price=10&price-filter.max-price=100
```

**Parsed Object:**
```csharp
// request.CategoryName = "Electronics"
// request.ProductCode = "SKU123"
// request.PriceFilter.MinPrice = 10
// request.PriceFilter.MaxPrice = 100
```

== tab "[FromBody]"

JSON body properties are serialized using the configured case style:

```csharp
[HttpPost("[action]")]
public ActionResult Create([FromBody] ProductSearchRequest request)
{
    return Ok(request);
}
```

**Request Body:**
```json
{
  "category-name": "Electronics",
  "product-code": "SKU123",
  "price-filter": {
    "min-price": 10,
    "max-price": 100
  }
}
```

== tab "[FromForm]"

Form data uses dot notation, same as query strings:

```csharp
[HttpPost("[action]")]
public ActionResult Submit([FromForm] ProductSearchRequest request)
{
    return Ok(request);
}
```

**Request (multipart/form-data):**
```http
POST /submit HTTP/1.1
Content-Type: multipart/form-data; boundary=----FormBoundary

------FormBoundary
Content-Disposition: form-data; name="category-name"

Electronics
------FormBoundary
Content-Disposition: form-data; name="price-filter.min-price"

10
------FormBoundary--
```

== tab "[FromRoute]"

Complex types can bind from route values when the template includes matching placeholders:

```csharp
[HttpGet("[action]/{CategoryName}/{ProductCode}")]
public ActionResult GetProduct([FromRoute] ProductIdentifier product)
{
    return Ok(product);
}

public class ProductIdentifier
{
    public string CategoryName { get; set; }
    public string ProductCode { get; set; }
}
```

**Route template becomes:**
```
/get-product/{category-name}/{product-code}
```

== tab "[FromHeader]"

::: callout warning ASP.NET Core Limitation
ASP.NET Core does **not** support complex type binding with `[FromHeader]` by default. When you use `[FromHeader]` with a complex type, ASP.NET expects a single header value in a comma-separated format, not individual headers per property.
:::

**For individual header binding**, use separate parameters instead:

```csharp
[HttpGet("[action]")]
public ActionResult GetWithHeaders(
    [FromHeader(Name = "X-Client-Version")] string clientVersion,
    [FromHeader(Name = "X-Request-Id")] string requestId)
{
    return Ok(new { clientVersion, requestId });
}
```

**Request Headers:**
```http
GET /get-with-headers HTTP/1.1
x-client-version: 1.0.0
x-request-id: abc-123
```

:::

---

## Custom Binding Names {#custom-binding-names}

Some binding attributes allow you to explicitly define the parameter name via `IModelNameProvider`. When an explicit name is provided, AspNetConventions transforms it according to your configured casing style.

```csharp
// Route: /api/profile/theme-generator/{accent-color}
[HttpGet("[action]/{AccentColor}")]
public ActionResult ThemeGenerator([FromRoute(Name = "AccentColor")] string color)
{
    return Ok(color);
}
```

### Preserving Explicit Names {#preserving-explicit-names}

To skip transformation for explicitly named parameters, enable [`PreserveExplicitBindingNames`](./configuration.md#routeconventionoptions):

```csharp
// MVC Controllers
options.Route.Controllers.PreserveExplicitBindingNames = true;

// Razor Pages
options.Route.RazorPages.PreserveExplicitBindingNames = true;
```

```csharp
// Route: /get-user/{user_id}  (parameter not transformed)
[HttpGet("GetUser/{user_id}")]
public ActionResult GetUser([FromRoute(Name = "user_id")] int userId)
{
    return Ok(userId);
}
```

### Complex Objects with Custom Names {#complex-objects-with-custom-names}

There are two levels of customization:

1. **Parameter-level prefix** — Using `[FromQuery(Name = "...")]` on the action parameter
2. **Property-level names** — Using `[ModelBinder(Name = "...")]` on class properties

**Both levels are transformed:**

```csharp
public class ProductFilter
{
    public string CategoryName { get; set; }
    public decimal MinPrice { get; set; }
}

[HttpGet("[action]")]
public ActionResult Search([FromQuery(Name = "FilterBy")] ProductFilter filter)
{
    return Ok(filter);
}
```

**Before:**
```
/search?FilterBy.CategoryName=Electronics&FilterBy.MinPrice=10
```

**After:**
```
/search?filter-by.category-name=Electronics&filter-by.min-price=10
```

### Property-Level Custom Names {#property-level-custom-names}

Override individual property names using `[ModelBinder(Name = "...")]`:

```csharp
public class SearchFilters
{
    [ModelBinder(Name = "Category")]
    public string CategoryName { get; set; }    // "Category" → "category"

    [ModelBinder(Name = "Q")]
    public string SearchQuery { get; set; }     // "Q" → "q"

    public PriceRange Prices { get; set; }      // "Prices" → "prices"
}

public class PriceRange
{
    [ModelBinder(Name = "From")]
    public decimal Min { get; set; }            // "From" → "from"

    [ModelBinder(Name = "To")]
    public decimal Max { get; set; }            // "To" → "to"
}

[HttpGet("[action]")]
public ActionResult Search([FromQuery(Name = "F")] SearchFilters filters)
{
    return Ok(filters);
}
```

**Result (kebab-case):**
```
/search?f.category=Electronics&f.q=laptop&f.prices.from=100&f.prices.to=500
```

When [`PreserveExplicitBindingNames`](./configuration.md#routeconventionoptions) is enabled, only properties **without** explicit `[ModelBinder(Name = "...")]` are transformed.

---

## Razor Pages Property Binding {#razor-pages-property-binding}

In Razor Pages, [`TransformPropertyNames`](./configuration.md#razorpagesrouteoptions) (enabled by default) transforms `[BindProperty]` names automatically, creating a seamless experience between standardized routes and your page models.

### Basic Property Binding {#basic-property-binding}
```csharp
// /Pages/Edit.cshtml.cs
public class EditModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int UserId { get; set; }   // bound from {user-id}

    [BindProperty(SupportsGet = true, Name = "SourceDevice")]
    public string? Source { get; set; } // bound from {source-device}

    [BindProperty]
    public string UserName { get; set; }   // bound from form field (POST) "user-name"
}
```

```html
<!-- /Pages/Edit.cshtml -->
@page "{UserId}"
<!-- Route becomes: /edit/{user-id}?source-device= -->
@model EditModel

<h1>Edit User</h1>

<form method="post">
    <input asp-for="UserName" />  <!-- Generates name="user-name" -->
    <button type="submit">Save</button>
</form>
...
```

### How Property Binding Works {#how-property-binding-works}

The transformation applies to three binding scenarios:

| Binding Source | Original Name | Transformed Name | Example |
|----------------|---------------|------------------|---------|
| **Route Values** | `UserId` | `user-id` | `/edit/123` → `UserId = 123` |
| **Query Strings** | `SourceDevice` | `source-device` | `?source-device=mobile` → `Source = "mobile"` |
| **Form Fields** | `UserName` | `user-name` | `<input name="user-name">` → `UserName = "value"` |


### Property Binding With Razor View {#property-binding-with-razor-view}

**AspNetConventions** extends ASP.NET Core's built-in **Tag Helpers** to automatically transform `asp-for` attribute outputs into your configured casing style. The form fields always match your standardized route conventions without any extra code in your views.

**Example:**

```csharp
// Your page model
[BindProperty]
public string UserName { get; set; }
```

```html
<!-- Your Razor view (no changes needed) -->
<input asp-for="UserName" />
```
Generated HTML with **AspNetConventions**:
```html
<input name="user-name" id="UserName" />
```
This seamless integration means you never have to manually maintain HTML attribute names, see how to [Enable Tag Helpers](../getting-started/quick-start.md#enable-tag-helpers) in your setup.