using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Enums;
using AspNetConventions.Routing;
using AspNetConventions.Routing.Conventions;
using AspNetConventions.Routing.Models;
using AspNetConventions.Routing.Parsers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AspNetConventions.Tests.Routing.Conventions;

public class RouteControllerConventionTests
{
    private readonly Mock<IOptions<AspNetConventionOptions>> _optionsMock;
    private readonly AspNetConventionOptions _options;

    public RouteControllerConventionTests()
    {
        _options = new AspNetConventionOptions();
        _optionsMock = new Mock<IOptions<AspNetConventionOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(_options);
    }

    [Theory]
    [InlineData(CasingStyle.KebabCase,  "api/test-controller/get-test")]
    [InlineData(CasingStyle.SnakeCase,  "api/test_controller/get_test")]
    [InlineData(CasingStyle.CamelCase,  "api/testController/getTest")]
    [InlineData(CasingStyle.PascalCase, "Api/TestController/GetTest")]
    public void Apply_TransformsRoute_ForAllCasingStyles(CasingStyle style, string expected)
    {
        _options.Route.CaseStyle = style;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        convention.Apply(CreateControllerModel("Api/TestController", "GetTest"));

        Assert.Equal(expected, GetTemplate(CreateAndApply(style, "Api/TestController", "GetTest")));
    }

    [Fact]
    public void Apply_WhenDisabled_PreservesOriginalTemplate()
    {
        _options.Route.IsEnabled = false;
        var convention = new RouteControllerConvention(_optionsMock.Object);
        var controller = CreateControllerModel("Api/TestController", "GetTest");

        convention.Apply(controller);

        Assert.Equal("Api/TestController/GetTest", GetTemplate(controller));
    }

    [Fact]
    public void Apply_WithParameters_TransformsNames_PreservesConstraints()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Controllers.TransformParameterNames = true;

        var convention = new RouteControllerConvention(_optionsMock.Object);
        var controller = CreateControllerModelWithParameters(
            "Api/TestController", "GetUser",
            [("id:int", typeof(int)), ("userName", typeof(string))]);

        convention.Apply(controller);

