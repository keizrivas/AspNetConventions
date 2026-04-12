# Metadata

**AspNetConventions** automatically enriches your API responses with contextual metadata and pagination information, providing better observability and a smoother client experience.

---

## Response Metadata {#response-metadata}

Every response includes a `metadata` block that provides essential request context and observability information. This helps with debugging, logging, and correlating requests across distributed systems.

### Metadata Fields {#metadata-fields}

| Field | Source | Description |
|-------|--------|-------------|
| `requestType` | `HttpContext.Request.Method` | HTTP method (`GET`, `POST`, `PUT`, `DELETE`, etc.) |
| `path` | `HttpContext.Request.Path` | Request URL path (e.g., `/api/users/123`) |
| `timestamp` | `DateTime.UtcNow` | **ISO 8601 UTC** timestamp of response generation |
| `traceId` | `Activity.Current?.Id` or `HttpContext.TraceIdentifier` | Unique identifier for end-to-end request tracing across services |

### Example Metadata Block {#example-metadata-block}

```json
{
  "requestType": "POST",
  "timestamp": "0000-00-00T00:00:00.000000Z",
  "traceId": "00-ed89d1cc507c35126d6f0e933984f774-99b8b9a3feb75652-00",
  "path": "/api/transactions"
}
```

### Configuration {#configuration}

You can disable metadata entirely or customize specific fields:

