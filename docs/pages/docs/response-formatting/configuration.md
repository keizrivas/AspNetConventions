# Configuration

Complete reference for all Response Formatting configuration options.

---

## ResponseFormattingOptions {#responseformattingoptions}

**Namespace:** `AspNetConventions.Configuration.Options`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.Response`{.code-right}

Controls how successful and error responses are wrapped and formatted.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables response formatting. When `false`, responses pass through unmodified |
| `IncludeMetadata` | `bool` | `true` | When `true`, responses include a `metadata` block with HTTP method, path, timestamp, and trace ID |
| `Pagination` | [`PaginationOptions`](#paginationoptions) | `new()` | Controls how paginated/collection responses are formatted |
| `ErrorResponse` | [`ErrorResponseOptions`](#errorresponseoptions) | `new()` | Controls error-specific response formatting behaviour |
| `CollectionResultAdapters` | `HashSet<ICollectionResultAdapter>` | `[]` | Custom adapters for resolving and formatting specific collection types in paginated responses |
| `ResponseBuilder` | [`IResponseBuilder`{.code-left}](./custom-response-builders.md#iresponsebuilder)`?`{.code-right} | `null` | Custom builder for successful responses. Falls back to `DefaultApiResponseBuilder` when `null` |
| `ErrorResponseBuilder` | [`IErrorResponseBuilder`{.code-left}](./custom-response-builders.md#ierrorresponsebuilder)`?`{.code-right} | `null` | Custom builder for error responses. Falls back to `DefaultApiErrorResponseBuilder` when `null` |
| `Hooks` | [`ResponseFormattingHooks`](#responseformattinghooks) | `new()` | Hooks for intercepting the response formatting pipeline |

### Disabling Response Formatting {#disabling-response-formatting}

```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        // All responses pass through as-is — no envelope wrapping
        options.Response.IsEnabled = false;
    });
```

### Disabling Metadata {#disabling-metadata}

```csharp
builder.Services.AddControllers()
    .AddAspNetConventions(options =>
    {
        // Responses will not include the "metadata" block
        options.Response.IncludeMetadata = false;

        // Responses will not include the "pagination" block
        options.Response.Pagination.IncludeMetadata = false;
    });
