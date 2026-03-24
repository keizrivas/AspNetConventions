# Quick Start

This guide covers installation and basic setup for each supported endpoint type.

---

## Installation

::: tabs

== tab "dotnet CLI"
```bash
dotnet add package AspNetConventions
```

== tab "Package Manager Console (PMC)"
```powershell
Install-Package AspNetConventions
```
:::

**Requirements:** .NET 8, .NET 9, or .NET 10 ([.NET SDK](https://dotnet.microsoft.com/en-us/download){target="_blank"})


---

## Setup 

### MVC Controllers

Call `.AddAspNetConventions()` on the `IMvcBuilder` returned by `.AddControllers()` or `.AddControllersWithViews()`:

```csharp
// Program.cs
using AspNetConventions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddAspNetConventions();   // ← Here

var app = builder.Build();

app.MapControllers();
app.Run();
```
---

### Minimal APIs

Call `.UseAspNetConventions()` on the `WebApplication` after endpoints declaration to apply conventions.

```csharp
// Program.cs
using AspNetConventions;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/WeatherForecast/{City}", (string City) =>
    Results.Ok(new { City, ZipCode = null }));

app.UseAspNetConventions();   // ← Here

app.Run();
```

---

### Razor Pages

Call `.AddAspNetConventions()` on the `IMvcBuilder` returned by `.AddRazorPages()` or `.AddControllersWithViews()`:

```csharp
// Program.cs
using AspNetConventions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorPages()
    .AddAspNetConventions();   // ← Here

var app = builder.Build();

app.MapRazorPages();
app.Run();
```
Enable Tag Helpers conventions by adding `AspNetConventions` directives:

```less
// _ViewImports.cshtml

@addTagHelper *, AspNetConventions
```