# Response Formatting

**AspNetConventions** wraps all API responses — successes and errors — in a consistent JSON envelope across your entire application. You get uniformity out of the box, with full control to customize or replace the envelope shape entirely.

---

## Why Response Formatting?

REST APIs often return inconsistent response structures — some endpoints return raw data, others wrap it in envelopes, and error responses vary wildly. This inconsistency makes client-side parsing difficult and increases maintenance overhead.

**Without AspNetConventions:**
```json
// Endpoint A returns raw data
{ "id": 1, "name": "John" }

// Endpoint B wraps data
{ "success": true, "data": { "id": 1, "name": "John" } }

// Error responses vary
{ "error": "Not found" }
{ "message": "Validation failed", "errors": [...] }
```

**With AspNetConventions:**
```json
// All success responses
{
  "status": "success",
  "statusCode": 200,
  "data": { "id": 1, "name": "John" },
  "metadata": { ... }
}

// All error responses
{
  "status": "failure",
  "statusCode": 404,
  "type": "NOT_FOUND",
  "message": "Resource not found.",
  "metadata": { ... }
}
```

---

## Features

- **Automatic response wrapping** — All responses are wrapped in a consistent envelope without code changes
- **Unified error handling** — Error responses follow the same structure with type codes and messages
- **Request metadata** — Includes HTTP method, path, timestamp, and trace ID in every response
- **Pagination support** — Built-in pagination metadata and navigation links
- **Custom response builders** — Replace the envelope shape entirely with your own implementation
- **MVC Controllers & Minimal APIs** — Works seamlessly with both endpoint types
- **ApiResults helper** — Strongly-typed factory methods for creating consistent responses

---

## The Response Pipeline

When your application returns a response, **AspNetConventions** processes it through the following pipeline:

```
1. Controller action / Minimal API handler returns a value
           ↓
2. Response interceptor captures the result
           ↓
3. IResponseBuilder wraps the value in an envelope
           ↓
4. Metadata is attached (method, path, timestamp, traceId)
           ↓
5. JSON serialization and response sent to client
```

For **error responses**, the flow is slightly different:

```
1. Exception is thrown or error result returned
           ↓
2. Exception middleware captures the error
           ↓
3. ExceptionMapper creates an ExceptionDescriptor
           ↓
4. IErrorResponseBuilder wraps the error in an envelope
           ↓
5. JSON serialization and error response sent to client
```
---

## Default Response Format

**AspNetConventions** provides a consistent, predictable response format across all your API endpoints. This standardization eliminates the guesswork for API consumers and simplifies client-side integration.

### Real-World Banking Examples

The examples below demonstrate a banking API handling various scenarios.

::: tabs

== tab "Success"
**Scenario:** Creating a transaction (transfer)

```json
{
  "status": "success",
  "statusCode": 201,
  "message": "Transaction created successfully.",
  "data": {
    "transactionId": "BNK_566C_UT567990",
    "amount": 250.00,
    "currency": "USD",
    "fromAccount": "****4582",
    "toAccount": "****9174",
    "status": "pending",
    "createdAt": "0000-00-00T00:00:00.000Z"
  },
  "metadata": {
    "requestType": "POST",
    "timestamp": "0000-00-00T00:00:00.000Z",
    "traceId": "00-ed89d1cc507c35126d6f0e933984f774-99b8b9a3feb75652-00",
    "path": "/api/transactions"
  }
}
```

== tab "Paginated"
**Scenario:** Retrieving transaction history

```json
{
  "status": "success",
  "statusCode": 200,
  "message": null,
  "data": [
    {
      "transactionId": "BNK_566C_UT569230",
      "amount": 120.50,
      "currency": "USD",
      "fromAccount": "****4582",
      "toAccount": "****1203",
      "status": "completed",
      "processedAt": "0000-00-00T00:00:00.000Z"
    },
    {
      "transactionId": "BNK_566C_UT569231",
      "amount": 45.00,
      "currency": "USD",
      "fromAccount": "****4582",
      "toAccount": "****7741",
      "status": "completed",
      "processedAt": "0000-00-00T00:00:00.000Z"
    }
  ],
  "metadata": {
    "requestType": "GET",
    "timestamp": "0000-00-00T00:00:00.000Z",
    "traceId": "00-8e5513ae9369648487c2323d9a3508aa-2a8f92c7d45d3f74-00",
    "path": "/api/transactions"
  },
  "pagination": {
    "pageNumber": 1,
    "pageSize": 25,
    "totalPages": 40,
    "totalRecords": 1000,
    "links": {
      "firstPageUrl": "/api/transactions?page-number=1&page-size=25",
      "lastPageUrl": "/api/transactions?page-number=3&page-size=25",
      "nextPageUrl": "/api/transactions?page-number=2&page-size=25",
      "previousPageUrl": null
    }
  }
}
```

== tab "Validation Failure"
**Scenario:** Invalid transaction request

