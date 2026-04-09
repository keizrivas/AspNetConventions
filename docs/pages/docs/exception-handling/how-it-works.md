# How It Works

**AspNetConventions** intercepts unhandled exceptions at the framework level, transforms them into structured error responses, and optionally logs them — all before ASP.NET Core's default error handling kicks in.

---

## The Exception Handling Pipeline

When an exception propagates out of your controller or Minimal API handler, AspNetConventions processes it through this pipeline:

```
1. Exception thrown in controller/handler
           ↓
2. ShouldHandleAsync hook (can skip handling)
           ↓
3. Check exclusion rules (ExcludeException, ExcludeStatusCodes)
           ↓
4. Resolve appropriate ExceptionMapper
           ↓
5. BeforeMappingAsync hook
           ↓
6. MapException → ExceptionDescriptor
           ↓
7. AfterMappingAsync hook (can modify descriptor)
           ↓
8. Log exception (if ShouldLog = true)
           ↓
9. IErrorResponseBuilder creates response
           ↓
10. JSON error response sent to client
```

---

## MVC Controllers

For MVC Controllers, exception handling is applied via an `IExceptionFilter` registered globally.

**How it works:**

1. Your controller action throws an exception
2. The exception filter catches it before the default error handler
3. AspNetConventions resolves the appropriate `ExceptionMapper<T>`
4. The mapper creates an `ExceptionDescriptor`
5. The `IErrorResponseBuilder` formats the response
6. A JSON error response is returned

```csharp
[HttpGet("{id}")]
public ActionResult GetOrder(int id)
{
    var order = _orderService.GetById(id)
        ?? throw new OrderNotFoundException(id);  // ← Caught by filter

    return Ok(order);
}
```

**What the filter does:**
```
throw OrderNotFoundException(123)
              ↓
    OrderNotFoundExceptionMapper
              ↓
    ExceptionDescriptor {
      StatusCode = 404,
      Type = "ORDER_NOT_FOUND",
      Message = "Order 123 was not found."
    }
              ↓
    IErrorResponseBuilder
              ↓
{
  "status": "failure",
  "statusCode": 404,
  "type": "ORDER_NOT_FOUND",
  "message": "Order 123 was not found.",
  "metadata": { ... }
}
```

---

## Minimal APIs

For Minimal APIs, a global exception handler wraps the entire request pipeline.

**How it works:**

1. Your endpoint handler throws an exception
2. The exception handler middleware catches it
3. AspNetConventions processes it through the same pipeline
4. A JSON error response is returned

```csharp
app.MapGet("/api/orders/{id}", (int id, IOrderService orderService) =>
{
    var order = orderService.GetById(id)
        ?? throw new OrderNotFoundException(id);  // ← Caught by middleware

    return Results.Ok(order);
});
```

---

## Mapper Resolution

When an exception is thrown, AspNetConventions resolves the mapper in this order:

1. **Custom mappers** — Checked in registration order, first match wins
2. **Built-in mappers** — `ArgumentNullException`, `ArgumentException`, `ValidationException`
3. **Default fallback** — Returns 500 Internal Server Error

### Type Matching

Mappers use the `CanMapException` method to determine if they can handle an exception:

```csharp
public virtual bool CanMapException(Exception exception, RequestDescriptor request)
{
    return exception is TException;  // Exact type match
}
```

Override this method for custom matching logic:

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

### Inheritance Chain

The most specific mapper wins. If you have mappers for both `Exception` and `NotFoundException`:

```csharp
options.Exceptions.Mappers.Add(new NotFoundExceptionMapper());      // Specific
options.Exceptions.Mappers.Add(new GenericExceptionMapper());        // General
```

Throwing `NotFoundException` uses `NotFoundExceptionMapper`, not `GenericExceptionMapper`.

---

## Exclusion Rules

Exceptions can bypass handling through exclusion rules:

### Exclude by Exception Type

```csharp
options.Exceptions.ExcludeException.Add(typeof(OperationCanceledException));
options.Exceptions.ExcludeException.Add(typeof(TaskCanceledException));
```

