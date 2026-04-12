# Quick Start

This guide covers installation and basic setup for each supported endpoint type.

---

## Installation
Install the package via your preferred package manager.

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

Call [`.AddAspNetConventions()`](/docs/getting-started/#addaspnetconventions) on the `IMvcBuilder` returned by `.AddControllers()` or `.AddControllersWithViews()`:

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

Call [.UseAspNetConventions()](/docs/getting-started/#useaspnetconventions) on the `WebApplication` after endpoints declaration to apply conventions.

```csharp
// Program.cs
using AspNetConventions;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/WeatherForecast/{City}", (string City) =>
    Results.Ok(new { City, ZipCode = 000 }));

app.UseAspNetConventions();   // ← Here

app.Run();
```

---

### Razor Pages

Call [`.AddAspNetConventions()`](/docs/getting-started/#addaspnetconventions) on the `IMvcBuilder` returned by `.AddRazorPages()` or `.AddControllersWithViews()`:

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

#### Enable Tag Helpers

To support standardized routing and property binding within your views, add the `AspNetConventions` directive to your `_ViewImports.cshtml` file. 
This ensures that helper attributes like `asp-for` correctly map your C# properties to the transformed HTML name attributes:

```less
// _ViewImports.cshtml
@addTagHelper *, AspNetConventions
```

**How it works:**
The Tag Helper automatically aligns your HTML forms with your selected casing style:

```html
<!-- Input Source -->
<input asp-for="CityName" />

<!-- Generated Output (e.g., kebab-case) -->
<input name="city-name" id="CityName" type="text" value="" />
```

See [Disabling Property Transformation](/docs/getting-started/basic-usage/#disabling-property-transformation) If you need to maintain the original C# property names.