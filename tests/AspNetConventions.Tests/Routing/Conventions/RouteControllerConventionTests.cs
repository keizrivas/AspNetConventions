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

    [Fact]
    public void Apply_WhenRouteIsDisabled_DoesNotTransformRoutes()
    {
        _options.Route.IsEnabled = false;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModel("api/test", "GetTest");

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("api/test/GetTest", template);
    }

    [Fact]
    public void Apply_WithKebabCase_TransformsRouteToKebabCase()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModel("Api/TestController", "GetTest");

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("api/test-controller/get-test", template);
    }

    [Fact]
    public void Apply_WithSnakeCase_TransformsRouteToSnakeCase()
    {
        _options.Route.CaseStyle = CasingStyle.SnakeCase;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModel("Api/TestController", "GetTest");

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("api/test_controller/get_test", template);
    }

    [Fact]
    public void Apply_WithCamelCase_TransformsRouteToCamelCase()
    {
        _options.Route.CaseStyle = CasingStyle.CamelCase;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModel("Api/TestController", "GetTest");

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("api/testController/getTest", template);
    }

    [Fact]
    public void Apply_WithPascalCase_TransformsRouteToPascalCase()
    {
        _options.Route.CaseStyle = CasingStyle.PascalCase;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModel("api/test-controller", "GetTest");

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("Api/TestController/GetTest", template);
    }

    [Fact]
    public void Apply_WithRouteParameters_TransformsParameterNames()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Controllers.TransformParameterNames = true;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModelWithParameters(
            "Api/TestController",
            "GetUser",
            [("id", typeof(int)), ("userName", typeof(string))]);

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("api/test-controller/get-user/{id}/{user-name}", template);
    }

    [Fact]
    public void Apply_WithPreserveExplicitBindingNames_DoesNotTransformExplicitNames()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Controllers.TransformParameterNames = true;
        _options.Route.Controllers.PreserveExplicitBindingNames = true;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModelWithParametersAndBinding(
            "Api/TestController",
            "GetUser",
            [("id", typeof(int), "UserId"),
            ("userName", typeof(string), null)]);

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("api/test-controller/get-user/{UserId}/{user-name}", template);
    }

    [Fact]
    public void Apply_WithShouldTransformRouteHook_ReturnsEarly()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Hooks.ShouldTransformRoute = (_, _) => false;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModel("Api/TestController", "GetTest");

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("Api/TestController/GetTest", template);
    }

    [Fact]
    public void Apply_CallsBeforeRouteTransformHook()
    {
        string? capturedTemplate = null;
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Hooks.BeforeRouteTransform = (template, _) => capturedTemplate = template;

        var convention = new RouteControllerConvention(_optionsMock.Object);
        var controller = CreateControllerModel("Api/TestController", "GetTest");

        convention.Apply(controller);

        Assert.Equal("Api/TestController/GetTest", capturedTemplate);
    }

    [Fact]
    public void Apply_CallsAfterRouteTransformHook()
    {
        string? capturedNewTemplate = null;
        string? capturedOldTemplate = null;
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Hooks.AfterRouteTransform = (newTemplate, oldTemplate, _) =>
        {
            capturedNewTemplate = newTemplate;
            capturedOldTemplate = oldTemplate;
        };

        var convention = new RouteControllerConvention(_optionsMock.Object);
        var controller = CreateControllerModel("Api/TestController", "GetTest");

        convention.Apply(controller);

        Assert.Equal("api/test-controller/get-test", capturedNewTemplate);
        Assert.Equal("Api/TestController/GetTest", capturedOldTemplate);
    }

    [Fact]
    public void Apply_WithNullRouteTemplate_SkipsTransformation()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModel(null, "GetTest");

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Null(template);
    }

    [Fact]
    public void Apply_WithEmptyRouteTemplate_SkipsTransformation()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModel("", "GetTest");

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("", template);
    }

    [Fact]
    public void Apply_PreservesRouteTokens_DoesNotTransformBrackets()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModel("api/[controller]", "GetTest");

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("api/[controller]/get-test", template);
    }

    [Fact]
    public void Apply_PreservesRouteConstraints_DoesNotTransformConstraints()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Controllers.TransformParameterNames = true;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModelWithParameters(
            "Api/TestController",
            "GetUser",
            [("id:int", typeof(int))]);

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("api/test-controller/get-user/{id:int}", template);
    }

    [Fact]
    public void Apply_PreservesOptionalParameters()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Controllers.TransformParameterNames = true;
        var convention = new RouteControllerConvention(_optionsMock.Object);

        var controller = CreateControllerModelWithParameters(
            "Api/TestController",
            "GetUser",
            [("id?", typeof(int?))]);

        convention.Apply(controller);

        var context = RouteModelContext.FromMvcAction(controller.Actions[0].Selectors[0], controller.Actions[0]);
        var template = RouteTransformer.GetRouteTemplate(context);

        Assert.Equal("api/test-controller/get-user/{id?}", template);
    }

    private static ControllerModel CreateControllerModel(string? routeTemplate, string actionName)
    {
        var controllerTypeInfo = typeof(TestController).GetTypeInfo();
        var methodInfo = typeof(TestController).GetMethod(actionName)!;

        var controller = new ControllerModel(controllerTypeInfo, [])
        {
            ControllerName = "Test"
        };

        controller.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = routeTemplate != null
                ? new AttributeRouteModel { Template = routeTemplate }
                : null
        });

        var action = new ActionModel(methodInfo, [])
        {
            Controller = controller
        };

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
        string routeTemplate,
        string actionName,
        (string name, Type type)[] parameters)
    {
        var controllerTypeInfo = typeof(TestController).GetTypeInfo();
        var methodInfo = typeof(TestController).GetMethod(actionName)!;

        var controller = new ControllerModel(controllerTypeInfo, [])
        {
            ControllerName = "Test"
        };

        controller.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel { Template = routeTemplate }
        });

        var action = new ActionModel(methodInfo, [])
        {
            Controller = controller
        };

        var paramsTemplate = new List<string>();
        foreach (var (name, type) in parameters)
        {
            var paramName = RouteParameterPatterns.CleanParameterName(name.Split(":")[0]);
            var paramInfo = methodInfo.GetParameters().First(p => p.Name == paramName);
            var param = new ParameterModel(paramInfo, [])
            {
                ParameterName = paramInfo.Name ?? paramName,
            };
            param.BindingInfo ??= new BindingInfo();
            action.Parameters.Add(param);
            paramsTemplate.Add($"{{{name}}}");
        }

        action.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel
            {
                Template = actionName + "/" + String.Join("/", [.. paramsTemplate])
            }
        });

        controller.Actions.Add(action);

        return controller;
    }

    private static ControllerModel CreateControllerModelWithParametersAndBinding(
        string routeTemplate,
        string actionName,
        (string name, Type type, string? bindingName)[] parameters)
    {
        var controllerTypeInfo = typeof(TestController).GetTypeInfo();
        var methodInfo = typeof(TestController).GetMethod(actionName)!;

        var controller = new ControllerModel(controllerTypeInfo, [])
        {
            ControllerName = "Test"
        };

        controller.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel { Template = routeTemplate }
        });

        var action = new ActionModel(methodInfo, [])
        {
            Controller = controller
        };

        var paramsTemplate = new List<string>();
        foreach (var (name, type, bindingName) in parameters)
        {
            var paramName = RouteParameterPatterns.CleanParameterName(name.Split(":")[0]);
            var paramInfo = methodInfo.GetParameters().First(p => p.Name == paramName);
            var param = new ParameterModel(paramInfo, [])
            {
                ParameterName = paramInfo.Name ?? paramName,
            };

            if (bindingName != null)
            {
                param.BindingInfo = new BindingInfo
                {
                    BinderModelName = bindingName,
                    BindingSource = BindingSource.Path
                };
            }
            else
            {
                param.BindingInfo = new BindingInfo();
            }

            action.Parameters.Add(param);
            paramsTemplate.Add($"{{{bindingName ?? name}}}");
        }

        action.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel
            {
                Template = actionName + "/" + String.Join("/", [.. paramsTemplate])
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
