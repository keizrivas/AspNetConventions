using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Enums;
using AspNetConventions.Extensions;
using AspNetConventions.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AspNetConventions.Tests.Routing.Conventions;

public class RazorPageRouteConventionTests
{
    private readonly Mock<IOptions<AspNetConventionOptions>> _optionsMock;
    private readonly AspNetConventionOptions _options;

    public RazorPageRouteConventionTests()
    {
        _options = new AspNetConventionOptions();
        _optionsMock = new Mock<IOptions<AspNetConventionOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(_options);
    }

    [Fact]
    public void Apply_WhenRouteIsDisabled_DoesNotTransformRoutes()
    {
        _options.Route.IsEnabled = false;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModel("Pages/TestPage");

        convention.Apply(pageModel);

        Assert.Equal("Pages/TestPage", pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_WhenRazorPagesIsDisabled_DoesNotTransformRoutes()
    {
        _options.Route.IsEnabled = true;
        _options.Route.RazorPages.IsEnabled = false;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModel("Pages/TestPage");

        convention.Apply(pageModel);

        Assert.Equal("Pages/TestPage", pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_WithKebabCase_TransformsRouteToKebabCase()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModel("Pages/TestPage");

        convention.Apply(pageModel);

        Assert.Equal("pages/test-page", pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_WithSnakeCase_TransformsRouteToSnakeCase()
    {
        _options.Route.CaseStyle = CasingStyle.SnakeCase;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModel("Pages/TestPage");

        convention.Apply(pageModel);

        Assert.Equal("pages/test_page", pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_WithCamelCase_TransformsRouteToCamelCase()
    {
        _options.Route.CaseStyle = CasingStyle.CamelCase;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModel("Pages/TestPage");

        convention.Apply(pageModel);

        Assert.Equal("pages/testPage", pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_WithPascalCase_TransformsRouteToPascalCase()
    {
        _options.Route.CaseStyle = CasingStyle.PascalCase;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModel("pages/test-page");

        convention.Apply(pageModel);

        Assert.Equal("Pages/TestPage", pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_WithRouteParameters_TransformsParameterNames()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.RazorPages.TransformParameterNames = true;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModelWithRouteParams("Pages/TestPage", "{id}/{userName}");

        convention.Apply(pageModel);

        Assert.Equal("pages/test-page/{id}/{user-name}",
            pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_WithShouldTransformRouteHook_ReturnsEarly()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Hooks.ShouldTransformRoute = (_, _) => false;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModel("Pages/TestPage");

        convention.Apply(pageModel);

        Assert.Equal("Pages/TestPage", pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_CallsBeforeRouteTransformHook()
    {
        string? capturedTemplate = null;
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Hooks.BeforeRouteTransform = (template, _) => capturedTemplate = template;

        var convention = new RazorPageRouteConvention(_optionsMock.Object);
        var pageModel = CreatePageModel("Pages/TestPage");

        convention.Apply(pageModel);

        Assert.Equal("Pages/TestPage", capturedTemplate);
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

        var convention = new RazorPageRouteConvention(_optionsMock.Object);
        var pageModel = CreatePageModel("Pages/TestPage");

        convention.Apply(pageModel);

        Assert.Equal("pages/test-page", capturedNewTemplate);
        Assert.Equal("Pages/TestPage", capturedOldTemplate);
    }

    [Fact]
    public void Apply_WithNullRouteTemplate_SkipsTransformation()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModel(null);

        convention.Apply(pageModel);

        Assert.Null(pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_WithEmptyRouteTemplate_SkipsTransformation()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModel("");

        convention.Apply(pageModel);

        Assert.Equal("", pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_PreservesRouteConstraints()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.RazorPages.TransformParameterNames = true;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModelWithRouteParams("Pages/TestPage", "{id:int}");

        convention.Apply(pageModel);

        Assert.Equal("pages/test-page/{id:int}",
            pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_PreservesOptionalParameters()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.RazorPages.TransformParameterNames = true;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModelWithRouteParams("Pages/TestPage", "{id?}");

        convention.Apply(pageModel);

        Assert.Equal("pages/test-page/{id?}",
            pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_PreservesWildcardParameters()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.RazorPages.TransformParameterNames = true;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModelWithRouteParams("Pages/TestPage", "{*path}");

        convention.Apply(pageModel);

        Assert.Equal("pages/test-page/{*path}",
            pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_WithCustomCaseConverter_UsesCustomConverter()
    {
        var customConverter = new TestCaseConverter("pre");
        _options.Route.CaseConverter = customConverter;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModel("Pages/TestPage");

        convention.Apply(pageModel);

        Assert.Equal("pre-pages/pre-test-page",
            pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_PreservesLeadingSlash()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var pageModel = CreatePageModel("/Pages/TestPage");

        convention.Apply(pageModel);

        Assert.Equal("/pages/test-page", pageModel.Selectors[0].AttributeRouteModel?.Template);
    }

    private static PageRouteModel CreatePageModel(string? template)
    {
        var pageModel = new PageRouteModel(
            "TestPage",
            "/TestPage");

        pageModel.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = template != null
                ? new AttributeRouteModel { Template = template }
                : null
        });

        return pageModel;
    }

    private static PageRouteModel CreatePageModelWithRouteParams(string baseTemplate, string routeParams)
    {
        var pageModel = new PageRouteModel(
            "TestPage",
            "/TestPage");

        var fullTemplate = routeParams != null ? $"{baseTemplate}/{routeParams}" : baseTemplate;

        pageModel.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel { Template = fullTemplate }
        });

        return pageModel;
    }

    private class TestCaseConverter : Core.Abstractions.Contracts.ICaseConverter
    {
        private readonly string _prefix;

        public TestCaseConverter(string prefix)
        {
            _prefix = prefix;
        }

        public string Convert(string value)
        {
            return $"{_prefix}-{value.ToKebabCase()}";
        }
    }
}
