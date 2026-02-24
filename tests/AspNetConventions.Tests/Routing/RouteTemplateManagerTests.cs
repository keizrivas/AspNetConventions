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
    [InlineData("api/test", CasingStyle.KebabCase, "api/test")]
    [InlineData("api/Test", CasingStyle.KebabCase, "api/test")]
    [InlineData("api/TestController", CasingStyle.KebabCase, "api/test-controller")]
    [InlineData("api/TestController/GetUser", CasingStyle.KebabCase, "api/test-controller/get-user")]
    [InlineData("api/Test-Controller/Get_User", CasingStyle.KebabCase, "api/test-controller/get-user")]
    public void TransformRouteTemplate_WithKebabCase_TransformsCorrectly(
        string template, CasingStyle caseStyle, string expected)
    {
        var converter = CaseConverterFactory.Create(caseStyle);

        var result = RouteTransformer.TransformRouteTemplate(template, converter);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("api/test", CasingStyle.SnakeCase, "api/test")]
    [InlineData("api/Test", CasingStyle.SnakeCase, "api/test")]
    [InlineData("api/TestController/GetUser", CasingStyle.SnakeCase, "api/test_controller/get_user")]
    [InlineData("api/TestController/Get-User", CasingStyle.SnakeCase, "api/test_controller/get_user")]
    public void TransformRouteTemplate_WithSnakeCase_TransformsCorrectly(
        string template, CasingStyle caseStyle, string expected)
    {
        var converter = CaseConverterFactory.Create(caseStyle);

        var result = RouteTransformer.TransformRouteTemplate(template, converter);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("api/test", CasingStyle.CamelCase, "api/test")]
    [InlineData("api/Test", CasingStyle.CamelCase, "api/test")]
    [InlineData("api/TestController/GetUser", CasingStyle.CamelCase, "api/testController/getUser")]
    [InlineData("api/TestController/Get_User", CasingStyle.CamelCase, "api/testController/getUser")]
    public void TransformRouteTemplate_WithCamelCase_TransformsCorrectly(
        string template, CasingStyle caseStyle, string expected)
    {
        var converter = CaseConverterFactory.Create(caseStyle);

        var result = RouteTransformer.TransformRouteTemplate(template, converter);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("api/test", CasingStyle.PascalCase, "Api/Test")]
    [InlineData("api/Test", CasingStyle.PascalCase, "Api/Test")]
    [InlineData("api/TestController/GetUser", CasingStyle.PascalCase, "Api/TestController/GetUser")]
    [InlineData("api/TestController/Get_User", CasingStyle.PascalCase, "Api/TestController/GetUser")]
    public void TransformRouteTemplate_WithPascalCase_TransformsCorrectly(
        string template, CasingStyle caseStyle, string expected)
    {
        var converter = CaseConverterFactory.Create(caseStyle);

        var result = RouteTransformer.TransformRouteTemplate(template, converter);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TransformRouteTemplate_WithNullOrEmpty_ReturnsOriginal(string? template)
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var result = RouteTransformer.TransformRouteTemplate(template!, converter);

        Assert.Equal(template, result);
    }

    [Fact]
    public void TransformRouteTemplate_PreservesLeadingSlash()
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var route = "/api/test";
        var result = RouteTransformer.TransformRouteTemplate(route, converter);

        Assert.Equal(route, result);
    }

    [Fact]
    public void TransformRouteTemplate_PreservesRouteTokens()
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var route = "api/[controller]/[action]";
        var result = RouteTransformer.TransformRouteTemplate(route, converter);

        Assert.Equal(route, result);
    }

    [Fact]
    public void TransformRouteTemplate_PreservesParameters()
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var route = "api/users/{id}";
        var result = RouteTransformer.TransformRouteTemplate(route, converter);

        Assert.Equal(route, result);
    }

    [Fact]
    public void TransformRouteTemplate_PreservesOptionalParameters()
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var route = "api/users/{id?}";
        var result = RouteTransformer.TransformRouteTemplate(route, converter);

        Assert.Equal(route, result);
    }

    [Fact]
    public void TransformRouteTemplate_PreservesRouteConstraints()
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var route = "api/users/{id:int}";
        var result = RouteTransformer.TransformRouteTemplate(route, converter);

        Assert.Equal(route, result);
    }


    [Fact]
    public void TransformRouteTemplate_PreservesComplexRouteConstraints()
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var route = "zip-code/{code:regex(^\\d{{3}}$)}";
        var result = RouteTransformer.TransformRouteTemplate(route, converter);

        Assert.Equal(route, result);
    }

    [Fact]
    public void TransformRouteTemplate_PreservesWildcardParameters()
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var route = "api/users/{*path}";
        var result = RouteTransformer.TransformRouteTemplate(route, converter);

        Assert.Equal(route, result);
    }

    [Fact]
    public void TransformRouteTemplate_PreservesTilde()
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var route = "~/api/test";
        var result = RouteTransformer.TransformRouteTemplate(route, converter);

        Assert.Equal(route, result);
    }

    [Fact]
    public void TransformRouteTemplate_PreservesBackslash()
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var route = @"api\test";
        var result = RouteTransformer.TransformRouteTemplate(route, converter);

        Assert.Equal(route, result);
    }

    [Fact]
    public void TransformRouteTemplate_PreservesDotNotation()
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var route = "api/users/..";
        var result = RouteTransformer.TransformRouteTemplate(route, converter);

        Assert.Equal(route, result);
    }

    [Fact]
    public void TransformRouteTemplate_WithEmptySegments_ReturnsOriginal()
    {
        var converter = CaseConverterFactory.CreateKebabCase();

        var route = "//";
        var result = RouteTransformer.TransformRouteTemplate(route, converter);

        Assert.Equal(route, result);
    }

    [Fact]
    public void TransformRouteParameters_ThrowsOnNullTemplate()
    {
        var options = CreateOptions();
        var modelContext = CreateMvcActionModelContext("api/test");

        Assert.Throws<ArgumentNullException>(() =>
            RouteTransformer.TransformRouteParameters(null!, modelContext, options));
    }

    [Theory]
    [InlineData("api/users/{id}", "api/users/{id}")]
    [InlineData("api/users/{id:int}", "api/users/{id:int}")]
    [InlineData("api/users/{id?}", "api/users/{id?}")]
    [InlineData("api/users/{*path}", "api/users/{*path}")]
    public void TransformRouteParameters_WithNoParameters_PreservesTemplate(string template, string expected)
    {
        var options = CreateOptions();
        var modelContext = CreateMvcActionModelContext(template);

        var result = RouteTransformer.TransformRouteParameters(template, modelContext, options);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TransformRouteParameters_WithParameters_TransformsParameterNames()
    {
        var options = CreateOptions();

        var route = "api/users/{id}/{userName}";
        var modelContext = CreateMvcActionModelContext(route);

        var result = RouteTransformer.TransformRouteParameters(route, modelContext, options);

        Assert.Equal("api/users/{id}/{user-name}", result);
    }

    [Fact]
    public void TransformRouteParameters_WithConstraint_TransformsParameterNameOnly()
    {
        var options = CreateOptions();

        var route = "api/users/{id:int}/{userName:alpha}";
        var modelContext = CreateMvcActionModelContext(route);

        var result = RouteTransformer.TransformRouteParameters(route, modelContext, options);

        Assert.Equal("api/users/{id:int}/{user-name:alpha}", result);
    }

    [Fact]
    public void TransformRouteParameters_WithCache_UsesCache()
    {
        var options = CreateOptions();

        var route = "api/users/{id}";
        var modelContext = CreateMvcActionModelContext(route);
        var cache = new Dictionary<RouteParameterContext, bool>();

        RouteTransformer.TransformRouteParameters(route, modelContext, options, cache);

        Assert.Single(cache);

        RouteTransformer.TransformRouteParameters(route, modelContext, options, cache);

        Assert.Single(cache);
    }

    private static AspNetConventionOptions CreateOptions()
    {
        return new AspNetConventionOptions
        {
            Route = new RouteConventionOptions
            {
                CaseStyle = CasingStyle.KebabCase
            }
        };
    }

    private static RouteModelContext CreateMvcActionModelContext(string template)
    {
        var controllerTypeInfo = typeof(TestController).GetTypeInfo();
        var methodInfo = controllerTypeInfo.GetMethod("Test")!;

        var controller = new ControllerModel(controllerTypeInfo, [])
        {
            ControllerName = "Test"
        };

        var action = new ActionModel(methodInfo, [])
        {
            Controller = controller
        };

        var selector = new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel
            {
                Template = template
            }
        };

        return RouteModelContext.FromMvcAction(selector, action);
    }

    private class TestController
    {
        public void Test() { }
    }
}
