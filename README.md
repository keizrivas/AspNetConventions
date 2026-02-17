# AspNetConventions

[![License](https://img.shields.io/github/license/keizrivas/AspNetConventions)](https://github.com/keizrivas/AspNetConventions/blob/main/LICENSE)
[![Build Status](https://github.com/keizrivas/AspNetConventions/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/keizrivas/AspNetConventions/actions)
[![NuGet Version](https://img.shields.io/nuget/v/AspNetConventions.svg)](https://www.nuget.org/packages/AspNetConventions)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AspNetConventions.svg)](https://www.nuget.org/packages/AspNetConventions)
[![.NET](https://img.shields.io/badge/.NET-8.0+-512BD4.svg)](https://dotnet.microsoft.com/download)

<p align="center">
  <img src="https://raw.githubusercontent.com/keizrivas/AspNetConventions/main/assets/asp_net_conventions.svg" width="250">
</p>

**Convention-driven standardization for ASP.NET Core** - Automatically applies consistent standardization across ASP.NET Core applications — including APIs, MVC, and Razor Pages — with minimal configuration and zero boilerplate.

<p align="center">
  <img src="https://raw.githubusercontent.com/keizrivas/AspNetConventions/main/assets/banner.png" width="100%">
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

## Contributing

[Contributions](https://github.com/keizrivas/AspNetConventions/tree/main?tab=contributing-ov-file), [issues](https://github.com/keizrivas/AspNetConventions/issues), and [feature requests](https://github.com/keizrivas/AspNetConventions/pulls) are welcome!

If you believe a convention should be improved or added, feel free to open a [discussion](https://github.com/keizrivas/AspNetConventions/discussions).

## License

This project is licensed under the MIT License. See [LICENSE](https://github.com/keizrivas/AspNetConventions/tree/main?tab=MIT-1-ov-file) for details.