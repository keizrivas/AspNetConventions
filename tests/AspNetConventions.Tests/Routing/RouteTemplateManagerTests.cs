using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Converters;
using AspNetConventions.Core.Enums;
using AspNetConventions.Routing;
using AspNetConventions.Routing.Models;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Xunit;

namespace AspNetConventions.Tests.Routing;

public class RouteTemplateManagerTests
{
    [Theory]
    [InlineData("api/TestController/GetUser", CasingStyle.KebabCase,  "api/test-controller/get-user")]
    [InlineData("api/TestController/GetUser", CasingStyle.SnakeCase,  "api/test_controller/get_user")]
    [InlineData("api/TestController/GetUser", CasingStyle.CamelCase,  "api/testController/getUser")]
    [InlineData("api/TestController/GetUser", CasingStyle.PascalCase, "Api/TestController/GetUser")]
    [InlineData("api/Test-Controller",        CasingStyle.KebabCase,  "api/test-controller")]
    [InlineData("api/test_controller",        CasingStyle.KebabCase,  "api/test-controller")]
    public void TransformTemplate_AppliesCasingStyle(string template, CasingStyle style, string expected)
    {
        var result = RouteTransformer.TransformRouteTemplate(template, CaseConverterFactory.Create(style));

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("api/[controller]/[action]")]    // route tokens
    [InlineData("api/users/{id}")]               // simple parameter
    [InlineData("api/users/{id?}")]              // optional parameter
    [InlineData("api/users/{id:int}")]           // typed constraint
    [InlineData("api/users/{*path}")]            // wildcard
    [InlineData("zip/{code:regex(^\\d{{3}}$)}")] // complex regex constraint
    [InlineData("~/api/test")]                   // tilde prefix
    public void TransformTemplate_PreservesSpecialPatterns(string template)
    {
        var result = RouteTransformer.TransformRouteTemplate(template, CaseConverterFactory.CreateKebabCase());

        Assert.Equal(template, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TransformTemplate_NullOrWhitespace_ReturnsOriginal(string? template)
    {
        var result = RouteTransformer.TransformRouteTemplate(template!, CaseConverterFactory.CreateKebabCase());

        Assert.Equal(template, result);
    }

    [Fact]
    public void TransformParameters_TransformsName_PreservesConstraint()
    {
        var options = CreateOptions();
        var route = "api/users/{id:int}/{userName:alpha}";
        var context = CreateMvcActionModelContext(route);

        var result = RouteTransformer.TransformRouteParameters(route, context, options);

        Assert.Equal("api/users/{id:int}/{user-name:alpha}", result);
    }

    [Fact]
    public void TransformParameters_NullTemplate_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RouteTransformer.TransformRouteParameters(null!, CreateMvcActionModelContext("api/test"), CreateOptions()));
    }

    [Fact]
    public void TransformParameters_UsesCacheOnSubsequentCalls()
    {
        var options = CreateOptions();
        var route = "api/users/{id}";
        var context = CreateMvcActionModelContext(route);
        var cache = new Dictionary<RouteParameterContext, bool>();

        RouteTransformer.TransformRouteParameters(route, context, options, cache);
        RouteTransformer.TransformRouteParameters(route, context, options, cache);

        Assert.Single(cache);
    }

    private static AspNetConventionOptions CreateOptions() =>
        new() { Route = new RouteConventionOptions { CaseStyle = CasingStyle.KebabCase } };

    private static RouteModelContext CreateMvcActionModelContext(string template)
    {
        var typeInfo = typeof(TestController).GetTypeInfo();
        var controller = new ControllerModel(typeInfo, []) { ControllerName = "Test" };
        var action = new ActionModel(typeInfo.GetMethod("Test")!, []) { Controller = controller };
        var selector = new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel { Template = template }
        };
        
        return RouteModelContext.FromMvcAction(selector, action);
    }

    private class TestController { public void Test() { } }
}
