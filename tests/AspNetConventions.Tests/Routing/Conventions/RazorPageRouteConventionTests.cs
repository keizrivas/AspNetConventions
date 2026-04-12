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

    [Theory]
    [InlineData("Pages/TestPage",  CasingStyle.KebabCase,  "pages/test-page")]
    [InlineData("Pages/TestPage",  CasingStyle.SnakeCase,  "pages/test_page")]
    [InlineData("Pages/TestPage",  CasingStyle.CamelCase,  "pages/testPage")]
    [InlineData("pages/test-page", CasingStyle.PascalCase, "Pages/TestPage")]
    public void Apply_TransformsRoute_ForAllCasingStyles(string template, CasingStyle style, string expected)
    {
        _options.Route.CaseStyle = style;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);
        var page = CreatePageModel(template);

        convention.Apply(page);

        Assert.Equal(expected, page.Selectors[0].AttributeRouteModel?.Template);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true,  false)]
    public void Apply_WhenDisabled_PreservesTemplate(bool routeEnabled, bool razorEnabled)
    {
        _options.Route.IsEnabled = routeEnabled;
        _options.Route.RazorPages.IsEnabled = razorEnabled;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);
        var page = CreatePageModel("Pages/TestPage");

        convention.Apply(page);

        Assert.Equal("Pages/TestPage", page.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_WithParameters_TransformsName_PreservesConstraint_Optional_Wildcard()
    {
        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.RazorPages.TransformParameterNames = true;
        var convention = new RazorPageRouteConvention(_optionsMock.Object);

        var plain    = CreatePageModelWithRouteParams("Pages/TestPage", "{userName}");
        var typed    = CreatePageModelWithRouteParams("Pages/TestPage", "{id:int}");
        var optional = CreatePageModelWithRouteParams("Pages/TestPage", "{id?}");
        var wildcard = CreatePageModelWithRouteParams("Pages/TestPage", "{*path}");

        convention.Apply(plain);
        convention.Apply(typed);
        convention.Apply(optional);
        convention.Apply(wildcard);

        Assert.Equal("pages/test-page/{user-name}", plain.Selectors[0].AttributeRouteModel?.Template);
        Assert.Equal("pages/test-page/{id:int}", typed.Selectors[0].AttributeRouteModel?.Template);
        Assert.Equal("pages/test-page/{id?}", optional.Selectors[0].AttributeRouteModel?.Template);
        Assert.Equal("pages/test-page/{*path}", wildcard.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_Hooks_BeforeAndAfterFireWithCorrectTemplates_ShouldTransformCanSkip()
    {
        string? before = null, afterOld = null, afterNew = null;

        _options.Route.CaseStyle = CasingStyle.KebabCase;
        _options.Route.Hooks.BeforeRouteTransform = (route, _) => before = route;
        _options.Route.Hooks.AfterRouteTransform  = (route, old, _) => { afterNew = route; afterOld = old; };

        new RazorPageRouteConvention(_optionsMock.Object)
            .Apply(CreatePageModel("Pages/TestPage"));

        Assert.Equal("Pages/TestPage", before);
        Assert.Equal("Pages/TestPage", afterOld);
        Assert.Equal("pages/test-page", afterNew);

        _options.Route.Hooks.ShouldTransformRoute = (_, _) => false;
        var page2 = CreatePageModel("Pages/TestPage");
        new RazorPageRouteConvention(_optionsMock.Object).Apply(page2);

        Assert.Equal("Pages/TestPage", page2.Selectors[0].AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_WithCustomCaseConverter_UsesCustomConverter()
    {
        _options.Route.CaseConverter = new PrefixedConverter("pre");
        var convention = new RazorPageRouteConvention(_optionsMock.Object);
        var page = CreatePageModel("Pages/TestPage");

        convention.Apply(page);

        Assert.Equal("pre-pages/pre-test-page", page.Selectors[0].AttributeRouteModel?.Template);
    }

    private static PageRouteModel CreatePageModel(string? template)
    {
        var page = new PageRouteModel("TestPage", "/TestPage");
        page.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = template != null
                ? new AttributeRouteModel { Template = template }
                : null
        });
        return page;
    }

    private static PageRouteModel CreatePageModelWithRouteParams(string baseTemplate, string routeParams)
    {
        var page = new PageRouteModel("TestPage", "/TestPage");
        page.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel { Template = $"{baseTemplate}/{routeParams}" }
        });
        return page;
    }

    private class PrefixedConverter : Core.Abstractions.Contracts.ICaseConverter
    {
        private readonly string _prefix;
        public PrefixedConverter(string prefix) => _prefix = prefix;
        public string Convert(string value) => $"{_prefix}-{value.ToKebabCase()}";
    }
}
