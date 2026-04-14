# Basic Usage

**AspNetConventions** is designed to be zero-effort implementation. This guide explains the core workflow from initialization to final production build, ensuring your application enforces consistent API conventions across your entire ASP.NET Core ecosystem.

## MVC Controllers {#mvc-controllers}

When using MVC Controllers, the library integrates [`.AddAspNetConventions()`](./index.md#addaspnetconventions) with the `IMvcBuilder` to inject global action filters and route token transformers. This ensures that every controller and action follows your specified naming conventions without requiring manual `[Route]` overrides on every method.

### Your Code {#your-code}

```csharp
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        [HttpGet("[action]/{UserId:int}")]
        public ActionResult GetUser(int UserId) => Ok(new { id = UserId });
    
        [HttpPut("CreateAccount")]
        public ActionResult CreateUser([FromBody] CreateAccountRequest request) 
        {
            // Use ApiResults class to return a "201 Created" status code with custom message.
            return ApiResults.Created(request, "User created successfully.");
        }
    }
```

See [`ApiResults`](../response-formatting/api-results.md) for more information about custom standardized responses.

### Route Transformation {#route-transformation}

The following table demonstrates how the library automatically transforms standard PascalCase names into clean, SEO-friendly paths.

| Original | Standardized |
| --- | --- |
| `GET /api/Profile/GetUser/{UserId:int}` | `GET /api/profile/get-user/{user-id:int}` |
| `POST /api/Profile/CreateAccount` | `POST /api/profile/create-account` |

### Customizing the Route Style {#customizing-the-route-style}

If you prefer a different naming convention, such as `snake_case` or remove prefixes/suffixes on controller/action names, you can define it globally during service registration. 

```csharp
builder.Services
    .AddControllers()
    .AddAspNetConventions(options =>
    {
        // Result: GET /api/profile/user/{user_id:int}
        options.Route.CaseStyle = CasingStyle.SnakeCase;
        options.Route.Controllers.RemoveActionPrefixes.Add("Get");
    });
```
See [`RouteConventionOptions`](../route-standardization/configuration.md#routeconventionoptions) and [`ControllerRouteOptions`](../route-standardization/configuration.md#controllerrouteoptions) for more information about MVC Controllers route customization.

---

## Minimal APIs {#minimal-apis}

For Minimal APIs, the library provides a [`.UseAspNetConventions()`](./index.md#useaspnetconventions) extension for `WebApplication` that returns a `RouteGroupBuilder`. Map your endpoints on the returned group to apply conventions to a specific branch of your API tree or the entire application.

### Your Code {#your-code}

```csharp
var api = app.UseAspNetConventions();

api.MapGet("/WeatherForecast/{city}", (string city) =>
    Results.Ok(new { city }));

api.MapGet("/GetUserById/{userId}", (int userId) =>
    Results.Ok(new { id = userId }));
```

### Route Transformation {#route-transformation}

In Minimal APIs, route segments are transformed, but parameters are kept stable by default to ensure binding remains intact.

| Original | Standardized |
| --- | --- |
| `GET /WeatherForecast/{city}` | `GET /weather-forecast/{city}` |
| `GET /GetUserById/{userId}` | `GET /get-user-by-id/{userId}` |

::: callout warning <u>Important:</u> Parameter Binding in Minimal APIs 
Minimal APIs handle parameter binding strictly by name. If the library automatically transforms a route parameter like `{userId}` to `{user-id}`, the underlying .NET binder will fail to locate the value unless explicitly told where to look.

**Enabling Parameter Transformation** 
To enable automatic casing for Minimal API route parameters, toggle this feature in your configuration:

```csharp
app.UseAspNetConventions(options => {
    // Default is "false" to prevent binding breaks
    options.Route.MinimalApi.TransformRouteParameters = true;
});
```

**The Solution: Explicit Binding** 
When transformation is enabled, you must use the `[FromRoute(Name = "...")]` attribute to manually map the transformed segment name back to your C# parameter:

```csharp
// The URL becomes: /user-account/{user-id}
api.MapGet("/UserAccount/{userId}", ([FromRoute(Name = "user-id")] int userId) => {
    // Parameter binding works
    return Results.Ok(new { userId });
});
```

:::

See [`MinimalApiRouteOptions`](../route-standardization/configuration.md#minimalapirouteoptions) for more information about Minimal APIs route customization.

---

## Razor Pages {#razor-pages}

Razor Pages often suffer from inconsistent URL structures due to folder nesting. AspNetConventions automatically transforms physical file paths and route parameters into a consistent casing style, making your UI routes match your API routes.

### Your Code {#your-code}

The transformation applies to the page path and any parameters defined in `OnGet` / `OnPost` method signatures or `@page` directives. Additionally, it can transform properties marked with `[BindProperty]`.

```csharp
// Pages/UserProfile/EditAddress.cshtml.cs
public class EditAddressModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ZipCode { get; set; }

    public void OnGet(int UserId, int AddressId) { }
}
```

### Route Transformation {#route-transformation}

| State | Route Example |
| --- | --- |
| Original | `GET /UserProfile/EditAddress/{UserId}/{AddressId}?ZipCode=...` |
| Standardized | `GET /user-profile/edit-address/{user-id}/{address-id}?zip-code=...` |

### Disabling Property Transformation {#disabling-property-transformation}

If you need to keep property names in their original C# casing (e.g., for compatibility with external integrations), you can disable property-level transformation.

```csharp
builder.Services
    .AddRazorPages()
    .AddAspNetConventions(e =>
        e.Route.RazorPages.TransformPropertyNames = false
    );
```

See [`RazorPagesRouteOptions`](../route-standardization/configuration.md#razorpagesrouteoptions) for more information about razor page route customization.

---

## Response Standardization Examples {#response-standardization-examples}

A core feature of the library is the **Uniform Response Envelope**. Whether an endpoint succeeds or fails, the client receives a predictable JSON structure containing the data, status information, and useful debugging metadata.

### Success Response {#success-response}

Successful requests are wrapped in a `success` status with a `2xx` code and a `metadata` object for request tracking.

```json
{
    "status": "success",
    "statusCode": 201,
    "message": "User created successfully.",
    "data": {
        "userId": 1,
        "name": "John Doe"
    },
    "metadata": {
        "requestType": "PUT",
        "timestamp": "0000-00-00T00:00:00.000000Z",
        "traceId": "00-ed89d1cc507c35126d6f0e933984f774-99b8b9a3feb75652-00",
        "path": "/api/users/{id}"
    }
}
```

### Validation Error Response {#validation-error-response}

Errors are categorized by `type` (e.g., `VALIDATION_ERROR`) and include a detailed `errors` array that maps specific fields to their failure reasons.

```json
{
    "status": "failure",
    "statusCode": 400,
    "type": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred.",
    "errors": [
        {
            "Email": ["'Email' is not a valid email address."]
        }
    ],
    "metadata": {
        "requestType": "PUT",
        "timestamp": "0000-00-00T00:00:00.000000Z",
        "traceId": "00-8e5513ae9369648487c2323d9a3508aa-2a8f92c7d45d3f74-00",
        "path": "/api/users/{id}"
    }
}
```

#### Global Response Configuration {#global-response-configuration}

You can control the verbosity of your responses globally, such as hiding metadata in production or changing default messages.

```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        // Change Json serialization to "snake_case"
        options.Json.CaseStyle = CasingStyle.SnakeCase;

        // Hide metadata for cleaner responses
        options.Response.IncludeMetadata = false;
        options.Response.Pagination.DefaultPageSize = 50;
        options.Response.ErrorResponse.DefaultValidationMessage = "Check your input.";
    });
```
See [`ResponseFormattingOptions`](../response-formatting/configuration.md#responseformattingoptions) and [`JsonSerializationOptions`](../json-serialization/configuration.md) for more information about response and json serialization customization.

## Next Steps {#next-steps}

Explore the deep-dive documentation for each specific feature to customize the behavior to your needs:

[<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-link-icon lucide-link"><path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/></svg> **Route Standardization**](../route-standardization/index.md){.text-color .link style="min-width: 300px;"}

[<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-shield-alert-icon lucide-shield-alert"><path d="M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z"/><path d="M12 8v4"/><path d="M12 16h.01"/></svg> **Exception Handling**](../exception-handling/index.md){.text-color .link style="min-width: 300px;"}

[<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-file-braces-corner-icon lucide-file-braces-corner"><path d="M14 22h4a2 2 0 0 0 2-2V8a2.4 2.4 0 0 0-.706-1.706l-3.588-3.588A2.4 2.4 0 0 0 14 2H6a2 2 0 0 0-2 2v6"/><path d="M14 2v5a1 1 0 0 0 1 1h5"/><path d="M5 14a1 1 0 0 0-1 1v2a1 1 0 0 1-1 1 1 1 0 0 1 1 1v2a1 1 0 0 0 1 1"/><path d="M9 22a1 1 0 0 0 1-1v-2a1 1 0 0 1 1-1 1 1 0 0 1-1-1v-2a1 1 0 0 0-1-1"/></svg> **Response Formatting**](../response-formatting/index.md){.text-color .link style="min-width: 300px;"}

[<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-braces-icon lucide-braces"><path d="M8 3H7a2 2 0 0 0-2 2v5a2 2 0 0 1-2 2 2 2 0 0 1 2 2v5c0 1.1.9 2 2 2h1"/><path d="M16 21h1a2 2 0 0 0 2-2v-5c0-1.1.9-2 2-2a2 2 0 0 1-2-2V5a2 2 0 0 0-2-2h-1"/></svg> **JSON Serialization**](../json-serialization/index.md){.text-color .link style="min-width: 300px;"}



