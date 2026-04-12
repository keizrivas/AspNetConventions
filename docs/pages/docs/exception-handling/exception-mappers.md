# Exception Mappers

Exception mappers transform exceptions into structured error responses. Create custom mappers for your domain exceptions to return meaningful HTTP responses with appropriate status codes and error data.

---

## Creating a Custom Mapper {#creating-a-custom-mapper}

Inherit from `ExceptionMapper<TException>` and implement the `MapException` method:

```csharp
using System.Net;
using AspNetConventions.ExceptionHandling.Mappers;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http.Services;
using Microsoft.Extensions.Logging;

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
```

### Registering Your Mapper {#registering-your-mapper}

```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Exceptions.Mappers.Add(new OrderNotFoundExceptionMapper());
    });
```

---

## ExceptionMapper&lt;T&gt; Base Class {#exceptionmapper-base-class}

The [`ExceptionMapper`{.code-left}](./configuration.md#iexceptionmapper)`<TException>`{.code-right} base class provides:

```csharp
public abstract class ExceptionMapper<TException> : IExceptionMapper
    where TException : Exception
{
    // Override this to customize type matching
    public virtual bool CanMapException(Exception exception, RequestDescriptor request)
    {
        return exception is TException;
    }

    // Implement this to transform the exception
    public abstract ExceptionDescriptor MapException(
        TException exception,
        RequestDescriptor request);
}
```

### CanMapException {#canmapexception}

Override `CanMapException` for custom matching logic:

```csharp
public class HttpExceptionMapper : ExceptionMapper<HttpRequestException>
{
    public override bool CanMapException(Exception exception, RequestDescriptor request)
    {
        // Match any HttpRequestException, including derived types
        return exception is HttpRequestException;
    }

    public override ExceptionDescriptor MapException(
        HttpRequestException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            Type = "HTTP_REQUEST_FAILED",
            StatusCode = HttpStatusCode.BadGateway,
            Message = "External service request failed.",
            LogLevel = LogLevel.Error
        };
    }
}
```

---

## Building ExceptionDescriptor {#building-exceptiondescriptor}

The `ExceptionDescriptor` controls every aspect of the error response:

### Simple Descriptor {#simple-descriptor}

```csharp
return new ExceptionDescriptor
{
    Type = "INVALID_REQUEST",
    StatusCode = HttpStatusCode.BadRequest,
    Message = "The request was invalid."
};
```

### Full Descriptor {#full-descriptor}

```csharp
return new ExceptionDescriptor
{
    Type = "PAYMENT_FAILED",
    StatusCode = HttpStatusCode.PaymentRequired,
    Message = "Payment processing failed.",
    Value = new
    {
        TransactionId = exception.TransactionId,
        ErrorCode = exception.ErrorCode,
        Reason = exception.Reason
    },
    ShouldLog = true,
    LogLevel = LogLevel.Critical
};
```

See [`ExceptionDescriptor`](./configuration.md#exceptiondescriptor) for more information.

---

## Common Mapper Patterns {#common-mapper-patterns}

### Not Found Exception {#not-found-exception}

```csharp
public class ResourceNotFoundException : Exception
{
    public string ResourceType { get; }
    public string ResourceId { get; }

    public ResourceNotFoundException(string resourceType, string resourceId)
        : base($"{resourceType} '{resourceId}' was not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

public class ResourceNotFoundExceptionMapper : ExceptionMapper<ResourceNotFoundException>
{
    public override ExceptionDescriptor MapException(
        ResourceNotFoundException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            Type = "RESOURCE_NOT_FOUND",
            StatusCode = HttpStatusCode.NotFound,
            Message = exception.Message,
            Value = new
            {
                exception.ResourceType,
                exception.ResourceId
            },
            LogLevel = LogLevel.Warning
        };
    }
}
```

### Validation Exception {#validation-exception}

```csharp
public class BusinessValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public BusinessValidationException(Dictionary<string, string[]> errors)
        : base("Business validation failed.")
    {
        Errors = errors;
    }
}

public class BusinessValidationExceptionMapper : ExceptionMapper<BusinessValidationException>
{
    public override ExceptionDescriptor MapException(
        BusinessValidationException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            Type = "BUSINESS_VALIDATION_ERROR",
            StatusCode = HttpStatusCode.UnprocessableEntity,
            Message = exception.Message,
            Value = exception.Errors,
            ShouldLog = false  // Expected validation errors don't need logging
        };
    }
}
```

### Conflict Exception {#conflict-exception}

```csharp
public class DuplicateEntityException : Exception
{
    public string EntityType { get; }
    public string ConflictingField { get; }
    public object ConflictingValue { get; }

    public DuplicateEntityException(string entityType, string field, object value)
        : base($"A {entityType} with {field} '{value}' already exists.")
    {
        EntityType = entityType;
        ConflictingField = field;
        ConflictingValue = value;
    }
}

public class DuplicateEntityExceptionMapper : ExceptionMapper<DuplicateEntityException>
{
    public override ExceptionDescriptor MapException(
        DuplicateEntityException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            Type = "DUPLICATE_ENTITY",
            StatusCode = HttpStatusCode.Conflict,
            Message = exception.Message,
            Value = new
            {
                EntityType = exception.EntityType,
                Field = exception.ConflictingField,
                Value = exception.ConflictingValue
            },
            LogLevel = LogLevel.Warning
        };
    }
}
```

### Rate Limit Exception {#rate-limit-exception}

```csharp
public class RateLimitExceededException : Exception
{
    public int RetryAfterSeconds { get; }
    public string Limit { get; }

    public RateLimitExceededException(int retryAfter, string limit)
        : base("Rate limit exceeded.")
    {
        RetryAfterSeconds = retryAfter;
        Limit = limit;
    }
}

public class RateLimitExceptionMapper : ExceptionMapper<RateLimitExceededException>
{
    public override ExceptionDescriptor MapException(
        RateLimitExceededException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            Type = "RATE_LIMIT_EXCEEDED",
            StatusCode = HttpStatusCode.TooManyRequests,
            Message = $"Rate limit exceeded. Retry after {exception.RetryAfterSeconds} seconds.",
            Value = new
            {
                RetryAfterSeconds = exception.RetryAfterSeconds,
                Limit = exception.Limit
            },
            ShouldLog = false  // Expected condition, no need to log
        };
    }
}
```

### External Service Exception {#external-service-exception}

```csharp
public class ExternalServiceException : Exception
{
    public string ServiceName { get; }
    public int? ServiceErrorCode { get; }

    public ExternalServiceException(string serviceName, string message, int? errorCode = null)
        : base(message)
    {
        ServiceName = serviceName;
        ServiceErrorCode = errorCode;
    }
}

public class ExternalServiceExceptionMapper : ExceptionMapper<ExternalServiceException>
{
    public override ExceptionDescriptor MapException(
        ExternalServiceException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            Type = "EXTERNAL_SERVICE_ERROR",
            StatusCode = HttpStatusCode.BadGateway,
            Message = $"External service '{exception.ServiceName}' is unavailable.",
            Value = new
            {
                Service = exception.ServiceName,
                ErrorCode = exception.ServiceErrorCode
            },
            LogLevel = LogLevel.Error
        };
    }
}
```

---

## Mappers with Dependencies {#mappers-with-dependencies}

For mappers that need injected services, create them with dependencies and register appropriately:

```csharp
public class LoggingExceptionMapper : ExceptionMapper<MyException>
{
    private readonly ILogger<LoggingExceptionMapper> _logger;
    private readonly IMetricsService _metrics;

    public LoggingExceptionMapper(
        ILogger<LoggingExceptionMapper> logger,
        IMetricsService metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public override ExceptionDescriptor MapException(
        MyException exception,
        RequestDescriptor request)
    {
        // Use injected services
        _metrics.IncrementCounter("my_exception_count");

        return new ExceptionDescriptor
        {
            Type = "MY_ERROR",
            StatusCode = HttpStatusCode.InternalServerError,
            Message = exception.Message
        };
    }
}

// Registration with DI
var serviceProvider = builder.Services.BuildServiceProvider();
var mapper = new LoggingExceptionMapper(
    serviceProvider.GetRequiredService<ILogger<LoggingExceptionMapper>>(),
    serviceProvider.GetRequiredService<IMetricsService>());

builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        options.Exceptions.Mappers.Add(mapper);
    });
```

---

## Logging Best Practices {#logging-best-practices}

**When to Log:**

| Scenario | ShouldLog | LogLevel |
|----------|-----------|----------|
| Unexpected errors | `true` | `Error` or `Critical` |
| Client errors (bad input) | `true` | `Warning` |
| Validation failures | `false` | — |
| Rate limiting | `false` | — |
| Not found (expected) | `true` | `Warning` or `Information` |
| External service failures | `true` | `Error` |

**Log Level Guidelines:**

```csharp
// Critical — System is unusable
LogLevel = LogLevel.Critical  // Database down, critical service failure

// Error — Operation failed
LogLevel = LogLevel.Error     // Unexpected exceptions, failed operations

// Warning — Something unexpected but handled
LogLevel = LogLevel.Warning   // Not found, invalid arguments

// Information — Normal operation details
LogLevel = LogLevel.Information  // Successful but noteworthy events
```