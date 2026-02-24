# AspNetConventions

[![License](https://img.shields.io/github/license/keizrivas/AspNetConventions)](https://github.com/keizrivas/AspNetConventions/blob/main/LICENSE)
[![Build Status](https://github.com/keizrivas/AspNetConventions/actions/workflows/build.yml/badge.svg)](https://github.com/keizrivas/AspNetConventions/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/AspNetConventions.svg)](https://www.nuget.org/packages/AspNetConventions)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AspNetConventions.svg)](https://www.nuget.org/packages/AspNetConventions)
[![.NET](https://img.shields.io/badge/.NET-8.0+-512BD4.svg)](https://dotnet.microsoft.com/download)

<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/keizrivas/AspNetConventions/main/assets/asp_net_conventions.svg" width="250">
    <img alt="AspNetConventions" src="https://raw.githubusercontent.com/keizrivas/AspNetConventions/main/assets/asp_net_conventions_light.svg" width="250">
  </picture>
</p>

**Convention-driven standardization for ASP.NET Core** - Automatically applies consistent standardization across ASP.NET Core applications — including APIs, MVC, and Razor Pages — with minimal configuration and zero boilerplate.

<p align="center">
    <picture>
        <source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/keizrivas/AspNetConventions/main/assets/banner.png" width="100%">
        <img alt="AspNetConventions" src="https://raw.githubusercontent.com/keizrivas/AspNetConventions/main/assets/banner_light.png" width="100%">
  </picture>
</p>

## Why AspNetConventions?

We believe API building should be intuitive. AspNetConventions transforms standard ASP.NET setups into a modern API solution by applying smart global behaviors automatically:

- **Universal Endpoint Support** - Consistent URL structure across MVC, Minimal APIs, and Razor Pages.
- **Automatic Route standardization** - Route and parameter names are transformed and bound automatically follow your preferred casing style.
- **Standardized responses** - Uniform JSON formatting and consistent response formatting application-wide.
- **Global exception handling** - Centralized error handling and formatting throughout your codebase.
- **Fully Extensible** - Supports custom converters, mappers, hooks, and response formatters, as well as third-party library compatibility.
- **Zero Runtime Overhead** - Conventions are applied during application startup, no performance impact on requests.

## Quick Start

### Installation

```bash
dotnet add package AspNetConventions
```

### Basic Configuration

```csharp
// Program.cs

var builder = WebApplication.CreateBuilder(args);

// Apply conventions to MVC Controllers/Razor Pages
builder.Services
  .AddControllersWithViews()
  .AddAspNetConventions();

var app = builder.Build();

// Apply conventions to Minimal APIs
var api = app.UseAspNetConventions("/");

api.MapGet("/GetUser/{UserId}", (int UserId) => new { userId = UserId });

app.Run();
```

**That's it!** Your entire application now follows consistent conventions.


## Route standardization Examples

### MVC Controllers

**Your Code:**
```csharp
[ApiController]
[Route("Api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("[Action]/{id}")]
    public ActionResult PublicProfile(int id) 
    {
        return Ok(new { UserId = id, Name = "John Doe" });
    }
}
```

**Standardized Route:**
```
GET /api/users/public-profile/{id}
```

---

### Minimal APIs

**Your Code:**
```csharp
var api = app.UseAspNetConventions("/Api");

api.MapGet("/WeatherForecasts/City/{name}", (string name) => 
    Results.Ok(new { CityName = name, Temperature = 72 }));
```

**Standardized Route:**
```
GET /api/weather-forecasts/city/{name}
```

---

### Razor Pages

**Your Code:**
```csharp
// Pages/UserProfile/Edit.cshtml.cs
public class EditModel : PageModel
{
    public void OnGet(int UserId) 
    {
        // Your logic here
    }
}
```

**Standardized Route:**
```
GET /user-profile/edit/{user-id}
```

## Response standardization Examples

### Success Response

```json
{
  "status": "success",
  "statusCode": 200,
  "message": "User created successfully.",
  "data": {
    "userId": 1,
    "name": "John Doe",
    "email": "john.doe@email.com"
  },
  "metadata": {
    "requestType": "PUT",
    "timestamp": "0000-00-00T00:00:00.000000Z",
    "traceId": "00-ed89d1cc507c35126d6f0e933984f774-99b8b9a3feb75652-00",
    "path": "/api/users/{id}"
  }
}
```

### Validation Error Response

```json
{
  "status": "failure",
  "statusCode": 400,
  "type": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred.",
  "errors": [
    {
      "Email": [
        "'Email' is not a valid email address."
      ]
    }
  ],
  "metadata": {
    "requestType": "PUT",
    "timestamp": "0000-00-00T00:00:00.000000Z",
    "trace_id": "00-8e5513ae9369648487c2323d9a3508aa-2a8f92c7d45d3f74-00",
    "path": "/api/users/{id}"
  }
}
```

## Contributing

[Contributions](https://github.com/keizrivas/AspNetConventions/tree/main?tab=contributing-ov-file), [issues](https://github.com/keizrivas/AspNetConventions/issues), and [feature requests](https://github.com/keizrivas/AspNetConventions/pulls) are welcome!

If you believe a convention should be improved or added, feel free to open a [discussion](https://github.com/keizrivas/AspNetConventions/discussions).

## License

This project is licensed under the MIT License. See [LICENSE](https://github.com/keizrivas/AspNetConventions/tree/main?tab=MIT-1-ov-file) for details.