# Troubleshooting

Common issues and solutions when using Response Formatting.

---

## Responses Not Being Wrapped

**Problem:** API responses are returned without the envelope wrapper.

**Possible causes:**

1. **Response formatting disabled:**
```csharp
// Check this is enabled
options.Response.IsEnabled = true;  // default
```

2. **Minimal API middleware not registered:**
```csharp
var app = builder.Build();

// Make sure this is called
app.UseAspNetConventions();

app.MapGet("/api/test", () => Results.Ok("test"));
app.Run();
```

3. **Hook returning false:**
```csharp
// Check if a hook is skipping your responses
options.Response.Hooks.ShouldWrapResponseAsync = async (result, request) =>
{
    Console.WriteLine($"Checking: {request.Path}");  // Debug
    return true;
};
```

4. **Non-JSON result types:**
Response formatting only wraps `ObjectResult` types. File downloads, redirects, and other non-JSON results pass through unchanged.

---

## Double-Wrapped Responses

**Problem:** Responses appear wrapped twice with nested envelopes.

**Cause:** Your code is returning a pre-wrapped response, and AspNetConventions wraps it again.

**Solution:** Implement `IsWrappedResponse` in your custom builder:

```csharp
public class MyResponseBuilder : IResponseBuilder
{
    public bool IsWrappedResponse(object? value)
    {
        // Return true for your wrapper type to prevent double-wrapping
        return value is MyApiResponse;
    }

    public object BuildResponse(ApiResult apiResult, RequestDescriptor request)
    {
        // ...
    }
}
```

Or avoid returning pre-wrapped responses from your controllers:

```csharp
// Don't do this - will be double-wrapped
return Ok(new { success = true, data = user });

// Do this instead - let AspNetConventions wrap it
return ApiResults.Ok(user);
```

---

## Minimal API Endpoints Not Wrapped

**Problem:** MVC Controller responses are wrapped, but Minimal API responses are not.

**Cause:** The `UseAspNetConventions()` middleware is not in the pipeline.

**Solution:** Call `UseAspNetConventions()` after `Build()` and before mapping endpoints:

```csharp
var app = builder.Build();

// Must be called before MapGet, MapPost, etc.
app.UseAspNetConventions();

app.MapGet("/api/test", () => Results.Ok("test"));
app.Run();
```

---

## Metadata TraceId Always Null

**Problem:** The `metadata.traceId` field is always `null` in responses.

**Cause:** Distributed tracing is not configured, or `Activity.Current` is null.

**Solution:** Ensure tracing middleware is configured:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry or similar tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation());

var app = builder.Build();

// Or ensure routing is added
app.UseRouting();
app.UseAspNetConventions();
```

If you don't need traceId, disable metadata:
```csharp
options.Response.IncludeMetadata = false;
```

---

## Pagination Links Incorrect

**Problem:** Pagination links have wrong parameter names or base URL.

**Solution:** Configure pagination options:

```csharp
options.Response.Pagination.PageNumberParameterName = "page";  // default
options.Response.Pagination.PageSizeParameterName = "pageSize";  // default
```

Make sure you're passing the correct parameters to `Paginate()`:

```csharp
// Correct - pass actual page and pageSize
return ApiResults.Paginate(items, totalCount, page, pageSize);

// Wrong - missing page parameters
return ApiResults.Paginate(items, totalCount);  // Won't generate links correctly
```

---

## Validation Errors Not Formatted

**Problem:** Model validation errors return raw `ValidationProblemDetails` instead of the wrapped format.

**Cause:** The default ASP.NET Core validation filter runs before AspNetConventions.

**Solution:** Use `ApiResults.BadRequest(ModelState)` for validation errors:

```csharp
[HttpPost]
public ActionResult Create([FromBody] CreateRequest request)
{
    if (!ModelState.IsValid)
        return ApiResults.BadRequest(ModelState);  // Use this

    // Don't use: return BadRequest(ModelState);

    return ApiResults.Created(result);
}
```

Or disable the automatic validation filter:
```csharp
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
```

---

## File/Redirect Results Being Wrapped

**Problem:** File downloads or redirects are being processed by response formatting.

**Answer:** This shouldn't happen. AspNetConventions only wraps `ObjectResult` types. If you're seeing issues:

1. **Verify the result type:**
```csharp
// These should NOT be wrapped:
return File(bytes, "application/pdf");
return Redirect("/other-page");
return PhysicalFile("/path/to/file", "image/png");

