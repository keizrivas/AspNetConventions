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

---

## Why AspNetConventions?

Building consistent ASP.NET Core applications shouldn't require hundreds of lines of configuration code. AspNetConventions applies standards automatically:

- **Universal Endpoint Support** - Consistent URL structure across MVC, Minimal APIs, and Razor Pages.
- **Automatic Route standardization** - Routes and parameters follow your preferred casing style.
- **Standardized responses** - Uniform JSON formatting and consistent response formatting application-wide.
- **Global exception handling** - Centralized error handling and formatting.
- **Fully Extensible** - Supports custom converters, mappers, hooks, and response formatters, as well as third-party library compatibility.
- **Zero Runtime Overhead** - Conventions are applied during application startup, no performance impact on requests.

---

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
builder.Services.AddControllersWithViews()
    .AddAspNetConventions();

var app = builder.Build();

// Apply conventions to Minimal APIs
var api = app.UseAspNetConventions("/");

api.MapGet("/GetUser/{UserId}", (int UserId) => new { userId = UserId });

app.Run();
```

**That's it!** Your entire application now follows consistent conventions.

---

## Contributing

Contributions are welcome! See [CONTRIBUTING.md](https://github.com/keizrivas/AspNetConventions/blob/main/CONTRIBUTING.md) for more information.

---

## Support & Community

- [Report Issues](https://github.com/keizrivas/AspNetConventions/issues)
- [Join Discussions](https://github.com/keizrivas/AspNetConventions/discussions)
- [Documentation](https://github.com/keizrivas/AspNetConventions/wiki)

---

## License

This project is licensed under the MIT License. See [LICENSE](https://github.com/keizrivas/AspNetConventions/blob/main/LICENSE) for details.