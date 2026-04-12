# Examples

Complete working examples demonstrating Exception Handling across MVC Controllers and Minimal APIs.

---

## Domain Exceptions Library

A common set of domain exceptions for your application:

```csharp
// Base domain exception
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }

    protected DomainException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

// Not found exceptions
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base("ENTITY_NOT_FOUND", $"{entityType} with ID '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

// Validation exception
public class DomainValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }

    public DomainValidationException(Dictionary<string, string[]> errors)
        : base("VALIDATION_FAILED", "One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

// Conflict exception
public class ConflictException : DomainException
{
    public string Resource { get; }
    public string ConflictReason { get; }

    public ConflictException(string resource, string reason)
        : base("CONFLICT", $"Conflict: {reason}")
    {
        Resource = resource;
        ConflictReason = reason;
    }
}

// Unauthorized exception
public class UnauthorizedAccessException : DomainException
{
    public string Resource { get; }
    public string RequiredPermission { get; }

    public UnauthorizedAccessException(string resource, string permission)
        : base("UNAUTHORIZED", $"Access denied to {resource}. Required permission: {permission}")
    {
        Resource = resource;
        RequiredPermission = permission;
    }
}

// Rate limit exception
public class RateLimitException : DomainException
{
    public int RetryAfterSeconds { get; }

    public RateLimitException(int retryAfter)
        : base("RATE_LIMITED", "Too many requests. Please slow down.")
    {
        RetryAfterSeconds = retryAfter;
    }
}
```

---

## Exception Mappers

Mappers for the domain exceptions:

```csharp
public class EntityNotFoundExceptionMapper : ExceptionMapper<EntityNotFoundException>
{
    public override ExceptionDescriptor MapException(
        EntityNotFoundException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            Type = exception.ErrorCode,
            StatusCode = HttpStatusCode.NotFound,
            Message = exception.Message,
            Value = new
            {
                EntityType = exception.EntityType,
                EntityId = exception.EntityId
            },
            LogLevel = LogLevel.Warning,
            ShouldLog = true
        };
    }
}

public class DomainValidationExceptionMapper : ExceptionMapper<DomainValidationException>
{
    public override ExceptionDescriptor MapException(
        DomainValidationException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            Type = exception.ErrorCode,
            StatusCode = HttpStatusCode.BadRequest,
            Message = exception.Message,
            Value = exception.Errors,
            ShouldLog = false  // Validation errors are expected
        };
    }
}

public class ConflictExceptionMapper : ExceptionMapper<ConflictException>
{
    public override ExceptionDescriptor MapException(
        ConflictException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            Type = exception.ErrorCode,
            StatusCode = HttpStatusCode.Conflict,
            Message = exception.Message,
            Value = new
            {
                Resource = exception.Resource,
                Reason = exception.ConflictReason
            },
            LogLevel = LogLevel.Warning
        };
    }
}

public class RateLimitExceptionMapper : ExceptionMapper<RateLimitException>
{
    public override ExceptionDescriptor MapException(
        RateLimitException exception,
        RequestDescriptor request)
    {
        return new ExceptionDescriptor
        {
            Type = exception.ErrorCode,
            StatusCode = HttpStatusCode.TooManyRequests,
            Message = exception.Message,
            Value = new { RetryAfterSeconds = exception.RetryAfterSeconds },
            ShouldLog = false  // Expected behavior, no logging needed
        };
    }
}
```

---

## MVC Controller

