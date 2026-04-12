# ApiResults

The `ApiResults` static class provides factory methods for creating strongly-typed [`ApiResult<T>`](./configuration.md#apiresultlttgt) instances with consistent HTTP status codes. It works seamlessly with both **MVC Controllers** and **Minimal APIs**.

---

## Standard Methods vs ApiResults {#standard-methods-vs-apiresults}

**AspNetConventions** works with both ASP.NET Core's built-in response methods—no changes required. However, the `ApiResults` helper class unlocks additional capabilities.

### What Works Out of the Box {#what-works-out-of-the-box}

| Framework | Standard Methods | AspNetConventions Support |
| --- | --- | --- |
| **MVC Controllers** | `Ok()`, `NotFound()`, `BadRequest()`, `CreatedAtAction()`, `Problem()`, etc. | <svg color="#56ce8a" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-circle-check-icon lucide-circle-check"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg> Automatically formats responses. |
| **Minimal APIs** | `Results.Ok()`, `Results.NotFound()`, `Results.BadRequest()`, `Results.Created()`, `Results.Problem()`, etc. | <svg color="#56ce8a" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-circle-check-icon lucide-circle-check"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg> Automatically formats responses. |

Standard methods return properly formatted responses with metadata, consistent structure, and your configured JSON conventions.

---

## Why ApiResults? {#why-apiresults}

Instead of manually constructing responses with status codes and messages, `ApiResults` provides:

- **Consistent status codes** — Each method maps to a specific HTTP status code
- **Type safety** — Generic methods ensure compile-time type checking
- **Implicit conversion** — [`ApiResult<T>`](./configuration.md#apiresultlttgt) converts automatically to `ActionResult` or `IResult`
- **Message support** — Optional messages for additional context
- **Pagination built-in** — `Paginate()` method for collection responses

```csharp
// Without ApiResults
return NotFound(new { error = "User not found." });

// With ApiResults
return ApiResults.NotFound("User not found.");
```
::: callout info ApiResults vs ApiResult
- [**`ApiResult`**](./configuration.md#apiresult) is the **object** that holds your response data—it's the container with properties like `StatusCode`, `Message`, `Data`, and `Metadata`. 
- **`ApiResults`** (notice the plural) is the **helper class** that creates these objects for you. Think of it this way: `ApiResults` is the factory, `ApiResult` is the product.
:::

---

## Usage {#usage}

### MVC Controllers {#mvc-controllers}

The [`ApiResult<T>`](./configuration.md#apiresultlttgt) class integrates directly with MVC controllers through **implicit conversions** to `ActionResult<T>` and `ActionResult`. This allows you to return strongly-typed, standardized responses without any additional boilerplate:

```csharp
using AspNetConventions.Http;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    // GET: /api/products/{id}
    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        var product = _productService.GetById(id);
        if (product is null)
        {
            return ApiResults.NotFound($"Product {id} not found.");
        }

        return ApiResults.Ok(product);
    }

    // POST: /api/products
    [HttpPost]
    public ActionResult<Product> CreateProduct([FromBody] CreateProductRequest request)
    {
        // Automatic ModelState validation handling
        if (!ModelState.IsValid)
        {
            return ApiResults.BadRequest(ModelState);
        }

        var product = _productService.Create(request);
        return ApiResults.Created(product, "Product created successfully.");
    }

    // DELETE: /api/products/{id}
    [HttpDelete("{id}")]
    public ActionResult DeleteProduct(int id)
    {
        var deleted = _productService.Delete(id);
        if (!deleted)
        {
            return ApiResults.NotFound($"Product {id} not found.");
        }

        return ApiResults.NoContent("Product deleted.");
    }
}
```

### Minimal APIs {#minimal-apis}

The `ApiResults` helper class is fully compatible with Minimal APIs, offering a consistent and expressive way to return standardized responses:

```csharp
using AspNetConventions.Http;

// GET endpoint with not found handling
app.MapGet("/api/products/{id}", (int id, IProductService productService) =>
{
    var product = productService.GetById(id);
    if (product is null)
    {
        return ApiResults.NotFound($"Product {id} not found.");
    }

    return ApiResults.Ok(product);
});

// POST endpoint with custom success message
app.MapPost("/api/products", (CreateProductRequest request, IProductService productService) =>
{
    var product = productService.Create(request);
    return ApiResults.Created(product, "Product created successfully.");
});

// PUT endpoint with conflict handling
app.MapPut("/api/products/{id}", (int id, UpdateProductRequest request, IProductService productService) =>
{
    if (!productService.Exists(id))
    {
        return ApiResults.NotFound($"Product {id} not found.");
    }
    
    if (productService.IsDuplicate(request.Name))
    {
        return ApiResults.Conflict(new { request.Name }, "Product name already exists.");
    }
    
    var updated = productService.Update(id, request);
    return ApiResults.Ok(updated, "Product updated successfully.");
});
```
#### Strongly-Typed Responses {#strongly-typed-responses}

When working with Minimal APIs, you can leverage .NET's `Results<T1, T2, T..>` union type for full type safety. Use the `.ToHttpResult()` method or the implicit conversion to `HttpApiResult<T>`:

```csharp
api.MapGet("/product/{id}", Results<HttpApiResult<UserDto>, NotFound> (int id, IUserService userService) =>
{
    var user = userService.GetById(id);

    return user == null
        ? ApiResults.Ok(user).ToHttpResult()
        : TypedResults.NotFound($"User {id} not found.");
});
```
---

## Success Methods {#success-methods}

### Ok {#ok}

Returns a `200 OK` response.

```csharp
// With value
ApiResults.Ok<TValue>(TValue? value, string? message = null)

// Message only
ApiResults.Ok(string message)
```

**Example:**
```csharp
return ApiResults.Ok(user);
return ApiResults.Ok(user, "User retrieved successfully.");
return ApiResults.Ok("Operation completed.");
```

### Created {#created}

Returns a `201 Created` response.

```csharp
// With value
ApiResults.Created<TValue>(TValue? value, string? message = null)

// Message only
ApiResults.Created(string message)
```

**Example:**
```csharp
return ApiResults.Created(newUser, "User created successfully.");
```

### Accepted {#accepted}

Returns a `202 Accepted` response for async operations.

```csharp
// With value
ApiResults.Accepted<TValue>(TValue? value, string? message = null)

// Message only
ApiResults.Accepted(string message)
```

**Example:**
```csharp
return ApiResults.Accepted(new { jobId = "abc-123" }, "Processing started.");
```

### NoContent {#nocontent}

Returns a `204 No Content` response.

```csharp
ApiResults.NoContent(string? message = null)
```

**Example:**
```csharp
return ApiResults.NoContent("Resource deleted.");
```

### PartialContent {#partialcontent}

Returns a `206 Partial Content` response.

```csharp
ApiResults.PartialContent<TValue>(TValue? value, string? message = null)
```

**Example:**
```csharp
return ApiResults.PartialContent(partialData, "Partial results returned.");
```

---

## Redirection Methods {#redirection-methods}

### MovedPermanently {#movedpermanently}

Returns a `301 Moved Permanently` response.

```csharp
ApiResults.MovedPermanently<TValue>(TValue? value, string? message = null)
```

### Found {#found}

Returns a `302 Found` response.

```csharp
ApiResults.Found<TValue>(TValue? value, string? message = null)
```

### NotModified {#notmodified}

Returns a `304 Not Modified` response.

```csharp
ApiResults.NotModified(string? message = null)
```

---

## Client Error Methods {#client-error-methods}

### BadRequest {#badrequest}

Returns a `400 Bad Request` response.

```csharp
// With ModelState (validation errors)
ApiResults.BadRequest(ModelStateDictionary modelState, string? message = null, string? type = null)

// With value
ApiResults.BadRequest<TValue>(TValue? value, string? message = null, string? type = null)

// Message only
ApiResults.BadRequest(string? message = null)
```

**Example:**
```csharp
// Validation errors from ModelState
if (!ModelState.IsValid){
    return ApiResults.BadRequest(ModelState);
}

// Complex error object (Manually)
return ApiResults.BadRequest(new ExceptionDescriptor
{
    Value = new
    {
        Value = 500,
        Unit = "C",
        LocalDate = DateTime.Now,
    },
    Type = "INVALID_WEATHER_INPUT",
    Message = "Invalid temperature value.",
    StatusCode = System.Net.HttpStatusCode.BadRequest
});

// Complex error object
return ApiResults.BadRequest(new
    {
        Value = 500,
        Unit = "C",
        LocalDate = DateTime.Now,
    },
    "Invalid temperature value.",
    "INVALID_WEATHER_INPUT")

// Simple message
return ApiResults.BadRequest("Invalid request parameters.");
```

### Unauthorized {#unauthorized}

Returns a `401 Unauthorized` response.

```csharp
ApiResults.Unauthorized(string? message = null)
```

**Example:**
```csharp
return ApiResults.Unauthorized("Authentication required.");
```

### Forbidden {#forbidden}

Returns a `403 Forbidden` response.

```csharp
ApiResults.Forbidden(string? message = null)
```

**Example:**
```csharp
return ApiResults.Forbidden("You don't have permission to access this resource.");
```

### NotFound {#notfound}

Returns a `404 Not Found` response.

```csharp
// Message only
ApiResults.NotFound(string? message = null)

// With value
ApiResults.NotFound<TValue>(TValue? value, string? message = null)
```

**Example:**
```csharp
return ApiResults.NotFound("User not found.");
return ApiResults.NotFound(new { resourceType = "User", id = 123 }, "Resource not found.");
```

### MethodNotAllowed {#methodnotallowed}

Returns a `405 Method Not Allowed` response.

```csharp
ApiResults.MethodNotAllowed(string? message = null)
```

### RequestTimeout {#requesttimeout}

Returns a `408 Request Timeout` response.

```csharp
ApiResults.RequestTimeout(string? message = null)
```

### Conflict {#conflict}

Returns a `409 Conflict` response.

```csharp
ApiResults.Conflict<TValue>(TValue? value, string? message = null)
ApiResults.Conflict(string? message = null)
```

**Example:**
```csharp
return ApiResults.Conflict("A user with this email already exists.");
```

### Gone {#gone}

Returns a `410 Gone` response.

```csharp
ApiResults.Gone(string? message = null)
```

### UnprocessableEntity {#unprocessableentity}

Returns a `422 Unprocessable Entity` response.

```csharp
ApiResults.UnprocessableEntity<TValue>(TValue? value, string? message = null)
ApiResults.UnprocessableEntity(string? message = null)
```

**Example:**
```csharp
return ApiResults.UnprocessableEntity("The request was valid but could not be processed.");
```

### TooManyRequests {#toomanyrequests}

Returns a `429 Too Many Requests` response.

```csharp
ApiResults.TooManyRequests(string? message = null)
```

**Example:**
```csharp
return ApiResults.TooManyRequests("Rate limit exceeded. Try again later.");
```

---

## Server Error Methods {#server-error-methods}

### InternalServerError {#internalservererror}

Returns a `500 Internal Server Error` response.

```csharp
ApiResults.InternalServerError<TValue>(TValue? value, string? message = null)
ApiResults.InternalServerError(string? message = null)
```

### NotImplemented {#notimplemented}

Returns a `501 Not Implemented` response.

```csharp
ApiResults.NotImplemented(string? message = null)
```

### BadGateway {#badgateway}

Returns a `502 Bad Gateway` response.

```csharp
ApiResults.BadGateway(string? message = null)
```

### ServiceUnavailable {#serviceunavailable}

Returns a `503 Service Unavailable` response.

```csharp
ApiResults.ServiceUnavailable(string? message = null)
```

### GatewayTimeout {#gatewaytimeout}

Returns a `504 Gateway Timeout` response.

```csharp
ApiResults.GatewayTimeout(string? message = null)
```

---

## Pagination Methods {#pagination-methods}

### Paginate {#paginate}

Returns a `200 OK` response with pagination metadata.

```csharp
// Basic pagination
ApiResults.Paginate<TValue>(
    IEnumerable<TValue> items,
    int totalRecords,
    string? message = null)

// With page parameters
ApiResults.Paginate<TValue>(
    IEnumerable<TValue> items,
    int totalRecords,
    int pageNumber,
    int pageSize,
    string? message = null)

// With custom status code
ApiResults.Paginate<TValue>(
    IEnumerable<TValue> items,
    int totalRecords,
    int pageNumber,
    int pageSize,
    HttpStatusCode statusCode,
    string? message = null)
```

**Example:**
```csharp
[HttpGet]
public ActionResult<CollectionResult<Product>> GetProducts(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    var (products, totalCount) = _productService.GetPaged(page, pageSize);

    return ApiResults.Paginate(
        products,
        totalRecords: totalCount,
        pageNumber: page,
        pageSize: pageSize);
}
```

**Response:**
```json
{
  "status": "success",
  "statusCode": 200,
  "data": [
    { "id": 1, "name": "Product A" },
    { "id": 2, "name": "Product B" }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalRecords": 50,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "links": {
    "first": "/api/products?page=1&pageSize=10",
    "last": "/api/products?page=5&pageSize=10",
    "next": "/api/products?page=2&pageSize=10",
    "prev": null
  }
}
```
See [Pagination Metadata](./metadata.md#pagination-metadata) for more information.

---

## Special Methods {#special-methods}

### Problem {#problem}

Returns a `ProblemDetails` response (RFC 7807).

```csharp
ApiResults.Problem(
    string? detail = null,
    string? instance = null,
    int? statusCode = null,
    string? title = null,
    string? type = null,
    IDictionary<string, object?>? extensions = null)
```

**Example:**
```csharp
return ApiResults.Problem(
    detail: "The user account is locked.",
    title: "Account Locked",
    statusCode: 423,
    type: "https://api.example.com/errors/account-locked",
    extensions: new Dictionary<string, object?>
    {
        ["retryAfter"] = 300,
        ["supportEmail"] = "support@example.com"
    });
```

### Custom {#custom}

Returns a response with a custom HTTP status code.

```csharp
// With value
ApiResults.Custom<TValue>(
    TValue? value,
    HttpStatusCode statusCode,
    string? message = null,
    string? type = null)

// Message only
ApiResults.Custom(
    HttpStatusCode statusCode,
    string? message = null,
    string? type = null)
```

**Example:**
```csharp
// Custom 418 I'm a teapot
return ApiResults.Custom(
    new { brewing = true },
    HttpStatusCode.ImATeapot,
    "I'm a teapot.",
    "TEAPOT");
```