// These WILL be wrapped:
return Ok(new { data = "test" });
return new ObjectResult(data);
```

2. **Check for custom middleware** that might be converting results.

---

## Different Envelope Per Controller

**Problem:** You want different response formats for different controllers.

**Solution:** Response builders are global, but you can implement conditional logic:

```csharp
public class ConditionalResponseBuilder : IResponseBuilder
{
    public bool IsWrappedResponse(object? value) => false;

    public object BuildResponse(ApiResult apiResult, RequestDescriptor request)
    {
        // Different format for v2 API
        if (request.Path.StartsWith("/api/v2"))
        {
            return new
            {
                result = apiResult.GetValue(),
                error = apiResult.IsSuccess ? null : apiResult.Message
            };
        }

        // Default format
        return new
        {
            status = apiResult.IsSuccess ? "success" : "error",
            data = apiResult.GetValue(),
            message = apiResult.Message
        };
    }
}
```

---

## Exception Details Showing in Production

**Problem:** Stack traces and exception details are visible in production responses.

**Solution:** Check the `IncludeExceptionDetails` setting:

```csharp
options.Response.ErrorResponse.IncludeExceptionDetails = false;  // Explicit

// Or use auto-detection (recommended)
options.Response.ErrorResponse.IncludeExceptionDetails = null;  // default
// This includes details only in Development environment
```

Verify your environment:
```csharp
if (builder.Environment.IsDevelopment())
{
    options.Response.ErrorResponse.IncludeExceptionDetails = true;
}
else
{
    options.Response.ErrorResponse.IncludeExceptionDetails = false;
}
```

---

## ApiResults Methods Not Found

**Problem:** Compiler error: `'ApiResults' does not contain a definition for 'Ok'`.

**Solution:** Add the correct using statement:

```csharp
using AspNetConventions.Http;  // Add this

public class MyController : ControllerBase
{
    public ActionResult Get()
    {
        return ApiResults.Ok("test");  // Now works
    }
}
```

---

## Custom Builder Not Being Used

**Problem:** You registered a custom builder but responses still use the default format.

**Solution:** Verify registration:

```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        // Make sure this is set
        options.Response.ResponseBuilder = new MyResponseBuilder();
        options.Response.ErrorResponseBuilder = new MyErrorResponseBuilder();
    });
```

If using DI:
```csharp
// Register the builder
builder.Services.AddSingleton<IResponseBuilder, MyResponseBuilder>();

// Then resolve and assign
var serviceProvider = builder.Services.BuildServiceProvider();
options.Response.ResponseBuilder = serviceProvider.GetRequiredService<IResponseBuilder>();
```

---

## Debugging Response Formatting

Use the `AfterResponseWrapAsync` hook to inspect responses:

```csharp
options.Response.Hooks.AfterResponseWrapAsync = async (wrapped, result, request) =>
{
    Console.WriteLine($"=== Response Debug ===");
    Console.WriteLine($"Path: {request.Path}");
    Console.WriteLine($"Status: {result.StatusCode}");
    Console.WriteLine($"Type: {result.Type}");
    Console.WriteLine($"IsSuccess: {result.IsSuccess}");
    Console.WriteLine($"Value Type: {result.GetValue()?.GetType().Name ?? "null"}");
    Console.WriteLine($"Wrapped Type: {wrapped?.GetType().Name ?? "null"}");
};
```

This helps identify:
- Which responses are being processed
- What data is available in the `ApiResult`
- Whether wrapping is occurring correctly
