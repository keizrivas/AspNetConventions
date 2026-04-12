# Configuration

Complete reference for all Exception Handling configuration options.

---

## ExceptionHandlingOptions

**Namespace:** `AspNetConventions.Configuration.Options`
**Accessed via:** [`options`{.code-left}](/docs/configuration-reference/#aspnetconventionoptions)`.ExceptionHandling`{.code-right}

Controls how unhandled exceptions are caught, mapped, and formatted across the application.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables global exception handling. When `false`, exceptions propagate normally |
| `Mappers` | `HashSet<`{.code-left}[`IExceptionMapper`{.code-left .code-right}](#iexceptionmapper)`>`{.code-right} | `[]` | Registered custom exception mappers. Evaluated before the built-in default mapper |
| `ExcludeStatusCodes` | `HashSet<HttpStatusCode>` | `[]` | HTTP status codes that should not be intercepted or reformatted |
| `ExcludeException` | `HashSet<Type>` | `[]` | Exception types that should bypass handling and propagate normally |
| `Hooks` | [`ExceptionHandlingHooks`](#exceptionhandlinghooks) | `new()` | Hooks for intercepting the exception handling pipeline |

### Disabling Exception Handling

```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        // Exceptions propagate normally — no interception
        options.ExceptionHandling.IsEnabled = false;
    });
```

### Excluding Status Codes

Prevent specific HTTP status codes from being reformatted — useful when other middleware handles certain responses:

```csharp
options.ExceptionHandling.ExcludeStatusCodes.Add(HttpStatusCode.NotFound);
options.ExceptionHandling.ExcludeStatusCodes.Add(HttpStatusCode.Unauthorized);
options.ExceptionHandling.ExcludeStatusCodes.Add(HttpStatusCode.Forbidden);
```

When a mapper returns an excluded status code, the exception is re-thrown to be handled by other middleware.

### Excluding Exception Types

Allow specific exception types to bypass the handler entirely and propagate up the pipeline:

```csharp
options.ExceptionHandling.ExcludeException.Add(typeof(OperationCanceledException));
options.ExceptionHandling.ExcludeException.Add(typeof(TaskCanceledException));
```

---

## IExceptionMapper

**Namespace:** `AspNetConventions.Core.Abstractions`

The contract that all exception mappers must implement. Custom mappers registered in [`ExceptionHandlingOptions`{.code-left}](#exceptionhandlingoptions)`.Mappers`{.code-right} must implement this interface.

| Member | Signature | Description |
|---|---|---|
| `CanMap` | `(Exception) → bool` | Returns `true` if this mapper handles the given exception type |
| `Map` | `(Exception, `{.code-left}[`RequestDescriptor`{.code-left .code-right}](/docs/response-formatting/configuration/#requestdescriptor)`)  →  `{.code-left .code-right}[`ExceptionDescriptor`{.code-right}](#exceptiondescriptor) | Produces the [`ExceptionDescriptor`](#exceptiondescriptor) that controls the error response |

### Implementing IExceptionMapper

The recommended approach is to extend the abstract `ExceptionMapper<TException>` base class, which provides a strongly-typed implementation of `IExceptionMapper`:

```csharp
public class OrderNotFoundExceptionMapper : ExceptionMapper<OrderNotFoundException>
{
    public override ExceptionDescriptor MapException(
        OrderNotFoundException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            StatusCode = HttpStatusCode.NotFound,
            Type = "ORDER_NOT_FOUND",
            Message = exception.Message
        };
    }
}
```

For cases where you need to handle multiple exception types in a single mapper, implement `IExceptionMapper` directly:

```csharp
public class PaymentExceptionMapper : IExceptionMapper
{
    public bool CanMap(Exception exception) =>
        exception is PaymentFailedException or PaymentDeclinedException;

    public ExceptionDescriptor Map(Exception exception, RequestDescriptor request)
    {
        var statusCode = exception is PaymentDeclinedException
            ? HttpStatusCode.UnprocessableEntity
            : HttpStatusCode.BadGateway;

        return new ExceptionDescriptor
        {
            StatusCode = statusCode,
            Type = "PAYMENT_ERROR",
            Message = exception.Message
        };
    }
}
```

---

### Registering Custom Mappers

Add instances of your `ExceptionMapper<T>` implementations to the `Mappers` set:

```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        options.ExceptionHandling.Mappers.Add(new NotFoundExceptionMapper());
        options.ExceptionHandling.Mappers.Add(new PaymentFailedExceptionMapper());
        options.ExceptionHandling.Mappers.Add(new RateLimitExceptionMapper());
    });
```

### Resolution Order

When an exception is thrown, mappers are resolved in this order:

1. **Custom mappers** in `Mappers`, evaluated in insertion order — first match wins
2. **Built-in mappers** for `ArgumentNullException`, `ArgumentException`, `ValidationException`, etc.
3. **DefaultExceptionMapper** as the final fallback (returns 500)

### Replacing Built-in Mappers

To override a built-in mapper, register your own mapper for that exception type:

```csharp
// This replaces the built-in ArgumentNullException mapper
options.ExceptionHandling.Mappers.Add(new CustomArgumentNullExceptionMapper());
```

Custom mappers always take precedence over built-in ones.

---

## ExceptionDescriptor

The `ExceptionDescriptor` is produced by mappers and controls the error response:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `StatusCode` | `HttpStatusCode?` | — | HTTP status code for the response |
| `Type` | `string?` | — | Machine-readable error code (e.g., `"ORDER_NOT_FOUND"`) |
| `Message` | `string?` | — | Human-readable error message |
| `Value` | `object?` | `null` | Structured error data (appears in `errors` field) |
| `ShouldLog` | `bool` | `true` | Whether to log this exception |
| `LogLevel` | `LogLevel` | `Error` | Log level when `ShouldLog` is `true` |
| `Exception` | `Exception?` | — | Original exception (set automatically) |

### Generic ExceptionDescriptor

For strongly-typed values, use `ExceptionDescriptor<TValue>`:

```csharp
public override ExceptionDescriptor MapException(
    OrderNotFoundException exception,
    RequestDescriptor request)
{
    return new ExceptionDescriptor<OrderErrorDetails>
    {
        StatusCode = HttpStatusCode.NotFound,
        Type = "ORDER_NOT_FOUND",
        Message = exception.Message,
        Value = new OrderErrorDetails
        {
            OrderId = exception.OrderId,
            Reason = "Order does not exist"
        }
    };
}
```

---

## ExceptionHandlingHooks

**Namespace:** `AspNetConventions.Core.Hooks`
**Accessed via:** [`options`{.code-left}](/docs/configuration-reference/#aspnetconventionoptions)`.`{.code-left .code-right}[`ExceptionHandling`{.code-left .code-right}](#exceptionhandlingoptions)`.Hooks`{.code-right}

Hooks provide fine-grained control over the exception handling pipeline. All hooks are asynchronous.

| Property | Delegate Signature | Description |
|---|---|---|
| `TryHandleAsync` | `(Exception) → Task` | Global exception observer. Runs for every exception across Minimal APIs, MVC Controllers, and Razor Pages — without affecting the normal handling pipeline |
| `ShouldHandleAsync` | `(Exception, `{.code-left}[`RequestDescriptor`{.code-left .code-right}](/docs/response-formatting/configuration/#requestdescriptor)`) → Task<bool>`{.code-right} | Return `false` to skip handling for a specific exception |
| `BeforeMappingAsync` | `(`{.code-left}[`IExceptionMapper`{.code-left .code-right}](#iexceptionmapper)`, `{.code-left .code-right}[`RequestDescriptor`{.code-left .code-right}](/docs/response-formatting/configuration/#requestdescriptor)`) → Task<`{.code-left .code-right}[`IExceptionMapper`{.code-left .code-right}](#iexceptionmapper)`>`{.code-right} | Called before mapping. Can replace the mapper |
| `AfterMappingAsync` | `(`{.code-left}[`ExceptionDescriptor`{.code-left .code-right}](#exceptiondescriptor)`, `{.code-left .code-right}[`IExceptionMapper`{.code-left .code-right}](#iexceptionmapper)`, `{.code-left .code-right}[`RequestDescriptor`{.code-left .code-right}](/docs/response-formatting/configuration/#requestdescriptor)`) → Task<`{.code-right .code-left}[`ExceptionDescriptor`{.code-left .code-right}](#exceptiondescriptor)`>`{.code-right} | Called after mapping. Can modify the descriptor |

### ShouldHandleAsync

Determine whether an exception should be handled:

```csharp
options.ExceptionHandling.Hooks.ShouldHandleAsync = async (exception, request) =>
{
    // Skip handling for cancelled requests
    if (exception is OperationCanceledException)
        return false;

    // Skip handling for specific paths
    if (request.Path.StartsWith("/health"))
        return false;

    return true;
};
```

### BeforeMappingAsync

Intercept before the mapper runs:

```csharp
options.ExceptionHandling.Hooks.BeforeMappingAsync = async (mapper, request) =>
{
    // Log which mapper is being used
    _logger.LogDebug(
        "Handling exception with mapper: {MapperType}",
        mapper.GetType().Name);

    // Optionally replace the mapper
    return mapper;
};
```

### AfterMappingAsync

Modify the descriptor after mapping:

```csharp
options.ExceptionHandling.Hooks.AfterMappingAsync = async (descriptor, mapper, request) =>
{
    // Add correlation ID to all error responses
    var correlationId = request.HttpContext.Request.Headers["X-Correlation-Id"].ToString();

    if (!string.IsNullOrEmpty(correlationId))
    {
        descriptor.Value = new
        {
            OriginalValue = descriptor.Value,
            CorrelationId = correlationId
        };
    }

    // Send alerts for critical errors
    if (descriptor.LogLevel == LogLevel.Critical)
    {
        await _alertService.SendCriticalAlertAsync(descriptor.Exception);
    }

    return descriptor;
};
```

### TryHandleAsync

A global exception observer that runs for every unhandled exception across Minimal APIs, MVC Controllers, and Razor Pages. It does **not** override the pipeline — mapper resolution, logging, and response building all continue normally.

Use it as a central place for cross-cutting concerns like structured logging, alerting, or telemetry:

```csharp
options.ExceptionHandling.Hooks.TryHandleAsync = async (exception) =>
{
    // Centralized logging for all unhandled exceptions
    _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

    // Send to external observability platform
    await _telemetry.TrackExceptionAsync(exception);
};
```

::: callout info Observer Only
`TryHandleAsync` is a side-effect hook. It does not replace the mapper, modify the response, or short-circuit the pipeline. The normal exception handling flow always continues after it runs.
:::

---

## Default Values Reference

| Option | Default |
|---|---|
| `Exceptions.IsEnabled` | `true` |
| `Exceptions.Mappers` | `[]` (empty) |
| `Exceptions.ExcludeStatusCodes` | `[]` (empty) |
| `Exceptions.ExcludeException` | `[]` (empty) |
| `ExceptionDescriptor.ShouldLog` | `true` |
| `ExceptionDescriptor.LogLevel` | `Error` |
