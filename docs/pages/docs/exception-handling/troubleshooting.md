# Troubleshooting

Common issues and solutions when using Exception Handling.

---

## Custom Mapper Not Being Used {#custom-mapper-not-being-used}

**Problem:** You registered a custom mapper but the fallback 500 response is still returned.

**Possible causes:**

1. **Mapper not registered:**
```csharp

// Registered in AddAspNetConventions
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        options.ExceptionHandling.Mappers.Add(new MyMapper());  // Correct
    });

// Registered in UseAspNetConventions
var api = app.UseAspNetConventions(options =>
{
    options.ExceptionHandling.Mappers.Add(new MyMapper());  // Correct
});

```

2. **Exception type doesn't match:**
```csharp
// Your mapper handles OrderNotFoundException
public class OrderNotFoundMapper : ExceptionMapper<OrderNotFoundException>

// But you're throwing a different type
throw new NotFoundException("Order not found");  // Different type!
```

3. **CanMapException returns false:**
```csharp
// Override CanMapException if using custom logic
public override bool CanMapException(Exception exception, RequestDescriptor request)
{
    // Make sure this returns true for your exception
    return exception is OrderNotFoundException;
}
```

---

## Exception Details Showing in Production {#exception-details-showing-in-production}

**Problem:** Stack traces and exception details are visible in production responses.

**Solution:** Check the error response configuration:

```csharp
options.Response.ErrorResponse.IncludeExceptionDetails = false;

// Or use auto-detection
options.Response.ErrorResponse.IncludeExceptionDetails = null;  // default
```

Verify your environment:
```csharp
// Explicit environment-based configuration
if (builder.Environment.IsProduction())
{
    options.Response.ErrorResponse.IncludeExceptionDetails = false;
}
```

---

## Exceptions Not Being Caught {#exceptions-not-being-caught}

**Problem:** Exceptions propagate without being handled by **AspNetConventions**.

**Possible causes:**

1. **ShouldHandleAsync returning false:**
```csharp
options.ExceptionHandling.Hooks.ShouldHandleAsync = async (exception, request) =>
{
    Console.WriteLine($"Checking: {exception.GetType().Name}");  // Debug
    return true;
};
```

2. **Exception thrown before middleware:**
Exceptions in `ConfigureServices` or early middleware won't be caught.

---

## Logging Not Working {#logging-not-working}

**Problem:** Exceptions aren't being logged even though `ShouldLog = true`.

**Check these:**

1. **Verify ShouldLog is set:**
```csharp
return new ExceptionDescriptor
{
    ShouldLog = true,  // Must be true
    LogLevel = LogLevel.Error,
    // ...
};
```

2. **Check logging configuration:**
```csharp
// Ensure logging is configured in Program.cs
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

3. **Verify log level threshold:**
```csharp
// If minimum level is Error, Warning logs won't appear
builder.Logging.SetMinimumLevel(LogLevel.Warning);
```

---

## Multiple Mappers Matching {#multiple-mappers-matching}

**Problem:** The wrong mapper is handling your exception.

**Solution:** Mappers are evaluated in registration order. Register more specific mappers first:

```csharp
// More specific mappers first
options.ExceptionHandling.Mappers.Add(new OrderNotFoundExceptionMapper());   // Specific
options.ExceptionHandling.Mappers.Add(new ProductNotFoundExceptionMapper()); // Specific
options.ExceptionHandling.Mappers.Add(new NotFoundExceptionMapper());        // General
options.ExceptionHandling.Mappers.Add(new DomainExceptionMapper());          // Base class
```

---

## Value Not Appearing in Response {#value-not-appearing-in-response}

**Problem:** The `Value` property from your mapper isn't showing up in the error response.

**Causes:**

1. **Value is in the `errors` field:**
```json
{
  "status": "failure",
  "statusCode": 404,
  "type": "NOT_FOUND",
  "message": "...",
  "errors": { "orderId": 123 }  // Value appears here
}
```

2. **Custom IErrorResponseBuilder doesn't read Value:**
```csharp
public object BuildResponse(ApiResult apiResult, Exception? exception, RequestDescriptor request)
{
    return new
    {
        // Make sure to include apiResult.GetValue()
        errors = apiResult.GetValue()
    };
}
```

3. **Value is null:**
```csharp
// Ensure you're setting Value
return new ExceptionDescriptor
{
    Value = new { OrderId = exception.OrderId }  // Set this
};
```

---

## Minimal API Exceptions Not Handled {#minimal-api-exceptions-not-handled}

**Problem:** MVC Controller exceptions are handled, but Minimal API exceptions aren't.

**Solution:** Ensure `UseAspNetConventions()` is called before mapping endpoints:

```csharp
var app = builder.Build();

