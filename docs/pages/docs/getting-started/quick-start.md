# Quick Start

This guide covers installation and basic setup for each supported endpoint type.

---

## Installation {#installation}
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

## Setup {#setup}

### MVC Controllers {#mvc-controllers}

Call [`.AddAspNetConventions()`](./index.md#addaspnetconventions) on the `IMvcBuilder` returned by `.AddControllers()` or `.AddControllersWithViews()`:

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

### Minimal APIs {#minimal-apis}

Call [`.UseAspNetConventions()`](./index.md#useaspnetconventions) on the `WebApplication` to get a `RouteGroupBuilder`, then map your endpoints on the returned group.

```csharp
// Program.cs
using AspNetConventions;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var api = app.UseAspNetConventions();   // ← Here

api.MapGet("/WeatherForecast/{City}", (string City) =>
    Results.Ok(new { City, ZipCode = 000 }));

app.Run();
```

---

### Razor Pages {#razor-pages}

Call [`.AddAspNetConventions()`](./index.md#addaspnetconventions) on the `IMvcBuilder` returned by `.AddRazorPages()` or `.AddControllersWithViews()`:

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

#### Enable Tag Helpers {#enable-tag-helpers}

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

See [Disabling Property Transformation](./basic-usage.md#disabling-property-transformation) If you need to maintain the original C# property names.