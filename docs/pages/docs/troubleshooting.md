# Troubleshooting

Common issues and solutions across all **AspNetConventions** features.

---

## Route Standardization {#route-standardization}

### Routes Not Being Transformed {#routes-not-being-transformed}

**Problem:** Routes remain in PascalCase despite configuration.

**Possible causes:**

1. **Transformation disabled** — check that all relevant flags are on:
```csharp
options.Route.IsEnabled = true;
options.Route.Controllers.IsEnabled = true; // MVC
options.Route.RazorPages.IsEnabled = true;  // Razor Pages
options.Route.MinimalApi.IsEnabled = true;  // Minimal APIs
```

2. **Endpoint is excluded:**
```csharp
// Check these lists for your controller, area, tag, or route pattern
options.Route.Controllers.ExcludeControllers
options.Route.Controllers.ExcludeAreas
options.Route.MinimalApi.ExcludeTags
options.Route.MinimalApi.ExcludeRoutePatterns
```

3. **A hook is returning `false`** — add a debug log to confirm:
```csharp
options.Route.Hooks.ShouldTransformToken = token =>
{
    Console.WriteLine($"Checking token: {token}");
    return true;
};
```

---

### Parameter Binding Fails in Minimal APIs {#parameter-binding-fails-in-minimal-apis}

**Problem:** After enabling route transformation, Minimal API parameters return `null` or cause binding errors.

**Cause:** Minimal APIs bind parameters strictly by name. When `{userId}` is transformed to `{user-id}`, the binder can no longer match the value.

**Solution A — Explicit binding:**
```csharp
options.Route.MinimalApi.TransformRouteParameters = true;

api.MapGet("/UserAccount/{userId}",
    ([FromRoute(Name = "user-id")] int userId) => Results.Ok(userId));
```

**Solution B — Leave parameter transformation off (the default):**
```csharp
options.Route.MinimalApi.TransformRouteParameters = false; // default
```

---

### Query String Parameters Not Binding {#query-string-parameters-not-binding}

**Problem:** Complex type properties don't bind from query strings after transformation.

**Cause:** Query parameter names must match the transformed property names.

```csharp
public class SearchRequest
{
    public string CategoryName { get; set; }
    public int PageNumber { get; set; }
}
```

| | URL |
|---|---|
| Incorrect | `/search?CategoryName=Books&PageNumber=1` |
| Correct | `/search?category-name=Books&page-number=1` |

---

### Razor Pages Form Fields Not Binding {#razor-pages-form-fields-not-binding}

**Problem:** Form submissions fail to bind to `[BindProperty]` properties.

**Cause:** Form field names must use the transformed names.

```html
<!-- Incorrect -->
<input type="text" name="UserName" />

<!-- Correct -->
<input type="text" name="user-name" />
```

Using tag helpers handles this automatically:
```html
<input asp-for="UserName" />
<!-- Generates: name="user-name" -->
```