// Must be called for Minimal APIs — map endpoints on the returned group
var api = app.UseAspNetConventions();

api.MapGet("/api/test", () => throw new Exception("test"));

app.Run();
```

---

## Hooks Not Being Called {#hooks-not-being-called}

**Problem:** Your exception hooks aren't executing.

**Check these:**

1. **Hooks are assigned correctly:**
```csharp
options.ExceptionHandling.Hooks.AfterMappingAsync = async (descriptor, mapper, request) =>
{
    Console.WriteLine("Hook called!");  // Debug
    return descriptor;
};
```

2. **TryHandleAsync isn't set:**
```csharp
// If TryHandleAsync is set, it bypasses other hooks
options.ExceptionHandling.Hooks.TryHandleAsync = null;  // Ensure this is not set
```

3. **ShouldHandleAsync returned false:**
```csharp
// If this returns false, no further processing occurs
options.ExceptionHandling.Hooks.ShouldHandleAsync = async (ex, req) => true;
```

---

## HTTP Status Code Not Set Correctly {#http-status-code-not-set-correctly}

**Problem:** The response has the wrong HTTP status code.

**Solution:** Ensure you're setting `StatusCode` in the descriptor:

```csharp
return new ExceptionDescriptor
{
    StatusCode = HttpStatusCode.NotFound,  // Must be set
    Type = "NOT_FOUND",
    Message = "Resource not found"
};
```

If `StatusCode` is null, it defaults to 500 Internal Server Error.

---

## Mapper Resolution Issues {#mapper-resolution-issues}

**Problem:** You're not sure which mapper is being selected.

**Solution:** Use the `BeforeMappingAsync` hook to debug:

```csharp
options.ExceptionHandling.Hooks.BeforeMappingAsync = async (mapper, request) =>
{
    Console.WriteLine($"Selected mapper: {mapper.GetType().Name}");
    return mapper;
};
```

Or check the resolution order:
```csharp
// Resolution order:
// 1. Custom mappers (in registration order)
// 2. Built-in mappers (ArgumentNullException, etc.)
// 3. DefaultExceptionMapper (fallback)
```

---

## Debugging Exception Handling {#debugging-exception-handling}

Enable detailed logging to troubleshoot issues:

```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        options.ExceptionHandling.Hooks.ShouldHandleAsync = async (exception, request) =>
        {
            Console.WriteLine($"=== Exception Caught ===");
            Console.WriteLine($"Type: {exception.GetType().FullName}");
            Console.WriteLine($"Message: {exception.Message}");
            Console.WriteLine($"Path: {request.Path}");
            return true;
        };

        options.ExceptionHandling.Hooks.BeforeMappingAsync = async (mapper, request) =>
        {
            Console.WriteLine($"Mapper: {mapper.GetType().Name}");
            return mapper;
        };

        options.ExceptionHandling.Hooks.AfterMappingAsync = async (descriptor, mapper, request) =>
        {
            Console.WriteLine($"Result: {descriptor.StatusCode} - {descriptor.Type}");
            Console.WriteLine($"ShouldLog: {descriptor.ShouldLog}, LogLevel: {descriptor.LogLevel}");
            return descriptor;
        };
    });
```

---

## Conflict with Other Exception Middleware {#conflict-with-other-exception-middleware}

**Problem:** **AspNetConventions** conflicts with another exception handling middleware.

**Solution:** Ensure proper middleware order:

```csharp
var app = builder.Build();

// AspNetConventions should typically come after authentication
// but before other exception handlers

app.UseAuthentication();
app.UseAuthorization();
var api = app.UseAspNetConventions();  // Add here

// Don't add UseExceptionHandler after this
// app.UseExceptionHandler("/error");  // Remove this

app.MapControllers();
```

Or exclude certain exceptions/status codes:
```csharp
// Let other middleware handle these
options.ExceptionHandling.ExcludeStatusCodes.Add(HttpStatusCode.Unauthorized);
options.ExceptionHandling.ExcludeException.Add(typeof(SecurityException));
```