A complete controller using domain exceptions:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET /api/orders/{id}
    [HttpGet("{id}")]
    public ActionResult<Order> GetOrder(int id)
    {
        var order = _orderService.GetById(id)
            ?? throw new EntityNotFoundException("Order", id);

        return Ok(order);
    }

    // POST /api/orders
    [HttpPost]
    public ActionResult<Order> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Domain validation
        var errors = _orderService.ValidateOrder(request);
        if (errors.Any())
            throw new DomainValidationException(errors);

        // Check for conflicts
        if (_orderService.HasDuplicateReference(request.Reference))
            throw new ConflictException("Order", $"Order with reference '{request.Reference}' already exists");

        var order = _orderService.Create(request);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    // PUT /api/orders/{id}
    [HttpPut("{id}")]
    public ActionResult<Order> UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
    {
        var order = _orderService.GetById(id)
            ?? throw new EntityNotFoundException("Order", id);

        var errors = _orderService.ValidateUpdate(request);
        if (errors.Any())
            throw new DomainValidationException(errors);

        var updated = _orderService.Update(id, request);
        return Ok(updated);
    }

    // DELETE /api/orders/{id}
    [HttpDelete("{id}")]
    public ActionResult DeleteOrder(int id)
    {
        var order = _orderService.GetById(id)
            ?? throw new EntityNotFoundException("Order", id);

        _orderService.Delete(id);
        return NoContent();
    }
}
```

**Response Examples:**

::: tabs

== tab "GET /api/orders/999 (Not Found):"
```json
{
  "status": "failure",
  "statusCode": 404,
  "type": "ENTITY_NOT_FOUND",
  "message": "Order with ID '999' was not found.",
  "errors": {
    "entityType": "Order",
    "entityId": 999
  },
  "metadata": { ... }
}
```

== tab "POST /api/orders (Validation Error):"
```json
{
  "status": "failure",
  "statusCode": 400,
  "type": "VALIDATION_FAILED",
  "message": "One or more validation errors occurred.",
  "errors": {
    "CustomerEmail": ["Email is required", "Invalid email format"],
    "Items": ["At least one item is required"]
  },
  "metadata": { ... }
}
```

== tab "POST /api/orders (Conflict):"
```json
{
  "status": "failure",
  "statusCode": 409,
  "type": "CONFLICT",
  "message": "Conflict: Order with reference 'ORD-123' already exists",
  "errors": {
    "resource": "Order",
    "reason": "Order with reference 'ORD-123' already exists"
  },
  "metadata": { ... }
}
```
:::

---

## Minimal API

The same functionality using Minimal APIs:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// GET /api/orders/{id}
app.MapGet("/api/orders/{id}", (int id, IOrderService orderService) =>
{
    var order = orderService.GetById(id)
        ?? throw new EntityNotFoundException("Order", id);

    return Results.Ok(order);
});

// POST /api/orders
app.MapPost("/api/orders", (CreateOrderRequest request, IOrderService orderService) =>
{
    var errors = orderService.ValidateOrder(request);
    if (errors.Any())
        throw new DomainValidationException(errors);

    if (orderService.HasDuplicateReference(request.Reference))
        throw new ConflictException("Order", $"Order with reference '{request.Reference}' already exists");

    var order = orderService.Create(request);
    return Results.Created($"/api/orders/{order.Id}", order);
});

// DELETE /api/orders/{id}
app.MapDelete("/api/orders/{id}", (int id, IOrderService orderService) =>
{
    var order = orderService.GetById(id)
        ?? throw new EntityNotFoundException("Order", id);

    orderService.Delete(id);
    return Results.NoContent();
});

app.UseAspNetConventions(options =>
{
    // Exception handling configuration
    options.Exceptions.Mappers.Add(new EntityNotFoundExceptionMapper());
    // Other mappers..
});

app.Run();
```
---

## Alerting Integration

Send alerts for critical errors:

```csharp
options.Exceptions.Hooks.AfterMappingAsync = async (descriptor, mapper, request) =>
{
    // Send alerts for critical errors
    if (descriptor.LogLevel >= LogLevel.Error)
    {
        var alertService = request.HttpContext.RequestServices
            .GetRequiredService<IAlertService>();

        await alertService.SendAlertAsync(new Alert
        {
            Severity = descriptor.LogLevel.ToString(),
            Type = descriptor.Type,
            Message = descriptor.Message,
            Path = request.Path,
            TraceId = request.TraceId,
            Timestamp = DateTime.UtcNow
        });
    }

    return descriptor;
};
```
