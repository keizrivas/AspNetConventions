# Troubleshooting

Common issues and solutions when using Route Standardization.

---

## Parameter Binding Fails in Minimal APIs {#parameter-binding-fails-in-minimal-apis}

**Problem:** After enabling route transformation, Minimal API parameters return null or cause binding errors.

**Cause:** Minimal APIs bind parameters strictly by name. When `{userId}` is transformed to `{user-id}`, the binder can't find the value.

**Solution:** Use explicit binding with `[FromRoute(Name = "...")]`:

```csharp
// Enable transformation
options.Route.MinimalApi.TransformRouteParameters = true;

// Use explicit binding
api.MapGet("/UserAccount/{userId}",
    ([FromRoute(Name = "user-id")] int userId) => Results.Ok(userId));
```

Or keep parameter transformation disabled (default behavior):

```csharp
options.Route.MinimalApi.TransformRouteParameters = false; // default
```

---

## Routes Not Being Transformed {#routes-not-being-transformed}

**Problem:** Routes remain in PascalCase despite configuration.

**Possible causes:**

1. **Transformation disabled:**
```csharp
// Check these are enabled
options.Route.IsEnabled = true;
options.Route.Controllers.IsEnabled = true; // for MVC
options.Route.RazorPages.IsEnabled = true;  // for Razor Pages
options.Route.MinimalApi.IsEnabled = true;  // for Minimal APIs
```

2. **Endpoint is excluded:**
```csharp
// Check exclusion lists
options.Route.Controllers.ExcludeControllers // contains your controller?
options.Route.Controllers.ExcludeAreas       // contains your area?
options.Route.MinimalApi.ExcludeTags         // contains your endpoint's tag?
options.Route.MinimalApi.ExcludeRoutePatterns // matches your route?
```

3. **Hook returning false:**
```csharp
// Check if a hook is skipping your route
options.Route.Hooks.ShouldTransformRoute = (template, model) => {
    Console.WriteLine($"Checking: {template}"); // Debug output
    return true;
};
```

---

## Query String Parameters Not Binding {#query-string-parameters-not-binding}

**Problem:** Complex type properties don't bind from query strings.

**Cause:** Query parameter names must match the transformed property names.

**Solution:** Use transformed names in your requests:

```csharp
public class SearchRequest
{
    public string CategoryName { get; set; }
    public int PageNumber { get; set; }
}
```

**Incorrect request:**
```
/search?CategoryName=Electronics&PageNumber=1
```

**Correct request:**
```
/search?category-name=Electronics&page-number=1
```

---

## Route Constraints Not Working {#route-constraints-not-working}

**Problem:** Route constraints like `{id:int}` cause 404 errors after transformation.

**Cause:** This typically happens when the constraint syntax is malformed.

**Verification:** Constraints should survive transformation intact:

```
{UserId:int}       →  {user-id:int}      ✓
{Slug:regex(...)}  →  {slug:regex(...)}  ✓
```

Check that your constraint is properly formatted:
```csharp
// Correct
[HttpGet("{userId:int}")]

// Incorrect (missing colon)
[HttpGet("{userId int}")]
```

---

## Razor Pages Form Fields Not Binding {#razor-pages-form-fields-not-binding}

**Problem:** Form submissions fail to bind to `[BindProperty]` properties.

**Cause:** Form field names must use transformed names.

**Solution:** Update your form field names:

```html
<!-- Incorrect -->
<input type="text" name="UserName" />

<!-- Correct -->
<input type="text" name="user-name" />
```

Or use tag helpers which handle this automatically:

```html
<input asp-for="UserName" />
<!-- Generates: name="user-name" -->
```

---

## URL Generation Returns Original Names {#url-generation-returns-original-names}

**Problem:** `Url.Action()` or tag helpers generate URLs with PascalCase names.

**Cause:** **AspNetConventions** may not be properly registered.

**Solution:** Ensure proper registration order:

```csharp
// MVC Controllers
builder.Services.AddControllers()
    .AddAspNetConventions(); // Must be called

// Razor Pages
builder.Services.AddRazorPages()
    .AddAspNetConventions(); // Must be called

var app = builder.Build();

// Minimal APIs
var api = app.UseAspNetConventions(); // Returns RouteGroupBuilder — map endpoints on it
```

---

## Debugging Transformations {#debugging-transformations}

Use the `AfterRouteTransform` hook to log all transformations:

```csharp
options.Route.Hooks.AfterRouteTransform = (newRoute, originalRoute, model) =>
{
    Console.WriteLine($"[{model.Identity.Kind}] {originalRoute} → {newRoute}");
};
```

**Output:**
```
[MvcAction] api/UserProfile/GetById/{UserId} → api/user-profile/get-by-id/{user-id}
[MinimalApi] /WeatherForecast/{CityName} → /weather-forecast/{city-name}
[RazorPage] UserProfile/Edit/{UserId} → user-profile/edit/{user-id}
```

This helps identify which routes are being transformed and which are being skipped.

