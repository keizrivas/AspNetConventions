using System.Net;
using AspNetConventions.Configuration.Options;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Mappers;
using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http.Services;
using Xunit;

namespace AspNetConventions.Tests.ExceptionHandling;

public class ExceptionHandlingOptionsTests
{
    [Fact]
    public void Defaults_IsEnabled_True_CollectionsInitialized_Empty()
    {
        var options = new ExceptionHandlingOptions();

        Assert.True(options.IsEnabled);
        Assert.Empty(options.Mappers);
        Assert.Empty(options.ExcludeStatusCodes);
        Assert.Empty(options.ExcludeException);
        Assert.NotNull(options.Hooks);
    }

    [Fact]
    public void GetMapper_ReturnsFirstRegisteredCustom_ThenDefaultFallback()
    {
        var options = new ExceptionHandlingOptions();
        var first = new AlwaysMatchMapper();
        var second = new AlwaysMatchMapper();
        options.Mappers.Add(first);
        options.Mappers.Add(second);

        Assert.Same(first, options.GetExceptionMapper(new Exception(), null!));

        options.Mappers.Clear();

        Assert.IsType<DefaultExceptionMapper>(
            options.GetExceptionMapper(new InvalidOperationException(), null!));
    }

    [Fact]
    public void Clone_ProducesIndependentCopy_MutatingOriginalDoesNotAffectClone()
    {
        var options = new ExceptionHandlingOptions
        {
            IsEnabled = false,
            ExcludeStatusCodes = { HttpStatusCode.NotFound },
            ExcludeException = { typeof(ArgumentException) }
        };

        var clone = (ExceptionHandlingOptions)options.Clone();
        options.IsEnabled = true;
        options.ExcludeStatusCodes.Add(HttpStatusCode.BadRequest);

        Assert.False(clone.IsEnabled);
        Assert.DoesNotContain(HttpStatusCode.BadRequest, clone.ExcludeStatusCodes);
        Assert.Contains(typeof(ArgumentException), clone.ExcludeException);
    }

    private class AlwaysMatchMapper : IExceptionMapper
    {
        public bool CanMapException(Exception exception, RequestDescriptor request) => true;
        public ExceptionDescriptor MapException(Exception exception, RequestDescriptor request) => new();
    }
}