```csharp
options.Response.IncludeMetadata = false;
```
See [`ResponseFormattingOptions`](./configuration.md#responseformattingoptions) for more configuration options.

#### When Metadata is Omitted {#when-metadata-is-omitted}

When `IncludeMetadata = false`, the response will omit the entire `metadata` field:

```json
{
  "status": "success",
  "statusCode": 200,
  "data": { ... }
  // No "metadata" field
}
```

See [`DefaultApiResponseBuilder`](./custom-response-builders.md#iresponsebuilder) and [`DefaultApiErrorResponseBuilder`](./custom-response-builders.md#ierrorresponsebuilder) for more information about response formats.

### Observability Benefits {#observability-benefits}

The `traceId` is particularly valuable for:

*   **Correlating logs** across multiple services in a distributed system
    
*   **Tracing request flow** through message queues, databases, and external APIs
    
*   **Debugging production issues** by following a single request from entry to exit
    

---

## Pagination Metadata {#pagination-metadata}

When returning paginated results using [`ApiResults.Paginate()`](./api-results.md#pagination-methods) or [`CollectionResult<T>`](#collectionresult), the response includes a `pagination` block with navigation links and page information.

### PaginationMetadata {#paginationmetadata}

| Field | Source | Description |
| --- | --- | --- |
| `pageNumber` | `Math.Max(pageNumber, 1)` | Current page number (1-indexed, automatically normalized to at least 1) |
| `pageSize` | `pageSize` parameter | Number of items per page |
| `totalPages` | `Math.Ceiling(totalRecords / pageSize)` | Total number of pages available |
| `totalRecords` | `totalRecords` parameter | Total count of items across all pages |
| `links` | [`PaginationLinks`](#paginationlinks) object | Navigation URLs for pagination traversal |

### PaginationLinks {#paginationlinks}

The `links` object contains URIs for navigating between pages:

| Field | Description |
| --- | --- |
| `firstPageUrl` | URI to the first page of results (always available when pagination is enabled) |
| `lastPageUrl` | URI to the last page of results (always available when pagination is enabled) |
| `nextPageUrl` | URI to the next page, or null if current page is the last page |
| `previousPageUrl` | URI to the previous page, or null if current page is the first page |

### CollectionResult {#collectionresult}

The `CollectionResult<T>` is a wrapper class that combines a collection of items with pagination metadata. It's used internally by [`ApiResults.Paginate()`](./api-results.md#pagination-methods) to structure paginated responses.

| Property | Type | Description |
| --- | --- | --- |
| `Items` | `IEnumerable<T>` | The collection of items for the current page |
| `TotalRecords` | `int` | Total number of records available across all pages |
| `PageNumber` | `int` | Current page number (1-indexed) |
| `PageSize` | `int` | Number of items per page |

**When to use:** Typically you won't need to create `CollectionResult<T>` manually—[`ApiResults.Paginate()`](./api-results.md#pagination-methods) handles this for you. Use it directly when you need more control over the pagination structure or when integrating with existing pagination logic. See [Using Pagination in Your Endpoints](#using-pagination-in-your-endpoints).

### Example Pagination Block {#example-pagination-block}

```json
{
  "pageNumber": 1,
  "pageSize": 25,
  "totalPages": 20,
  "totalRecords": 500,
  "links": {
    "firstPageUrl": "/api/user/orders?page-number=1&page-size=25",
    "lastPageUrl": "/api/user/orders?page-number=3&page-size=25",
    "nextPageUrl": "/api/user/orders?page-number=2&page-size=25",
    "previousPageUrl": null
  }
}

```

### Configuration {#configuration}

Customize pagination behavior in your configuration:

```csharp
options.Response.Pagination.IncludeMetadata = true;
options.Response.Pagination.IncludeLinks = false;
options.Response.Pagination.PageNumberParameterName = "p";
options.Response.Pagination.PageSizeParameterName = "limit";
```

See [`PaginationOptions`](./configuration.md#paginationoptions) for more information.

#### When Pagination Metadata is Omitted {#when-pagination-metadata-is-omitted}

When `Pagination.IncludeMetadata = false`, the response will omit the entire `pagination` field:

```json
{
  "status": "success",
  "statusCode": 200,
  "data": { ... },
  "metadata": { ... },
  // No "pagination" field
}
```
#### When Pagination Links are Omitted {#when-pagination-links-are-omitted}

When `Pagination.IncludeLinks = false`, the response will omit the entire `pagination.links` fields:

```json
{
  "status": "success",
  "statusCode": 200,
  "data": { ... },
  "metadata": { ... },
  "pagination": {
    "pageNumber": 1,
    "pageSize": 25,
    "totalPages": 20,
    "totalRecords": 500,
    // No "links" field
  }
}
```


### Using Pagination in Your Endpoints {#using-pagination-in-your-endpoints}

Return paginated results from your controllers or minimal APIs. 
When your endpoint returns a [`ApiResults.Paginate()`](./api-results.md#pagination-methods) or [`CollectionResult<T>`](#collectionresult), **AspNetConventions** automatically adds pagination metadata:

**Example:**

```csharp
// using CollectionResult<T> (Manually)
var result = new CollectionResult<Product>(
    items: items,
    totalRecords: 100,
    pageNumber: 1,
    pageSize: 10);

return Ok(result);

// or using ApiResults.Paginate() (Recommended)

// Basic pagination
return ApiResults.Paginate(items, totalRecords, pageNumber, pageSize);
// With custom status code
return ApiResults.Paginate(items, totalRecords, pageNumber, pageSize, HttpStatusCode.OK);
// With custom message
return ApiResults.Paginate(items, totalRecords, pageNumber, pageSize, "Records retrieved successfully");
```

### How Link Generation Works {#how-link-generation-works}

The library automatically builds navigation links by:

1.  Preserving all existing query parameters (filters, sorting, search terms, etc.)
    
2.  Updating only the `pageNumber` and `pageSize` parameters
    
3.  Generating complete, absolute URLs based on the current request
    

**Example with preserved filters:**

Request URL:

```text
GET /api/transactions?category=electronics&sort=desc&page-number=2&page-size=20
```

Generated `nextPageUrl`:

```text
GET /api/transactions?category=electronics&sort=desc&page-number=3&page-size=20
```

---

## Summary {#summary}

| Feature | Purpose | Key Fields |
| --- | --- | --- |
| Response Metadata | Request observability and tracing | traceId, timestamp, requestType, path |
| Pagination Metadata | Navigation through large result sets | pageNumber, totalPages, links |

Both features work together to create APIs that are:

*   **Observable** — Every response carries tracing information
    
*   **Discoverable** — Clients can navigate pages without URL construction
    
*   **Consistent** — Same structure across all endpoints