Excluded exceptions propagate up the pipeline as if AspNetConventions wasn't there.

### Exclude by Status Code

```csharp
options.Exceptions.ExcludeStatusCodes.Add(HttpStatusCode.NotFound);
options.Exceptions.ExcludeStatusCodes.Add(HttpStatusCode.Unauthorized);
```

When a mapper returns an excluded status code, the exception is re-thrown.

---

## Logging Behavior

Each `ExceptionDescriptor` controls its own logging:

| Property | Effect |
|----------|--------|
| `ShouldLog = true` | Exception is logged at the specified `LogLevel` |
| `ShouldLog = false` | Exception is not logged |
| `LogLevel` | Determines the severity (Debug, Information, Warning, Error, Critical) |

**Built-in logging defaults:**

| Exception | ShouldLog | LogLevel |
|-----------|-----------|----------|
| `ArgumentNullException` | `true` | Warning |
| `ArgumentException` | `true` | Warning |
| `ValidationException` | `false` | — |
| Unhandled (fallback) | `true` | Error |

**Log output example:**
```
[Warning] AspNetConventions: ArgumentNullException at /api/users/create
    Parameter name: request
    at UsersController.Create(CreateUserRequest request)
    ...
```

---

## Integration with Response Formatting

Exception handling integrates with the [Response Formatting](/docs/response-formatting) module:

1. The `ExceptionDescriptor` is converted to an `ApiResult`
2. The `IErrorResponseBuilder` wraps it in the error envelope
3. Metadata is attached (method, path, timestamp, traceId)
4. The response is serialized to JSON

```
ExceptionDescriptor
       ↓
    ApiResult {
      StatusCode = 404,
      Type = "ORDER_NOT_FOUND",
      Message = "Order 123 was not found.",
      Value = { orderId: 123 }
    }
       ↓
IErrorResponseBuilder
       ↓
{
  "status": "failure",
  "statusCode": 404,
  "type": "ORDER_NOT_FOUND",
  "message": "Order 123 was not found.",
  "errors": { "orderId": 123 },
  "metadata": { ... }
}
```

If you've configured a custom `IErrorResponseBuilder`, it will be used for exception responses too.

---

## Hooks Pipeline

Hooks allow you to intercept the exception handling process at various stages:

### ShouldHandleAsync

Called first to determine if the exception should be handled:

```csharp
options.Exceptions.Hooks.ShouldHandleAsync = async (exception, request) =>
{
    // Skip handling for client disconnects
    if (exception is OperationCanceledException)
        return false;

    return true;
};
```

### BeforeMappingAsync

Called before the mapper processes the exception:

```csharp
options.Exceptions.Hooks.BeforeMappingAsync = async (mapper, request) =>
{
    // Log which mapper will be used
    _logger.LogDebug("Using mapper: {Mapper}", mapper.GetType().Name);
    return mapper;
};
```

### AfterMappingAsync

Called after mapping, allows modifying the descriptor:

```csharp
options.Exceptions.Hooks.AfterMappingAsync = async (descriptor, mapper, request) =>
{
    // Add correlation ID to all error responses
    var correlationId = request.HttpContext.Request.Headers["X-Correlation-Id"].ToString();
    descriptor.Value = new
    {
        Original = descriptor.Value,
        CorrelationId = correlationId
    };
    return descriptor;
};
```

### TryHandleAsync

Completely override the exception handling pipeline:

```csharp
options.Exceptions.Hooks.TryHandleAsync = async (exception) =>
{
    // Custom handling logic
    await _customErrorHandler.HandleAsync(exception);
};
```

When `TryHandleAsync` is set, it bypasses the normal pipeline.

---

## Request-Time Processing

Exception handling happens **at request time** when an exception is thrown:

- Full `HttpContext` is available
- Request headers can be inspected
- User context is available for authorization-related errors
- Trace IDs are captured for correlation

This enables context-aware error responses and logging.
