# Examples

Complete working examples demonstrating JSON Serialization configuration.

---

## Library Response Types {#library-response-types}

**AspNetConventions** ships with a set of default response types (`ApiResponse`, `DefaultApiResponse`, `PaginationMetadata`, `PaginationLinks`) that already have sensible serialization defaults applied internally. The examples below show how you can extend or override those defaults using `ConfigureTypes`.

### Customizing ApiResponse {#customizing-apiresponse}

`ApiResponse` is the abstract base for all standard response envelopes. It has four properties: `Status`, `StatusCode`, `Message`, and `Metadata`.

**Scenario:** Suppress `StatusCode` from the response body (the HTTP status header is sufficient) and ensure `Message` is omitted when null:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<ApiResponse>(type =>
    {
        type.Property(x => x.StatusCode).Ignore();
        type.Property(x => x.Message).Ignore(JsonIgnoreCondition.WhenWritingNull);
    });
};
```

**Before (default):**
```json
{
  "status": "success",
  "statusCode": 200,
  "message": null,
  "data": { ... }
}
```

**After:**
```json
{
  "status": "success",
  "data": { ... }
}
```

---

### Customizing PaginationLinks {#customizing-paginationlinks}

`PaginationLinks` holds the navigation URLs for paginated responses. By default all four properties use their camelCase names. You can rename them to match a short, client-friendly contract:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<PaginationLinks>(type =>
    {
        type.Property(x => x.FirstPageUrl).Name("first");
        type.Property(x => x.LastPageUrl).Name("last");
        type.Property(x => x.NextPageUrl).Name("next");
        type.Property(x => x.PreviousPageUrl).Name("prev");
    });
};
```

**Serialized output:**
```json
{
  "pagination": {
    "pageNumber": 1,
    "pageSize": 25,
    "totalPages": 40,
    "totalRecords": 1000,
    "links": {
      "first": "/api/transactions?page-number=1&page-size=25",
      "last": "/api/transactions?page-number=40&page-size=25",
      "next": "/api/transactions?page-number=2&page-size=25",
      "prev": null
    }
  }
}
```

---

### Controlling Field Order in Responses {#controlling-field-order-in-responses}

Enforce a predictable property order in the standard response envelope:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<DefaultApiResponse>(type =>
    {
        type.Property(x => x.Status).Order(0);
        type.Property(x => x.StatusCode).Order(1);
        type.Property(x => x.Message).Order(2).Ignore(JsonIgnoreCondition.WhenWritingNull);
        type.Property(x => x.Data).Order(3);
        type.Property(x => x.Pagination).Order(4);
        type.Property(x => x.Metadata).Order(5);
    });
};
```

---

## User Domain {#user-domain}

### Hiding sensitive fields {#hiding-sensitive-fields}

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<User>(type =>
    {
        type.Property(x => x.Id).Order(0);
        type.Property(x => x.UserName).Order(1);
        type.Property(x => x.Email).Order(2);

        // Never include these in any response
        type.Property(x => x.PasswordHash).Ignore();
        type.Property(x => x.RefreshToken).Ignore();
        type.Property(x => x.TwoFactorSecret).Ignore();
    });
};
```

### Renaming fields for a public API {#renaming-fields-for-a-public-api}

Your internal model uses verbose property names; your public API contract uses compact names:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<ProductCatalogItem>(type =>
    {
        type.Property(x => x.InternalSkuCode).Name("sku");
        type.Property(x => x.DisplayName).Name("name");
        type.Property(x => x.RetailPriceExcludingTax).Name("price");
        type.Property(x => x.CostPriceExcludingTax).Ignore();
    });
};
```

**Result:**
```json
{ "sku": "WIDGET-001", "name": "Widget Pro", "price": 29.99 }
```

---

## Multiple Types {#multiple-types}

Configure several types at once inside a single `ConfigureTypes` delegate:

```csharp
options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<User>(type =>
    {
        type.Property(x => x.Id).Order(0);
        type.Property(x => x.UserName).Order(1);
        type.Property(x => x.PasswordHash).Ignore();
        type.Property(x => x.RefreshToken).Ignore();
    });

    cfg.Type<Order>(type =>
    {
        type.Property(x => x.OrderId).Name("id").Order(0);
        type.Property(x => x.InternalReference).Ignore();
        type.Property(x => x.Notes).Ignore(JsonIgnoreCondition.WhenWritingNull);
    });

    cfg.Type<ApiResponse>(type =>
    {
        type.Property(x => x.StatusCode).Ignore();
    });

    cfg.Type<PaginationLinks>(type =>
    {
        type.Property(x => x.FirstPageUrl).Name("first");
        type.Property(x => x.LastPageUrl).Name("last");
        type.Property(x => x.NextPageUrl).Name("next");
        type.Property(x => x.PreviousPageUrl).Name("prev");
    });
};
```

---

## Snake Case API {#snake-case-api}

A common real-world requirement: snake_case JSON with sensitive fields hidden and compact pagination link names.

```csharp
options.Json.CaseStyle = CasingStyle.SnakeCase;

options.Json.ConfigureTypes = cfg =>
{
    cfg.Type<User>(type =>
    {
        type.Property(x => x.PasswordHash).Ignore();
        type.Property(x => x.SecurityStamp).Ignore();
    });

    cfg.Type<PaginationLinks>(type =>
    {
        type.Property(x => x.FirstPageUrl).Name("first");
        type.Property(x => x.LastPageUrl).Name("last");
        type.Property(x => x.NextPageUrl).Name("next");
        type.Property(x => x.PreviousPageUrl).Name("prev");
    });
};
```

**Resulting response:**
```json
{
  "status": "success",
  "status_code": 200,
  "data": {
    "user_id": 42,
    "user_name": "jdoe",
    "email": "jdoe@example.com"
  },
  "pagination": {
    "page_number": 1,
    "page_size": 20,
    "total_pages": 5,
    "total_records": 100,
    "links": {
      "first": "/api/users?page_number=1&page_size=20",
      "last": "/api/users?page_number=5&page_size=20",
      "next": "/api/users?page_number=2&page_size=20",
      "prev": null
    }
  }
}
```