See [Enable Tag Helpers](../getting-started/quick-start.md#enable-tag-helpers) for setup instructions.

---

### Debugging Route Transformations {#debugging-route-transformations}

Use `AfterRouteTransform` to log all transformations at startup:

```csharp
options.Route.Hooks.AfterRouteTransform = (newRoute, originalRoute, model) =>
{
    Console.WriteLine($"[{model.Identity.Kind}] {originalRoute} → {newRoute}");
};
```

```
[MvcAction] api/UserProfile/GetById/{UserId} → api/user-profile/get-by-id/{user-id}
[MinimalApi] /WeatherForecast/{CityName} → /weather-forecast/{city-name}
[RazorPage] UserProfile/Edit/{UserId} → user-profile/edit/{user-id}
```

---

## Response Formatting {#response-formatting}

### Responses Not Being Wrapped {#responses-not-being-wrapped}

**Problem:** API responses are returned without the envelope wrapper.

**Possible causes:**

1. **Response formatting disabled:**
```csharp
options.Response.IsEnabled = true; // default
```

2. **Minimal API middleware not registered** — endpoints must be mapped on the group returned by `UseAspNetConventions()`:
```csharp
var api = app.UseAspNetConventions();
api.MapGet("/api/test", () => Results.Ok("test")); // ✓
app.MapGet("/api/test", () => Results.Ok("test")); // ✗ bypasses formatting
```

3. **A hook is returning `false`:**
```csharp
options.Response.Hooks.ShouldWrapResponseAsync = async (result, request) =>
{
    Console.WriteLine($"Checking: {request.Path}");
    return true;
};
```

4. **Non-JSON result** — only `ObjectResult` types are wrapped. File downloads, redirects, and similar results pass through unchanged.

---

### Double-Wrapped Responses {#double-wrapped-responses}

**Problem:** Responses appear wrapped twice with nested envelopes.

**Cause:** Your controller is returning a pre-wrapped object, and **AspNetConventions** wraps it again.

**Solution:** Let the library do the wrapping — return raw values:
```csharp
// Don't: returns a wrapper that gets wrapped again
return Ok(new { success = true, data = user });

// Do: return the value directly
return ApiResults.Ok(user);
```

If you use a custom `IResponseBuilder`, implement `IsWrappedResponse` to signal your own type:
```csharp
public bool IsWrappedResponse(object? value) => value is MyApiResponse;
```

---

### Validation Errors Not Formatted {#validation-errors-not-formatted}

**Problem:** Model validation errors return raw `ValidationProblemDetails` instead of the wrapped format.

**Solution:** Use `ApiResults.BadRequest(ModelState)` or `BadRequest(ModelState)`:
```csharp
if (!ModelState.IsValid)
    return ApiResults.BadRequest(ModelState);
```

---

### Pagination Links Incorrect {#pagination-links-incorrect}

**Problem:** Pagination links have wrong parameter names or base URL.

**Solution:** Verify your pagination parameter name configuration and pass the correct values to `Paginate()`:
```csharp
options.Response.Pagination.PageNumberParameterName = "page-number"; // default
options.Response.Pagination.PageSizeParameterName = "page-size";     // default

return ApiResults.Paginate(items, totalCount, pageNumber, pageSize);
```

---

### Metadata TraceId Always Null {#metadata-traceid-always-null}

**Problem:** The `metadata.traceId` field is always `null`.

**Cause:** Distributed tracing is not configured, or `Activity.Current` is `null`.

**Solution:** Configure tracing middleware or disable the metadata block:
```csharp
// Option 1: add tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddAspNetCoreInstrumentation());

// Option 2: disable metadata
options.Response.IncludeMetadata = false;
```

---

### Exception Details Showing in Production {#exception-details-in-production}

**Problem:** Stack traces and exception details are visible in production responses.

**Solution:** Use `null` (auto-detection, recommended) or explicitly set to `false` in production:
```csharp
options.Response.ErrorResponse.IncludeExceptionDetails = null;  // default — Development only

// Explicit
options.Response.ErrorResponse.IncludeExceptionDetails =
    builder.Environment.IsDevelopment();
```

---

### ApiResults Methods Not Found {#apiresults-methods-not-found}

**Problem:** Compiler error: `'ApiResults' does not contain a definition for 'Ok'`.

**Solution:** Add the correct using:
```csharp
using AspNetConventions.Http;
```

---

### Debugging Response Formatting {#debugging-response-formatting}

Use `AfterResponseWrapAsync` to inspect what the formatter sees:

```csharp
options.Response.Hooks.AfterResponseWrapAsync = async (wrapped, result, request) =>
{
    Console.WriteLine($"Path: {request.Path} | Status: {result.StatusCode} | ValueType: {result.GetValue()?.GetType().Name ?? "null"}");
};
```

---

## Exception Handling {#exception-handling}

### Custom Mapper Not Being Used {#custom-mapper-not-being-used}

**Problem:** You registered a custom mapper but still get the default 500 response.

**Possible causes:**

1. **Mapper not registered:**
```csharp
options.ExceptionHandling.Mappers.Add(new MyMapper());
```

2. **Exception type doesn't match** — your mapper handles `OrderNotFoundException` but a different type is thrown:
```csharp
throw new NotFoundException("Order not found"); // not the same type
```

3. **`CanMapException` returns `false`** — add a log to verify:
```csharp
public override bool CanMapException(Exception exception, RequestDescriptor request)
{
    Console.WriteLine($"CanMap: {exception.GetType().Name}");
    return exception is OrderNotFoundException;
}
```

---

### Multiple Mappers Matching {#multiple-mappers-matching}

**Problem:** The wrong mapper is handling your exception.

**Solution:** Mappers are evaluated in registration order. Register more specific mappers first:

```csharp
options.ExceptionHandling.Mappers.Add(new OrderNotFoundExceptionMapper());  // Specific
options.ExceptionHandling.Mappers.Add(new NotFoundExceptionMapper());       // General
options.ExceptionHandling.Mappers.Add(new DomainExceptionMapper());         // Base class
```

---

### Exceptions Not Being Caught {#exceptions-not-being-caught}

**Problem:** Exceptions propagate without being handled by **AspNetConventions**.

**Possible causes:**

1. **`ShouldHandleAsync` returning `false`:**
```csharp
options.ExceptionHandling.Hooks.ShouldHandleAsync = async (exception, request) =>
{
    Console.WriteLine($"Caught: {exception.GetType().Name}");
    return true;
};
```

2. **Exception thrown before middleware** — exceptions in `ConfigureServices` or early pipeline stages won't be caught.

3. **Middleware order** — `UseAspNetConventions()` must be called before the endpoints that throw:
```csharp
app.UseAuthentication();
app.UseAuthorization();
var api = app.UseAspNetConventions(); // ← here
api.MapGet("/api/orders", ...);
```

---

### HTTP Status Code Defaults to 500 {#http-status-code-defaults-to-500}

**Problem:** The response always returns 500 regardless of the exception.

**Cause:** `StatusCode` is not set in the `ExceptionDescriptor`.

**Solution:**
```csharp
return new ExceptionDescriptor
{
    StatusCode = HttpStatusCode.NotFound, // Must be set
    Type = "NOT_FOUND",
    Message = "Resource not found"
};
```

If `StatusCode` is `null`, the fallback behavior can be customized via [`ErrorResponseOptions`](../response-formatting/configuration.md#errorresponseoptions):

```csharp
options.Response.ErrorResponse.DefaultStatusCode = HttpStatusCode.InternalServerError; // default
options.Response.ErrorResponse.DefaultErrorType = "INTERNAL_ERROR";
options.Response.ErrorResponse.DefaultErrorMessage = "An error occurred.";
```

---

### Conflict with Other Exception Middleware {#conflict-with-other-exception-middleware}

**Problem:** **AspNetConventions** conflicts with another exception handling middleware (e.g. `UseExceptionHandler`).

**Solution:** Remove the conflicting middleware, or exclude specific exception types:

```csharp
// Remove this if AspNetConventions handles exceptions
// app.UseExceptionHandler("/error");

// Or exclude specific types from AspNetConventions
options.ExceptionHandling.ExcludeStatusCodes.Add(HttpStatusCode.Unauthorized);
options.ExceptionHandling.ExcludeException.Add(typeof(SecurityException));
```

---

### Debugging Exception Handling {#debugging-exception-handling}

Hook into all three stages to trace the full pipeline:

```csharp
options.ExceptionHandling.Hooks.ShouldHandleAsync = async (ex, req) =>
{
    Console.WriteLine($"[ExceptionHandling] Caught {ex.GetType().Name} on {req.Path}");
    return true;
};

options.ExceptionHandling.Hooks.BeforeMappingAsync = async (mapper, req) =>
{
    Console.WriteLine($"[ExceptionHandling] Mapper: {mapper.GetType().Name}");
    return mapper;
};

options.ExceptionHandling.Hooks.AfterMappingAsync = async (descriptor, mapper, req) =>
{
    Console.WriteLine($"[ExceptionHandling] Result: {descriptor.StatusCode} {descriptor.Type} | Log: {descriptor.ShouldLog}");
    return descriptor;
};
```

---

## JSON Serialization {#json-serialization}

### Property Order Not Working {#property-order-not-working}

**Problem:** Setting `.Order()` on one property doesn't produce the expected output order.

**Cause:** `System.Text.Json` only guarantees order for properties that have an explicit order value set. Properties without one are placed **after** all ordered properties, in their natural declaration order. If you set `.Order(0)` on a single property but leave the rest unordered, the result can be unexpected depending on how many unordered properties precede or follow it.

**Solution A — Set order on all properties** for full control:
```csharp
cfg.Type<UserResponse>(type =>
{
    type.Property(x => x.Id).Order(0);
    type.Property(x => x.Name).Order(1);
    type.Property(x => x.Email).Order(2);
    type.Property(x => x.CreatedAt).Order(3);
});
```

**Solution B — Use a negative value** to push a single property to the front without touching the rest:
```csharp
cfg.Type<UserResponse>(type =>
{
    // Id appears first; all other properties follow in their declaration order
    type.Property(x => x.Id).Order(-1);
});
```

---

### CaseStyle Not Applying {#casestyle-not-applying}

**Problem:** Properties are still camelCase even though a different `CaseStyle` is set.

**Cause:** Each `AddAspNetConventions()` / `UseAspNetConventions()` call has its own independent options scope. If you configure `CaseStyle` in one call but your endpoints are registered under a different call, the setting won't apply there.

**Solution:** Check that the `CaseStyle` is set in the correct scope. For example, these two calls are independent — a `CasingStyle` set in one does not affect the other:

```csharp
// MVC Controllers — snake_case
builder.Services.AddControllers()
    .AddAspNetConventions(o => o.Json.CaseStyle = CasingStyle.SnakeCase);

// Minimal APIs — camelCase (separate scope, separate options)
var api = app.UseAspNetConventions(o => o.Json.CaseStyle = CasingStyle.CamelCase);
```

Set `CaseStyle` in every scope where it needs to apply.

---

### `.Name()` Override Still Being Transformed {#name-override-still-transformed}

**Problem:** A property with `.Name("my_name")` is still being renamed by the global `CaseStyle`.

**Answer:** Explicit `.Name()` values are written as-is and are never further transformed. If the output still looks wrong, verify the rule is targeting the correct type and property:

```csharp
cfg.Type<Product>(type =>
{
    // Confirm: is this the right type? Is "InternalSku" the exact CLR property name?
    type.Property(x => x.InternalSku).Name("sku");
});
```

---

### `IgnoreType<T>()` Not Working {#ignoretype-not-working}

**Problem:** Properties of type `T` are still appearing in the response after calling `cfg.IgnoreType<T>()`.

**Common causes:**

1. **`IgnoreType<T>()` is registered inside the `ConfigureTypes` delegate** — confirm the delegate is actually assigned:
```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.IgnoreType<Metadata>(); // Must be inside the delegate
};
```

2. **Type mismatch** — `IgnoreType<T>()` walks the property's **value type** chain. If the property is declared as `object` or an interface, the runtime type won't match `T`. Use `IgnorePropertyName` for duck-typed properties instead.

3. **Framework default overriding** — `IgnoreType<T>()` has the **highest** priority and will override any per-type rule. If you see this failing, confirm you're not mixing it with an `IgnoreType` call on a base type that shouldn't apply here.

---

### `IgnorePropertyName()` Not Matching {#ignorepropertyname-not-matching}

**Problem:** `cfg.IgnorePropertyName("StatusCode")` has no effect.

**Cause:** The lookup tries the CLR name first, then falls back to the JSON-transformed name (e.g. `status_code` or `status-code`). If neither matches, the rule is not applied.

**Solution:** Pass either the exact CLR name or the exact serialized name. Both forms are accepted:
```csharp
cfg.IgnorePropertyName("StatusCode");   // matches CLR name
cfg.IgnorePropertyName("status_code");  // matches serialized name (snake_case)
cfg.IgnorePropertyName("statusCode");   // matches serialized name (camelCase)
```

---

### Assembly Scanning Not Picking Up Configurations {#assembly-scanning-not-picking-up}

**Problem:** A `JsonTypeConfiguration<T>` class exists but its rules are never applied.

**Check these:**

1. The class is `public` and non-abstract.
2. The class has a public parameterless constructor.
3. The correct assembly is passed to `ScanAssemblies`:
```csharp
// Use any type from the target assembly as the anchor
options.Json.ScanAssemblies(typeof(UserConfiguration).Assembly);
```

4. The class actually inherits from `JsonTypeConfiguration<T>` or `JsonOpenGenericTypeConfiguration<T>` (not a custom base in between that does not extend `JsonTypeConfigurationBase`).
