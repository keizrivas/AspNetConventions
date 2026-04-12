# Exception Handling

**AspNetConventions** provides centralized, application-wide exception handling that catches unhandled exceptions, formats them into consistent error responses, and optionally logs them — all without scattering `try/catch` blocks through your web app.

---

## Why Centralized Exception Handling? {#why-centralized-exception-handling}

Without centralized handling, exceptions lead to inconsistent error responses, duplicated error handling code, and missed logging opportunities.

**Without AspNetConventions:**
```csharp
// Repeated try/catch in every controller
[HttpGet("{id}")]
public ActionResult GetUser(int id)
{
    try
    {
        var user = _userService.GetById(id);
        if (user is null)
            return NotFound(new { error = "User not found" });
        return Ok(user);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting user");
        return StatusCode(500, new { error = "Something went wrong" });
    }
}
```

**With AspNetConventions:**
```csharp
// Clean controller code — exceptions handled automatically
[HttpGet("{id}")]
public ActionResult GetUser(int id)
{
    var user = _userService.GetById(id)
        ?? throw new NotFoundException($"User {id}");
    return Ok(user);
}
```

---

## Features {#features}

- **Automatic exception catching** — Intercepts all unhandled exceptions from controllers and Minimal APIs
- **Consistent error responses** — All errors follow the same JSON structure
- **Custom exception mappers** — Map domain exceptions to specific HTTP responses
- **Built-in mappers** — Common exceptions like `ArgumentNullException` handled automatically
- **Configurable logging** — Control log level and whether to log per exception type
- **Exception hooks** — Intercept exceptions for alerting, telemetry, or custom processing
- **Exclusion support** — Exclude specific exceptions or status codes from handling

---

## How It Works {#how-it-works}

**AspNetConventions** hooks into ASP.NET Core at the framework level so every unhandled exception — regardless of where it originates — flows through the same pipeline. You get a single place to define mappers, configure logging, and attach hooks, without wiring anything per-controller or per-endpoint.

The integration point differs slightly by hosting model, but the pipeline that runs after interception is identical in all cases:

```text
Action: Exception thrown (Controller / Minimal API / Razor Page)
      ↓
Action: Intercepted by AspNetConventions
      ↓
Hook: ShouldHandleAsync
      ↓
Action: Mapper resolution (options.Exceptions.Mappers)
      ↓
Hook: TryHandleAsync
      ↓
Hook: BeforeMappingAsync
      ↓
Action: Mapper.MapException() → ExceptionDescriptor
      ↓
Hook: AfterMappingAsync
      ↓
Action: IErrorResponseBuilder formats the descriptor into a JSON response
      ↓
Response written to the client
```

Because the pipeline is shared, a mapper or hook registered once covers all three hosting models transparently.

---

## Built-in Exception Mappers {#built-in-exception-mappers}

The following exceptions are handled automatically:

| Exception Type | Status Code | Error Type |
|----------------|-------------|------------|
| `ArgumentNullException` | **400** Bad Request | `ARGUMENT_NULL` |
| `ArgumentOutOfRangeException` | **400** Bad Request | `ARGUMENT_OUT_OF_RANGE` |
| `ArgumentException` | **400** Bad Request | `INVALID_ARGUMENT` |
| `ValidationException` | **400** Bad Request | `VALIDATION_ERROR` |
| `UnauthorizedAccessException` | **401** Unauthorized | `UNAUTHORIZED` |
| `SecurityException` | **403** Forbidden | `FORBIDDEN` |
| `KeyNotFoundException` | **404** Not Found | `NOT_FOUND` |
| `FileNotFoundException` | **404** Not Found | `FILE_NOT_FOUND` |
| `DirectoryNotFoundException` | **404** Not Found | `DIRECTORY_NOT_FOUND` |
| `InvalidOperationException` | **409** Conflict | `INVALID_OPERATION` |
| `ObjectDisposedException` | **410** Gone | `OBJECT_DISPOSED` |
| `NotImplementedException` | **501** Not Implemented | `NOT_IMPLEMENTED` |
| `TimeoutException` | **408** Request Timeout | `TIMEOUT` |
| `TaskCanceledException` | **408** Request Timeout | `REQUEST_CANCELLED` |
| `OperationCanceledException` | **408** Request Timeout | `OPERATION_CANCELLED` |
| Any other exception | **500** Internal Server Error | `UNEXPECTED_ERROR` |

