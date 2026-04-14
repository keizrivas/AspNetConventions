# Examples

Complete working examples demonstrating Response Formatting across **MVC Controllers** and **Minimal APIs**.

---

## MVC Controller API {#mvc-controller-api}

A complete REST API controller using `ApiResults` for consistent responses.

```csharp
using AspNetConventions.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET /api/products
    [HttpGet]
    public ActionResult<CollectionResult<Product>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var (products, total) = _productService.GetPaged(page, pageSize);
        return ApiResults.Paginate(products, total, page, pageSize);
    }

    // GET /api/products/{id}
    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = _productService.GetById(id);
        if (product is null)
            return ApiResults.NotFound($"Product {id} not found.");

        return ApiResults.Ok(product);
    }

    // POST /api/products
    [HttpPost]
    public ActionResult<Product> Create([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
            return ApiResults.BadRequest(ModelState);

        var product = _productService.Create(request);
        return ApiResults.Created(product, "Product created successfully.");
    }

    // PUT /api/products/{id}
    [HttpPut("{id}")]
    public ActionResult<Product> Update(int id, [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
            return ApiResults.BadRequest(ModelState);

        var product = _productService.Update(id, request);
        if (product is null)
            return ApiResults.NotFound($"Product {id} not found.");

        return ApiResults.Ok(product, "Product updated successfully.");
    }

    // DELETE /api/products/{id}
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var deleted = _productService.Delete(id);
        if (!deleted)
            return ApiResults.NotFound($"Product {id} not found.");

        return ApiResults.NoContent("Product deleted.");
    }
}
```

### Response Examples {#response-examples}

::: tabs

== tab "GET /api/products (paginated):"

```json
{
  "status": "success",
  "statusCode": 200,
  "data": [
    { "id": 1, "name": "Widget A", "price": 29.99 },
    { "id": 2, "name": "Widget B", "price": 39.99 }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 25,
    "totalPages": 4,
    "totalRecords": 100,
    "links": {
      "firstPageUrl": "/api/products?page-number=1&page-size=25",
      "lastPageUrl": "/api/products?page-number=3&page-size=25",
      "nextPageUrl": "/api/products?page-number=2&page-size=25",
      "previousPageUrl": null
    }
  },
  "metadata": {
    "requestType": "GET",
    "timestamp": "0000-00-00T00:00:00.000Z",
    "traceId": "00-abc123...",
    "path": "/api/products"
  }
}
```

== tab "POST /api/products (created):"

```json
{
  "status": "success",
  "statusCode": 201,
  "message": "Product created successfully.",
  "data": {
    "id": 43,
    "name": "New Widget",
    "price": 49.99
  },
  "metadata": { ... }
}
```

== tab "POST /api/products (validation error):"

```json
{
  "status": "failure",
  "statusCode": 400,
  "type": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred.",
  "errors": {
    "Name": ["'Name' must not be empty."],
    "Price": ["'Price' must be greater than 0."]
  },
  "metadata": { ... }
}
```

:::

---

## Minimal API {#minimal-api}

A complete Minimal API setup with response formatting.

```csharp
using AspNetConventions;
using AspNetConventions.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var api = app.UseAspNetConventions();

// GET /api/users
api.MapGet("/api/users", (IUserService userService, int page = 1, int pageSize = 10) =>
{
    var (users, total) = userService.GetPaged(page, pageSize);
    return ApiResults.Paginate(users, total, page, pageSize);
});

// GET /api/users/{id}
api.MapGet("/api/users/{id}", (int id, IUserService userService) =>
{
    var user = userService.GetById(id);
    if (user is null)
        return ApiResults.NotFound($"User {id} not found.");

    return ApiResults.Ok(user);
});

// POST /api/users
app.MapPost("/api/users", (CreateUserRequest request, IUserService userService) =>
{
    var user = userService.Create(request);
    return ApiResults.Created(user, "User created successfully.");
});

// PUT /api/users/{id}
app.MapPut("/api/users/{id}", (int id, UpdateUserRequest request, IUserService userService) =>
{
    var user = userService.Update(id, request);
    if (user is null)
        return ApiResults.NotFound($"User {id} not found.");

    return ApiResults.Ok(user, "User updated.");
});

// DELETE /api/users/{id}
app.MapDelete("/api/users/{id}", (int id, IUserService userService) =>
{
    var deleted = userService.Delete(id);
    if (!deleted)
        return ApiResults.NotFound($"User {id} not found.");

    return ApiResults.NoContent();
});

app.Run();
```

---

## Error Handling Scenarios {#error-handling-scenarios}