```json
{
  "status": "failure",
  "statusCode": 400,
  "type": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred.",
  "errors": [
    {
      "amount": [
        "Amount must be greater than zero."
      ]
    },
    {
      "toAccount": [
        "Destination account is required."
      ]
    }
  ],
  "metadata": {
    "requestType": "POST",
    "timestamp": "0000-00-00T00:00:00.000Z",
    "traceId": "00-8e5513ae9369648487c2323d9a3508aa-2a8f92c7d45d3f74-00",
    "path": "/api/transactions"
  }
}
```

== tab "Exception"
**Scenario:** Business rule failure (example: insufficient funds on production mode)

```json
{
  "status": "failure",
  "statusCode": 409,
  "type": "INSUFFICIENT_FUNDS",
  "message": "The transaction could not be completed due to insufficient funds.",
  "errors": [
    {
      "internal_code": "EX200045",
      "transaction_amount": 183.35
    }
  ],
  "metadata": {
    "requestType": "POST",
    "timestamp": "0000-00-00T00:00:00.000Z",
    "traceId": "00-c5b8cf60cc87100670434c546b7a2093-d1fbaf13a71b86ad-00",
    "path": "/api/transactions"
  }
}
```
:::

---

### Response Types

The `type` field in your error responses can be customized to reflect your business domain. By implementing a custom `ExceptionMapper<TException>`, you can return meaningful error codes that your API clients can handle programmatically.

**Example:** `"ORDER_NOT_FOUND"` tells the client exactly which resource is missing, rather than a generic `"NOT_FOUND"`.

::: callout info
The `type` field is only visible in **validation failure** and **exception** responses. See [`DefaultApiResponseBuilder`](/docs/response-formatting/custom-response-builders/#iresponsebuilder) and [`DefaultApiErrorResponseBuilder`](/docs/response-formatting/custom-response-builders/#ierrorresponsebuilder) for more information about response formats.
:::

To return custom `type` codes directly from your endpoints, use [`ApiResults.Custom()`](/docs/response-formatting/api-results/#custom)—it works for both **success** and **error** responses. For automatic exception handling with custom `type` codes, see [Custom Exception Mapping](/docs/exception-handling/exception-mappers/#creating-a-custom-mapper).

---

### Response Metadata

Every response automatically includes a `metadata` block that provides essential request context and observability information. This helps with debugging, logging, and correlating requests across distributed systems.

**Example metadata:**
```json
{
  "requestType": "POST",
  "timestamp": "0000-00-00T00:00:00.000000Z",
  "traceId": "00-ed89d1cc507c35126d6f0e933984f774-99b8b9a3feb75652-00",
  "path": "/api/users"
}
```

See [Response Metadata](/docs/response-formatting/metadata/#response-metadata) for more information.

---

### Pagination Metadata

When returning large result sets, **AspNetConventions** automatically includes pagination information that helps clients navigate through data efficiently. The `pagination` block provides current page details, total counts, and ready-to-use navigation URLs—eliminating the need for clients to manually construct pagination links.

**How it works:** The response builder detects the `CollectionResult<T>` result and:

1. Extracts pagination parameters (`pageNumber`, `pageSize`, `totalRecords`)
2. Calculates derived values (`totalPages`, Has Next Page, Has Previous Page)
3. Generates navigation links (`firstPageUrl`, `lastPageUrl`, `nextPageUrl`, `previousPageUrl`)
4. Attaches the `pagination` and `links` blocks to the response

**Example pagination metadata:**
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

See [Pagination Metadata](/docs/response-formatting/metadata/#pagination-metadata) for more information.

---

## ApiResults

While ASP.NET Core provides built-in response methods like `Ok()`, `BadRequest()`, and `Problem()` for MVC, and `Results.Ok()`, `Results.BadRequest()`, `Results.Created()` for Minimal APIs, **AspNetConventions** offers an enhanced alternative: The [`ApiResults`](/docs/response-formatting/api-results/) helper class.

[`ApiResults`](/docs/response-formatting/api-results/) builds on top of these standard methods, providing a unified factory for creating strongly-typed responses with additional capabilities:

| Feature | Standard ASP.NET Core | With ApiResults |
| --- | --- | --- |
| Custom messages | Manual implementation | `ApiResults.Created(user, "User created successfully")` |
| Pagination support | Manual implementation | `ApiResults.Paginate(users, totalRecords, pageNumber, pageSize)` |
| Strongly-typed responses | Limited | Full generic support (`ApiResult<TValue>`) |
| Custom status codes | Complex | `ApiResults.Custom(value, HttpStatusCode.Locked, "Resource locked")` |
| ModelState validation | Manual | `ApiResults.BadRequest(ModelState)` |
| Problem Details | Manual | `ApiResults.Problem(detail, title, statusCode)` |


**Example usage:**

```csharp
return ApiResults.Ok(users, "Users retrieved successfully");
return ApiResults.Created(newUser, "Account created successfully");
return ApiResults.Paginate(users, totalRecords, pageNumber, pageSize);
return ApiResults.BadRequest(ModelState);
```

See [ApiResults Reference](/docs/response-formatting/api-results) for complete documentation.