```

---

## PaginationOptions {#paginationoptions}

**Namespace:** `AspNetConventions.Configuration.Options.Response`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Response`{.code-left .code-right}](#responseformattingoptions)`.Pagination`{.code-right}

Controls how paginated and collection responses are formatted.

| Property | Type | Default | Description |
|---|---|---|---|
| `IncludeMetadata` | `bool` | `true` | Include pagination metadata (`currentPage`, `pageSize`, `totalPages`, etc.) in responses |
| `IncludeLinks` | `bool` | `true` | Include pagination links (`first`, `last`, `next`, `prev`) in responses |
| `IncludeNavigationFlags` | `bool` | `false` | Include `hasNextPage` and `hasPreviousPage` boolean flags in pagination metadata |
| `PageNumberParameterName` | `string` | `"page"` | Query parameter name for page number in generated links |
| `PageSizeParameterName` | `string` | `"pageSize"` | Query parameter name for page size in generated links |
| `DefaultPageSize` | `int` | `25` | Default page size when not specified in the request |

**Example:**
```csharp
options.Response.Pagination.IncludeLinks = true;
options.Response.Pagination.IncludeNavigationFlags = true;
options.Response.Pagination.PageNumberParameterName = "p";
options.Response.Pagination.PageSizeParameterName = "size";
options.Response.Pagination.DefaultPageSize = 50;
```

---

## ErrorResponseOptions {#errorresponseoptions}

**Namespace:** `AspNetConventions.Configuration.Options.Response`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Response`{.code-left .code-right}](#responseformattingoptions)`.ErrorResponse`{.code-right}

Controls error-specific response formatting behaviour.

| Property | Type | Default | Description |
|---|---|---|---|
| `DefaultStatusCode` | `HttpStatusCode` | `InternalServerError` | Default HTTP status code for unhandled exceptions |
| `DefaultErrorType` | `string` | `"UNEXPECTED_ERROR"` | Default type label for unhandled exceptions |
| `DefaultErrorMessage` | `string` | `"An unexpected error occurred."` | Default message for unhandled exceptions |
| `DefaultValidationType` | `string` | `"VALIDATION_ERROR"` | Default type label for validation exceptions |
| `DefaultValidationMessage` | `string` | `"One or more validation errors occurred."` | Default message for validation exceptions |
| `AllowedProblemDetailsExtensions` | `HashSet<string>` | `[]` | ProblemDetails extension keys allowed in error responses |
| `IncludeExceptionDetails` | `bool?` | `null` | Include exception details in responses. `null` = auto (Development environment only) |
| `MaxStackTraceDepth` | `int` | `10` | Maximum depth for nested stack trace when exception details are included |

**Example:**
```csharp
options.Response.ErrorResponse.DefaultErrorType = "INTERNAL_ERROR";
options.Response.ErrorResponse.DefaultErrorMessage = "Something went wrong.";
options.Response.ErrorResponse.DefaultValidationType = "INVALID_INPUT";
options.Response.ErrorResponse.DefaultValidationMessage = "Please check your input.";

// Include exception details (not recommended for production)
options.Response.ErrorResponse.IncludeExceptionDetails = true;
options.Response.ErrorResponse.MaxStackTraceDepth = 20;

// Allow specific ProblemDetails extensions
options.Response.ErrorResponse.AllowedProblemDetailsExtensions.Add("correlationId");
options.Response.ErrorResponse.AllowedProblemDetailsExtensions.Add("helpUrl");
```

### Exception Details Behavior {#exception-details-behavior}

The `IncludeExceptionDetails` property controls whether stack traces and inner exception information are included:

| Value | Behavior |
|-------|----------|
| `null` (default) | Auto-detect: included in **Development**, excluded in **Production** |
| `true` | Always include exception details |
| `false` | Never include exception details |

::: callout warning Security Note
Never enable `IncludeExceptionDetails = true` in production environments. Stack traces can expose sensitive information about your application's internals.
:::

---

## ResponseFormattingHooks {#responseformattinghooks}

**Namespace:** `AspNetConventions.Core.Hooks`
**Accessed via:** [`options`{.code-left}](../configuration-reference.md#aspnetconventionoptions)`.`{.code-left .code-right}[`Response`{.code-left .code-right}](#responseformattingoptions)`.Hooks`{.code-right}

Hooks provide fine-grained control over the response formatting pipeline. All hooks are asynchronous.

| Property | Delegate Signature | Description |
|---|---|---|
| `ShouldWrapResponseAsync` | `(`{.code-left}[`ApiResult`{.code-left .code-right}](#apiresult)`, `{.code-left .code-right}[`RequestDescriptor`{.code-left .code-right}](#requestdescriptor)`) → Task<bool>`{.code-right} | Return `false` to skip wrapping for a specific response |
| `BeforeResponseWrapAsync` | `(`{.code-left}[`ApiResult`{.code-left .code-right}](#apiresult)`, `{.code-left .code-right}[`RequestDescriptor`{.code-left .code-right}](#requestdescriptor)`) → Task`{.code-right} | Called before a response is wrapped. Use for logging or pre-processing |
| `AfterResponseWrapAsync` | `(object?, `{.code-left}[`ApiResult`{.code-left .code-right}](#apiresult)`, `{.code-left .code-right}[`RequestDescriptor`{.code-left .code-right}](#requestdescriptor)`) → Task`{.code-right} | Called after a response is wrapped. Receives the wrapped response object |
| `CustomizeMetadata` | `(`{.code-left}[`Metadata`](./metadata.md#metadata-fields)`, `{.code-left .code-right}[`RequestDescriptor`{.code-left .code-right}](#requestdescriptor)`) → void`{.code-right} | Called after standard metadata fields are populated. Add, remove, or replace entries before the response is built |



**Examples - Skip wrapping for specific routes and logging response format:**

```csharp
options.Response.Hooks.ShouldWrapResponseAsync = async (apiResult, request) =>
{
    // Skip wrapping for health checks
    if (request.Path.StartsWith("/health"))
        return false;

    // Skip wrapping for metrics endpoints
    if (request.Path.StartsWith("/metrics"))
        return false;

    return true;
};

options.Response.Hooks.AfterResponseWrapAsync = async (wrappedResponse, apiResult, request) =>
{
    _logger.LogInformation(
        "Response: {Method} {Path} → {StatusCode}",
        request.Method,
        request.Path,
        (int)apiResult.StatusCode);
};

options.Response.Hooks.BeforeResponseWrapAsync = async (apiResult, request) =>
{
    // Add custom processing before wrapping
    if (!apiResult.IsSuccess)
    {
        // Log error responses
        _logger.LogWarning(
            "Error response: {Type} - {Message}",
            apiResult.Type,
            apiResult.Message);
    }
};

options.Response.Hooks.CustomizeMetadata = (metadata, request) =>
{
    // Add custom entries
    metadata["userId"] = request.UserId;

    // Remove a standard entry
    metadata.Remove(Metadata.PathKey);
};
```

### ApiResult {#apiresult}
Encapsulates the standard execution result structure for an HTTP request. This is the abstract base class for all API responses.

| Property | Type | Description |
| --- | --- | --- |
| `StatusCode` | `HttpStatusCode` | The HTTP status code of the response (e.g., `OK`, `BadRequest`). |
| `Type` | `string` | Categorizes the response type. Auto-calculated from `StatusCode` (e.g., `"SUCCESS"`, `"CLIENT_ERROR"`, `"SERVER_ERROR"`). |
| `Message` | `string?` | An optional, human-readable response message. |
| `Payload` | `object?` | The original, untransformed content of the response. Useful for debugging or audit trails. |
| `Metadata` | `Metadata?` | Contains request context information like timing and trace identifiers. |
| `Pagination` | `PaginationMetadata?` | Provides pagination details (e.g., page number, total count) when the result is a paginated list. |
| `IsSuccess` | `bool` | Indicates if the request was successful. Returns `true` if the `StatusCode` is less than **400** (covers `1xx`, `2xx`, and `3xx` responses). |

| Method | Return Type | Description |
| --- | --- | --- |
| `GetValue()` | `object?` | Retrieves the main result value. For `ApiResult<T>`, returns strongly-typed value |


::: callout info Note on GetValue() vs Payload:

- `GetValue()` returns the primary business data of the response.

- `Payload` holds the raw, original source data before any processing or transformation by the library.

:::

### ApiResult&lt;T&gt; {#apiresult-t}

The generic version adds type safety and is the recommended way to return responses in your controllers.

```csharp
public sealed class ApiResult<TValue> : ApiResult
```

| Method | Return Type | Description |
| --- | --- | --- |
| `GetValue()` | `object?` | Retrieves the main result value. For `ApiResult<T>`, returns strongly-typed value |
| `ToHttpResult()` | `HttpApiResult<T>` | Converts to Minimal API result type |

**Implicit Conversions**

`ApiResult<T>` can be implicitly converted to:

- `ActionResult<T>` — For MVC controller return types
- `ActionResult` — For non-generic action returns
- `HttpApiResult<T>` — For Minimal API endpoints

### RequestDescriptor {#requestdescriptor}
Provides a comprehensive snapshot of the current HTTP request and its associated metadata within the application context. This class is useful for logging, error handling, and response formatting.

| Property | Type | Description |
| --- | --- | --- |
| `HttpContext` | `HttpContext` | The complete HTTP context containing all request and response information. |
| `Path` | `string?` | The request path relative to the application (excludes query string and domain). |
| `PathBase` | `string?` | The base path for the application. |
| `Method` | `string?` | The HTTP verb/method of the request (e.g., `GET`, `POST`, etc.). |
| `StatusCode` | `HttpStatusCode` | The HTTP status code for the response. Initially set from `HttpContext.Response.StatusCode`, but can be modified via `SetStatusCode()`. |
| `StatusCodeType` | `HttpStatusCodeType` | The categorical classification of the status code (e.g., `Informational`, `Success`, `Redirection`, `ClientError`, `ServerError`). |
| `TraceId` | `string?` | A unique identifier for correlating the request across distributed systems. Prioritizes `Activity.Current?.Id`, falling back to `HttpContext.TraceIdentifier`. |
| `EndpointType` | `EndpointType` | Identifies the type of endpoint handling the request (e.g., `"MvcAction"`, `"RazorPage"`, `"MinimalApi"`). |
| `UserId` | `string?` | The authenticated user's identifier (from `User.Identity.Name`), or null if no user is authenticated. |
| `IsDevelopment` | `bool` | Indicates whether the application is running in a Development environment (based on `IHostEnvironment`). |
| `Timestamp` | `DateTime` | The UTC timestamp when the request descriptor was created (typically when request processing began). |

---

## Default Values Reference {#default-values-reference}

| Option | Default |
|---|---|
| `Response.IsEnabled` | `true` |
| `Response.IncludeMetadata` | `true` |
| `Pagination.IncludeMetadata` | `true` |
| `Pagination.IncludeLinks` | `true` |
| `Pagination.IncludeNavigationFlags` | `false` |
| `Pagination.PageNumberParameterName` | `"page"` |
| `Pagination.PageSizeParameterName` | `"pageSize"` |
| `Pagination.DefaultPageSize` | `25` |
| `ErrorResponse.DefaultStatusCode` | `InternalServerError` (500) |
| `ErrorResponse.DefaultErrorType` | `"UNEXPECTED_ERROR"` |
| `ErrorResponse.DefaultErrorMessage` | `"An unexpected error occurred."` |
| `ErrorResponse.DefaultValidationType` | `"VALIDATION_ERROR"` |
| `ErrorResponse.DefaultValidationMessage` | `"One or more validation errors occurred."` |
| `ErrorResponse.IncludeExceptionDetails` | `null` (auto) |
| `ErrorResponse.MaxStackTraceDepth` | `10` |
