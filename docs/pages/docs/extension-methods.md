# Extensions

Public extension methods and utilities included with **AspNetConventions** that you can use directly in your application.

---

## StringExtensions {#string-extensions}

**Namespace:** `AspNetConventions.Extensions`

Fluent string case-conversion methods for transforming identifiers between the naming conventions common in API development.

```csharp
using AspNetConventions.Extensions;
```

---

### `ToKebabCase()` {#to-kebab-case}

Converts a string to kebab-case (lowercase words separated by hyphens).

```csharp
"UserProfileId".ToKebabCase()   // "user-profile-id"
"firstName".ToKebabCase()       // "first-name"
"HTTPSRequest".ToKebabCase()    // "https-request"
```

---

### `ToSnakeCase()` {#to-snake-case}

Converts a string to snake_case (lowercase words separated by underscores).

```csharp
"UserProfileId".ToSnakeCase()   // "user_profile_id"
"firstName".ToSnakeCase()       // "first_name"
"HTTPSRequest".ToSnakeCase()    // "https_request"
```

---

### `ToCamelCase()` {#to-camel-case}

Converts a string to camelCase (first letter lowercase, subsequent words capitalized).

```csharp
"UserProfileId".ToCamelCase()   // "userProfileId"
"user-profile-id".ToCamelCase() // "userProfileId"
"user_profile_id".ToCamelCase() // "userProfileId"
```

---

### `ToPascalCase()` {#to-pascal-case}

Converts a string to PascalCase (first letter of each word capitalized, no separator).

```csharp
"user-profile-id".ToPascalCase() // "UserProfileId"
"user_profile_id".ToPascalCase() // "UserProfileId"
"userProfileId".ToPascalCase()   // "UserProfileId"
```

---

### Chaining

The methods can be chained to convert between formats:

```csharp
"UserProfileId"
    .ToKebabCase()   // "user-profile-id"
    .ToPascalCase()  // "UserProfileId"
    .ToCamelCase();  // "userProfileId"
```

---

## CaseTokenizer {#case-tokenizer}

**Namespace:** `AspNetConventions.Core.Converters`

Splits a string into word ranges based on casing and separator characters. Used internally by all built-in case converters and available for custom `ICaseConverter` implementations.

```csharp
using AspNetConventions.Core.Converters;
```

### `Tokenize(ReadOnlySpan<char> span, bool numberBoundaries = false)` {#tokenize}

Returns an `IReadOnlyList<WordRange>` where each `WordRange` holds a `Start` index and a `Length` into the original span.

Word boundaries are detected on:
- `_` and `-` separator characters
- An uppercase letter following a lowercase letter (`camelCase`, `PascalCase`)
- Letter/digit transitions when `numberBoundaries: true` (`Iso2` → `["Iso", "2"]`)

```csharp
var words = CaseTokenizer.Tokenize("UserProfileId".AsSpan());
// [{ Start=0, Length=4 }, { Start=4, Length=7 }, { Start=11, Length=2 }]
// → "User", "Profile", "Id"

var words = CaseTokenizer.Tokenize("Base64Encoder".AsSpan(), numberBoundaries: true);
// → "Base", "64", "Encoder"
```

### Building a custom converter

`CaseTokenizer` is the foundation for all built-in converters. Implementing `ICaseConverter` with it is straightforward:

```csharp
using AspNetConventions.Core.Abstractions.Contracts;
using AspNetConventions.Core.Converters;

public class DotCaseConverter : ICaseConverter
{
    public string Convert(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var words = CaseTokenizer.Tokenize(value.AsSpan());
        return string.Join(".", words.Select(w =>
            value.Substring(w.Start, w.Length).ToLowerInvariant()));
    }
}

// "UserProfileId" → "user.profile.id"
```

See [Custom Case Converter](./json-serialization/features.md#custom-case-converter) for a full integration example.
