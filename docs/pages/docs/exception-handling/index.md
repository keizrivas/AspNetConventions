# Exception Handling

**AspNetConventions** provides centralized, application-wide exception handling that catches unhandled exceptions, formats them into consistent error responses, and optionally logs them — all without scattering `try/catch` blocks through your controllers.

---

## Why Centralized Exception Handling?

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

## Features

- **Automatic exception catching** — Intercepts all unhandled exceptions from controllers and Minimal APIs
- **Consistent error responses** — All errors follow the same JSON structure
- **Custom exception mappers** — Map domain exceptions to specific HTTP responses
- **Built-in mappers** — Common exceptions like `ArgumentNullException` handled automatically
- **Configurable logging** — Control log level and whether to log per exception type
- **Exception hooks** — Intercept exceptions for alerting, telemetry, or custom processing
- **Exclusion support** — Exclude specific exceptions or status codes from handling

---

## Default Behavior

Without any configuration, all unhandled exceptions return a `500 Internal Server Error`:

```json
{
  "status": "failure",
  "statusCode": 500,
  "type": "INTERNAL_SERVER_ERROR",
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

---

## Built-in Exception Mappers

The following exceptions are handled automatically:

| Exception Type | Status Code | Response Type | Logged |
|----------------|-------------|---------------|--------|
| `ArgumentNullException` | 400 Bad Request | `ARGUMENT_NULL` | Warning |
| `ArgumentException` | 400 Bad Request | `ARGUMENT_INVALID` | Warning |
| `ValidationException` | 400 Bad Request | `VALIDATION_ERROR` | No |
| Any other exception | 500 Internal Server Error | `INTERNAL_SERVER_ERROR` | Error |

---

## Custom Exception Mappers

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

See [Exception Mappers](/docs/exception-handling/exception-mappers) for complete documentation.

---

## Exception Hooks

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

---

## ExceptionDescriptor Reference

The `ExceptionDescriptor` controls how an exception is transformed into a response:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Type` | `string` | — | Machine-readable error code (e.g., `"ORDER_NOT_FOUND"`) |
| `StatusCode` | `HttpStatusCode` | — | HTTP status code for the response |
| `Message` | `string` | — | Human-readable error message |
| `Value` | `object?` | `null` | Structured error data (appears in `errors` field) |
| `ShouldLog` | `bool` | `true` | Whether to log this exception |
| `LogLevel` | `LogLevel` | `Error` | Log level when logging is enabled |
| `Exception` | `Exception?` | — | Original exception (set automatically) |
