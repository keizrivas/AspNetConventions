# Configuration Reference

Configuration for **AspNetConventions** is centralized through the options callback. Depending on your project type, you can access these settings via the [`.AddAspNetConventions()`](/docs/getting-started/#addaspnetconventions) method on `IMvcBuilder` or the [`.UseAspNetConventions()`](/docs/getting-started/#useaspnetconventions) method for `WebApplication` setups.

## Entry Points

For **MVC Controllers** and **Razor Pages**:
```csharp
builder.Services
    .AddControllersWithViews()
    .AddAspNetConventions(options =>
    {
        // Custom options for MVC and Razor Pages here...
    });
```
For **Minimal APIs**:
```csharp
app.UseAspNetConventions(options =>
    {
        // Convention options for Minimal API here...
    });
```
## MVC vs Minimal APIs

AspNetConventions uses **different configuration contexts** depending on the type of endpoint you are working with. This separation exists because each endpoint type is integrated at a different stage of the ASP.NET pipeline.

**Key Difference:**
Each method receives its **own instance of [`AspNetConventionOptions`](#aspnetconventionoptions)**, meaning configurations are **not shared automatically** between MVC and Minimal APIs.

This allows you to configure each endpoint type independently.

---
**Example:**
```csharp
builder.Services
    .AddControllersWithViews()
    .AddAspNetConventions(options =>
    {
        // Applies only to MVC Controllers / Razor Pages
        options.Json.CaseStyle = CasingStyle.SnakeCase;
    });

var app = builder.Build();

app.UseAspNetConventions(options =>
{
    // Applies only to Minimal APIs
    options.Json.CaseStyle = CasingStyle.CamelCase;
});
```
---

## AspNetConventionOptions

**Namespace:** `AspNetConventions.Configuration.Options`

The `AspNetConventionOptions` object is the root configuration container. Every feature within the library is managed through the properties of this class.

| Property | Type | Default | Description |
|---|---|---|---|
| `Route` | [`RouteConventionOptions`](/docs/route-standardization/route-convention-options) | `new()` | Route naming convention settings |
| `Response` | [`ResponseFormattingOptions`](/) | `new()` | Response formatting settings |
| `Json` | [`JsonSerializationOptions`](/) | `new()` | JSON serialization settings |
| `ExceptionHandling` | [`ExceptionHandlingOptions`](/) | `new()` | Exception handling settings |

**Configuration Example:**
You can configure multiple features simultaneously to ensure your API remains consistent across all layers.

```csharp
builder.Services
    .AddControllers()
    .AddAspNetConventions(options =>
    {
        // Disable response metadata
        options.Response.IncludeMetadata = false;

        // Enforce "snake_case" for JSON properties and Route segments
        options.Json.CaseStyle = CasingStyle.SnakeCase;
        options.Route.CaseStyle = CasingStyle.SnakeCase;

        // Register a custom exception mapper
        options.ExceptionHandling.Mappers.Add(new BusinessExceptionMapper());
    });
```

---

## Related Documentation

To see the specific properties available for each feature, refer to their individual reference pages:

*   [Route Convention Options](/docs/route-standardization/route-convention-options)
    
*   [Response Formatting Options](/docs/response-formatting/options)
    
*   [JSON Serialization Options](/docs/json-serialization/options)
    
*   [Exception Handling Options](/docs/exception-handling/options)