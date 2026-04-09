# Custom Response Builders

If the default response envelope doesn't match your API contract, you can replace it entirely by implementing `IResponseBuilder` and/or `IErrorResponseBuilder`.

---

## When to Use Custom Builders

- **Different envelope structure** — Your API contract requires a specific JSON shape
- **Legacy API compatibility** — Matching an existing API format
- **Third-party integration** — Conforming to partner/client requirements
- **Minimalist responses** — Removing metadata or simplifying the envelope

::: callout info Before you start
Make sure you understand [`ApiResult`](/docs/response-formatting/configuration/#apiresult) and [`RequestDescriptor`](/docs/response-formatting/configuration/#requestdescriptor) before proceeding. The following examples build on these concepts.
:::

---

## IResponseBuilder

Controls the shape of **success responses** (`2xx`, `3xx` status codes).

```csharp
public interface IResponseBuilder
{
    /// <summary>
    /// Determines if the value is already a wrapped response.
    /// </summary>
    bool IsWrappedResponse(object? value);

    /// <summary>
    /// Builds the response envelope.
    /// </summary>
    object BuildResponse(ApiResult apiResult, RequestDescriptor request);
}
```

### Implementing IResponseBuilder

```csharp
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;

public class MyResponseBuilder : IResponseBuilder
{
    public bool IsWrappedResponse(object? value) => false;

    public object BuildResponse(ApiResult apiResult, RequestDescriptor request)
    {
        return new
        {
            success = apiResult.IsSuccess,
            code = (int)apiResult.StatusCode,
            type = apiResult.Type,
            message = apiResult.Message,
            data = apiResult.GetValue(),
        };
    }
}
```

---

## IErrorResponseBuilder

Controls the shape of **error responses** (`4xx`, `5xx` status codes).

```csharp
public interface IErrorResponseBuilder
{
    /// <summary>
    /// Determines if the value is already a wrapped error response.
    /// </summary>
    bool IsWrappedResponse(object? value);

    /// <summary>
    /// Builds the error response envelope.
    /// </summary>
    object BuildResponse(ApiResult apiResult, Exception? exception, RequestDescriptor request);
}
```

### Implementing IErrorResponseBuilder

```csharp
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;

public class MyErrorResponseBuilder : IErrorResponseBuilder
{
    public bool IsWrappedResponse(object? value) => false;

    public object BuildResponse(
        ApiResult apiResult,
        Exception? exception,
        RequestDescriptor request)
    {
        return new
        {
            success = false,
            code = (int)apiResult.StatusCode,
            type = apiResult.Type,
            message = apiResult.Message,
            errors = apiResult.GetValue(),  // Validation errors, etc.
        };
    }
}
```

The `exception` parameter is the original exception (if one was thrown). Use it for logging or debugging, but avoid exposing details in production responses.

---

## Registering Custom Builders

Register your custom builders in the options configuration:

```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Response.ResponseBuilder = new MyResponseBuilder();
        options.Response.ErrorResponseBuilder = new MyErrorResponseBuilder();
    });
```

You can replace one or both builders independently. If you only set `ResponseBuilder`, the default error builder is kept, and vice versa.

---

## Preventing Double-Wrapping

The `IsWrappedResponse` method prevents your response from being wrapped twice. This is important when:

- Your service layer returns pre-wrapped responses
- You have a custom response type that's already in the correct format
- You want certain responses to bypass wrapping entirely

```csharp
// Your custom wrapper type
public class MyApiResponse<T>
{
    public bool Success { get; set; }
    public int Code { get; set; }
    public T? Data { get; set; }
}

// In your builder
public bool IsWrappedResponse(object? value)
{
    // Check if value is already your wrapper type
    return value?.GetType().IsGenericType == true
        && value.GetType().GetGenericTypeDefinition() == typeof(MyApiResponse<>);
}
```

When `IsWrappedResponse` returns `true`:
1. `BuildResponse` is **not** called
2. The value is serialized **as-is**
3. No envelope is added

---

## Example

### Flat Envelope

A minimal, flat response structure:

```csharp
public class FlatResponseBuilder : IResponseBuilder
{
    public bool IsWrappedResponse(object? value) => false;

    public object BuildResponse(ApiResult apiResult, RequestDescriptor request)
    {
        return new
        {
            ok = true,
            code = (int)apiResult.StatusCode,
            payload = apiResult.GetValue()
        };
    }
}

public class FlatErrorResponseBuilder : IErrorResponseBuilder
{
    public bool IsWrappedResponse(object? value) => false;

    public object BuildResponse(
        ApiResult apiResult,
        Exception? exception,
        RequestDescriptor request)
    {
        return new
        {
            ok = false,
            code = (int)apiResult.StatusCode,
            error = apiResult.Type,
            message = apiResult.Message
        };
    }
}
```

**Success response:**
```json
{ "ok": true, "code": 200, "payload": { "userId": 1 } }
```

**Error response:**
```json
{ "ok": false, "code": 404, "error": "NOT_FOUND", "message": "User not found." }
```

---

### HATEOAS-Style Links

Adding hypermedia links to responses:

```csharp
public class HateoasResponseBuilder : IResponseBuilder
{
    public bool IsWrappedResponse(object? value) => false;

    public object BuildResponse(ApiResult apiResult, RequestDescriptor request)
    {
        var links = new Dictionary<string, string>
        {
            ["self"] = request.Path
        };

        // Add pagination links if available
        if (apiResult.Pagination is not null)
        {
            var pagination = apiResult.Pagination;
            if (pagination.Links.NextPageUrl is not null)
                links["next"] = pagination.Links.NextPageUrl;
            if (pagination.Links.PreviousPageUrl is not null)
                links["prev"] = pagination.Links.PreviousPageUrl;
        }

        return new
        {
            data = apiResult.GetValue(),
            status = (int)apiResult.StatusCode,
            _links = links
        };
    }
}
```

**Response:**
```json
{
  "data": { "id": 1, "name": "John" },
  "status": 200,
  "_links": {
    "self": "/api/users/1"
  }
}
```

---

### RFC 7807 Problem Details

Conforming to the Problem Details specification:

```csharp
public class ProblemDetailsErrorBuilder : IErrorResponseBuilder
{
    private readonly string _baseUrl;

    public ProblemDetailsErrorBuilder(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    public bool IsWrappedResponse(object? value) => false;

    public object BuildResponse(
        ApiResult apiResult,
        Exception? exception,
        RequestDescriptor request)
    {
        return new
        {
            type = $"{_baseUrl}/errors/{apiResult.Type?.ToLowerInvariant()}",
            title = GetTitle(apiResult.StatusCode),
            status = (int)apiResult.StatusCode,
            detail = apiResult.Message,
            instance = request.Path,
            traceId = request.TraceId
        };
    }

    private static string GetTitle(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "Bad Request",
        HttpStatusCode.Unauthorized => "Unauthorized",
        HttpStatusCode.Forbidden => "Forbidden",
        HttpStatusCode.NotFound => "Not Found",
        HttpStatusCode.Conflict => "Conflict",
        HttpStatusCode.InternalServerError => "Internal Server Error",
        _ => "Error"
    };
}
```

**Response:**
```json
{
  "type": "https://api.example.com/errors/not_found",
  "title": "Not Found",
  "status": 404,
  "detail": "User with ID 123 was not found.",
  "instance": "/api/users/123",
  "traceId": "00-abc123..."
}
```

---

### Conditional Wrapping

Wrap only certain response types:

```csharp
public class ConditionalResponseBuilder : IResponseBuilder
{
    public bool IsWrappedResponse(object? value)
    {
        // Don't wrap primitive types or strings
        if (value is null) return true;
        var type = value.GetType();
        return type.IsPrimitive || type == typeof(string);
    }

    public object BuildResponse(ApiResult apiResult, RequestDescriptor request)
    {
        return new
        {
            status = "success",
            data = apiResult.GetValue(),
            metadata = new
            {
                timestamp = DateTime.UtcNow,
                path = request.Path
            }
        };
    }
}
```

---

## Using Dependency Injection

For builders that need dependencies, register them with DI:

```csharp
public class LoggingResponseBuilder : IResponseBuilder
{
    private readonly ILogger<LoggingResponseBuilder> _logger;

    public LoggingResponseBuilder(ILogger<LoggingResponseBuilder> logger)
    {
        _logger = logger;
    }

    public bool IsWrappedResponse(object? value) => false;

    public object BuildResponse(ApiResult apiResult, RequestDescriptor request)
    {
        _logger.LogInformation(
            "Response: {Method} {Path} → {StatusCode}",
            request.Method, request.Path, (int)apiResult.StatusCode);

        return new
        {
            success = apiResult.IsSuccess,
            code = (int)apiResult.StatusCode,
            data = apiResult.GetValue()
        };
    }
}

// Registration
builder.Services.AddSingleton<IResponseBuilder, LoggingResponseBuilder>();

builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        // Builder will be resolved from DI
        options.Response.ResponseBuilder =
            builder.Services.BuildServiceProvider()
                .GetRequiredService<IResponseBuilder>();
    });
```

---

## Best Practices

1. **Keep builders simple** — Focus on structure transformation, not business logic
2. **Use consistent naming** — Match your API documentation and client expectations
3. **Handle nulls gracefully** — `ApiResult.GetValue()` may return `null`
4. **Don't expose exceptions** — Use `exception` for logging only in production
5. **Test both success and error paths** — Ensure both builders produce valid JSON
6. **Document your envelope** — Update API documentation to reflect custom shapes