### Default Behavior {#default-behavior}

Without any configuration, all unhandled exceptions return a `500 Internal Server Error`:

```json
{
  "status": "failure",
  "statusCode": 500,
  "type": "UNEXPECTED_ERROR",
  "message": "An unexpected error occurred.",
  "errors": null,
  "metadata": {
    "requestType": "GET",
    "timestamp": "2024-01-15T10:30:00.000000Z",
    "traceId": "00-ed89d1cc507c35126d6f0e933984f774-99b8b9a3feb75652-00",
    "path": "/api/users/99"
  }
}
```

Customize default error fallbacks with [`ErrorResponseOptions`](../response-formatting/configuration.md#errorresponseoptions):

```csharp
options.Response.ErrorResponse.DefaultStatusCode = HttpStatusCode.BadRequest;
options.Response.ErrorResponse.DefaultErrorType = "INTERNAL_ERROR";
options.Response.ErrorResponse.DefaultErrorMessage = "An error occurred. If this issue persists please contact us through our help center at help.";
```
---

## Custom Exception Mappers {#custom-exception-mappers}

Create mappers for your domain exceptions to return meaningful error responses:

```csharp
// Define your domain exception
public class OrderNotFoundException : Exception
{
    public int OrderId { get; }
    public OrderNotFoundException(int orderId)
        : base($"Order {orderId} was not found.")
        => OrderId = orderId;
}

// Create a mapper
public class OrderNotFoundExceptionMapper : ExceptionMapper<OrderNotFoundException>
{
    public override ExceptionDescriptor MapException(
        OrderNotFoundException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            Type = "ORDER_NOT_FOUND",
            StatusCode = HttpStatusCode.NotFound,
            Message = exception.Message,
            Value = new { exception.OrderId },
            LogLevel = LogLevel.Warning,
            ShouldLog = true
        };
    }
}

// Register the mapper
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Exceptions.Mappers.Add(new OrderNotFoundExceptionMapper());
    });
```

**Response:**
```json
{
  "status": "failure",
  "statusCode": 404,
  "type": "ORDER_NOT_FOUND",
  "message": "Order 123 was not found.",
  "errors": { "orderId": 123 },
  "metadata": { ... }
}
```


### Mapper Resolution {#mapper-resolution}

When an exception is thrown, **AspNetConventions** resolves the mapper in this order:

1. **Custom mappers** — Checked in registration order, first match wins
2. **Built-in mappers** — `ArgumentNullException`, `ArgumentException`, `ValidationException`, etc.
3. **Default fallback** — Returns **500** Internal Server Error

### Type Matching {#type-matching}

Mappers use the `CanMapException` method to determine if they can handle an exception:

```csharp
public class HttpExceptionMapper : ExceptionMapper<HttpRequestException>
{
    public override bool CanMapException(Exception exception, RequestDescriptor request)
    {
        // Also handle derived types
        return exception is HttpRequestException;
    }
}
```

See [Exception Mappers](./exception-mappers.md) for complete documentation.

---

## Exception Hooks {#exception-hooks}

Intercept exceptions globally for alerting, telemetry, or custom processing:

```csharp
options.Exceptions.Hooks.ShouldHandleAsync = async (exception, request) =>
{
    // Skip handling for cancelled requests
    if (exception is OperationCanceledException)
        return false;
    return true;
};

options.Exceptions.Hooks.AfterMappingAsync = async (descriptor, mapper, request) =>
{
    // Send critical errors to alerting system
    if (descriptor.LogLevel == LogLevel.Critical)
    {
        await _alertService.SendAlertAsync(descriptor.Exception);
    }
    return descriptor;
};
```

See [`ExceptionHandlingHooks`](./configuration.md#exceptionhandlinghooks) for more information.