        Assert.Equal("api/test-controller/get-user/{id:int}/{user-name}", GetTemplate(controller));
    }

    [Fact]
    public void Apply_WithPreserveExplicitBindingNames_HonorsExplicitBinderName()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Controllers.TransformParameterNames = true;
        _options.Route.Controllers.PreserveExplicitBindingNames = true;

        var convention = new RouteControllerConvention(_optionsMock.Object);
        var controller = CreateControllerModelWithParametersAndBinding(
            "Api/TestController", "GetUser",
            [("id", typeof(int), "UserId"), ("userName", typeof(string), null)]);

        convention.Apply(controller);

        Assert.Equal("api/test-controller/get-user/{UserId}/{user-name}", GetTemplate(controller));
    }

    [Fact]
    public void Apply_Hooks_BeforeAndAfterFireWithCorrectTemplates()
    {
        string? before = null, afterOld = null, afterNew = null;
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Hooks.BeforeRouteTransform = (route, _) => before = route;
        _options.Route.Hooks.AfterRouteTransform  = (route, old, _) => { afterNew = route; afterOld = old; };

        new RouteControllerConvention(_optionsMock.Object)
            .Apply(CreateControllerModel("Api/TestController", "GetTest"));

        Assert.Equal("Api/TestController/GetTest", before);
        Assert.Equal("Api/TestController/GetTest", afterOld);
        Assert.Equal("api/test-controller/get-test", afterNew);
    }

    [Fact]
    public void Apply_PreservesRouteTokens_And_Constraints()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Controllers.TransformParameterNames = true;
        
        var convention = new RouteControllerConvention(_optionsMock.Object);
        var controller = CreateControllerModelWithParameters(
            "api/[controller]", "GetUser", [("id:int", typeof(int))]);

        convention.Apply(controller);

        Assert.Equal("api/[controller]/get-user/{id:int}", GetTemplate(controller));
    }

    private ControllerModel CreateAndApply(CasingStyle style, string route, string action)
    {
        _options.Route.CaseStyle = style;
        var controller = CreateControllerModel(route, action);
        new RouteControllerConvention(_optionsMock.Object).Apply(controller);
        return controller;
    }

    private static string? GetTemplate(ControllerModel controller)
    {
        var context = RouteModelContext.FromMvcAction(
            controller.Actions[0].Selectors[0],
            controller.Actions[0]);
        return RouteTransformer.GetRouteTemplate(context);
    }

    private static ControllerModel CreateControllerModel(string? routeTemplate, string actionName)
    {
        var typeInfo = typeof(TestController).GetTypeInfo();
        var controller = new ControllerModel(typeInfo, []) { ControllerName = "Test" };
        controller.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = routeTemplate != null
                ? new AttributeRouteModel { Template = routeTemplate }
                : null
        });
        var action = new ActionModel(typeInfo.GetMethod(actionName)!, []) { Controller = controller };
        action.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = routeTemplate != null
                ? new AttributeRouteModel { Template = actionName }
                : null
        });
        controller.Actions.Add(action);
        return controller;
    }

    private static ControllerModel CreateControllerModelWithParameters(
        string routeTemplate, string actionName, (string name, Type type)[] parameters)
    {
        var typeInfo = typeof(TestController).GetTypeInfo();
        var controller = new ControllerModel(typeInfo, []) { ControllerName = "Test" };
        controller.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel { Template = routeTemplate }
        });
        var action = new ActionModel(typeInfo.GetMethod(actionName)!, []) { Controller = controller };
        var parts = new List<string>();
        foreach (var (name, type) in parameters)
        {
            var cleanName = RouteParameterPatterns.CleanParameterName(name.Split(":")[0]);
            var paramInfo = typeInfo.GetMethod(actionName)!.GetParameters().First(p => p.Name == cleanName);
            var param = new ParameterModel(paramInfo, [])
            {
                ParameterName = paramInfo.Name ?? cleanName,
                BindingInfo = new BindingInfo()
            };
            action.Parameters.Add(param);
            parts.Add($"{{{name}}}");
        }
        action.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel
            {
                Template = $"{actionName}/{string.Join("/", parts)}"
            }
        });
        controller.Actions.Add(action);
        return controller;
    }

    private static ControllerModel CreateControllerModelWithParametersAndBinding(
        string routeTemplate, string actionName, (string name, Type type, string? bindingName)[] parameters)
    {
        var typeInfo = typeof(TestController).GetTypeInfo();
        var controller = new ControllerModel(typeInfo, []) { ControllerName = "Test" };

        controller.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel { Template = routeTemplate }
        });

        var action = new ActionModel(typeInfo.GetMethod(actionName)!, []) { Controller = controller };
        var parts = new List<string>();
        
        foreach (var (name, type, bindingName) in parameters)
        {
            var cleanName = RouteParameterPatterns.CleanParameterName(name.Split(":")[0]);
            var paramInfo = typeInfo.GetMethod(actionName)!.GetParameters().First(p => p.Name == cleanName);
            var param = new ParameterModel(paramInfo, [])
            {
                ParameterName = paramInfo.Name ?? cleanName,
                BindingInfo = bindingName != null
                    ? new BindingInfo { BinderModelName = bindingName, BindingSource = BindingSource.Path }
                    : new BindingInfo()
            };
            action.Parameters.Add(param);
            parts.Add($"{{{bindingName ?? name}}}");
        }

        action.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel
            {
                Template = $"{actionName}/{string.Join("/", parts)}"
            }
        });
        
        controller.Actions.Add(action);
        return controller;
    }

    private class TestController : Controller
    {
        public IActionResult GetTest() => Ok();
        public IActionResult GetUser(int id, string userName) => Ok(new { id, userName });
    }
}