### Validation Errors {#validation-errors}

```csharp
[HttpPost]
public ActionResult<User> CreateUser([FromBody] CreateUserRequest request)
{
    // ModelState validation
    if (!ModelState.IsValid)
        return ApiResults.BadRequest(ModelState);

    // Custom validation
    if (_userService.EmailExists(request.Email))
        return ApiResults.Conflict("A user with this email already exists.");

    var user = _userService.Create(request);
    return ApiResults.Created(user);
}
```

**Response (ModelState errors):**
```json
{
  "status": "failure",
  "statusCode": 400,
  "type": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred.",
  "errors": {
    "Email": ["'Email' is not a valid email address."],
    "Password": ["'Password' must be at least 8 characters."]
  },
  "metadata": { ... }
}
```

### Not Found {#not-found}

```csharp
[HttpGet("{id}")]
public ActionResult<Order> GetOrder(int id)
{
    var order = _orderService.GetById(id);
    if (order is null)
        return ApiResults.NotFound($"Order {id} not found.");

    return ApiResults.Ok(order);
}
```

**Response:**
```json
{
  "status": "failure",
  "statusCode": 404,
  "type": "CLIENT_ERROR",
  "message": "Order 123 not found.",
  "metadata": { ... }
}
```

### Conflict {#conflict}

```csharp
[HttpPost]
public ActionResult<Account> CreateAccount([FromBody] CreateAccountRequest request)
{
    if (_accountService.UsernameExists(request.Username))
        return ApiResults.Conflict(
            new { field = "username", value = request.Username },
            "Username is already taken.");

    var account = _accountService.Create(request);
    return ApiResults.Created(account);
}
```

**Response:**
```json
{
  "status": "failure",
  "statusCode": 409,
  "type": "CLIENT_ERROR",
  "message": "Username is already taken.",
  "data": {
    "field": "username",
    "value": "johndoe"
  },
  "metadata": { ... }
}
```

---

## Pagination Examples {#pagination-examples}

### Basic Pagination {#basic-pagination}

```csharp
[HttpGet]
public ActionResult<CollectionResult<Product>> GetProducts(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var (products, totalCount) = _productService.GetPaged(page, pageSize);
    return ApiResults.Paginate(products, totalCount, page, pageSize);
}
```

### Filtered Pagination {#filtered-pagination}

```csharp
[HttpGet]
public ActionResult<CollectionResult<Product>> SearchProducts(
    [FromQuery] string? category,
    [FromQuery] decimal? minPrice,
    [FromQuery] decimal? maxPrice,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var filter = new ProductFilter
    {
        Category = category,
        MinPrice = minPrice,
        MaxPrice = maxPrice
    };

    var (products, totalCount) = _productService.Search(filter, page, pageSize);
    return ApiResults.Paginate(products, totalCount, page, pageSize);
}
```

### Cursor-Based Pagination {#cursor-based-pagination}

For large datasets, use cursor-based pagination:

```csharp
[HttpGet]
public ActionResult GetProducts([FromQuery] string? cursor, [FromQuery] int limit = 20)
{
    var (products, nextCursor, hasMore) = _productService.GetWithCursor(cursor, limit);

    return ApiResults.Ok(new
    {
        items = products,
        nextCursor = hasMore ? nextCursor : null,
        hasMore
    });
}
```

---

## Integration with Exception Handling {#integration-with-exception-handling}

Combine with [exception mappers](../exception-handling/exception-mappers.md#creating-a-custom-mapper) for consistent error responses:

```csharp
// Custom exception
public class OrderNotFoundException : Exception
{
    public int OrderId { get; }
    public OrderNotFoundException(int orderId)
        : base($"Order {orderId} not found.")
    {
        OrderId = orderId;
    }
}

// Exception mapper
public class OrderNotFoundExceptionMapper : ExceptionMapper<OrderNotFoundException>
{
    public override ExceptionDescriptor Map(OrderNotFoundException exception)
    {
        return new ExceptionDescriptor(
            HttpStatusCode.NotFound,
            "ORDER_NOT_FOUND",
            exception.Message,
            new { orderId = exception.OrderId });
    }
}

// Controller throws exception
[HttpGet("{id}")]
public ActionResult<Order> GetOrder(int id)
{
    var order = _orderService.GetById(id)
        ?? throw new OrderNotFoundException(id);

    return ApiResults.Ok(order);
}
```

**Error response:**
```json
{
  "status": "failure",
  "statusCode": 404,
  "type": "ORDER_NOT_FOUND",
  "message": "Order 123 not found.",
  "data": { "orderId": 123 },
  "metadata": { ... }
}
```