using System.Net;
using AspNetConventions.Configuration.Options;
using AspNetConventions.ExceptionHandling.Abstractions;
using AspNetConventions.ExceptionHandling.Mappers;
using Xunit;

namespace AspNetConventions.Tests.ExceptionHandling;

public class ExceptionHandlingOptionsTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var options = new ExceptionHandlingOptions();

        Assert.True(options.IsEnabled);
        Assert.NotNull(options.Mappers);
        Assert.NotNull(options.ExcludeStatusCodes);
        Assert.NotNull(options.ExcludeException);
        Assert.NotNull(options.Hooks);
    }

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var options = new ExceptionHandlingOptions
        {
            IsEnabled = false,
            ExcludeStatusCodes = { HttpStatusCode.NotFound },
            ExcludeException = { typeof(ArgumentException) }
        };

        var cloned = (ExceptionHandlingOptions)options.Clone();

        Assert.False(cloned.IsEnabled);
        Assert.Contains(HttpStatusCode.NotFound, cloned.ExcludeStatusCodes);
        Assert.Contains(typeof(ArgumentException), cloned.ExcludeException);

        options.IsEnabled = true;
        Assert.False(cloned.IsEnabled);
    }

    [Fact]
    public void GetExceptionMapper_WithCustomMapper_ReturnsCustomMapper()
    {
        var options = new ExceptionHandlingOptions();
        var customMapper = new CustomExceptionMapper();
        options.Mappers.Add(customMapper);

        var result = options.GetExceptionMapper(new InvalidOperationException(), null!);

        Assert.Same(customMapper, result);
    }

    [Fact]
    public void GetExceptionMapper_WithNoCustomMapper_ReturnsDefaultMapper()
    {
        var options = new ExceptionHandlingOptions();

        var result = options.GetExceptionMapper(new InvalidOperationException(), null!);

        Assert.IsType<DefaultExceptionMapper>(result);
    }

    [Fact]
    public void GetExceptionMapper_FirstMatchingMapperIsReturned()
    {
        var options = new ExceptionHandlingOptions();
        var firstMapper = new CustomExceptionMapper();
        var secondMapper = new CustomExceptionMapper();
        options.Mappers.Add(firstMapper);
        options.Mappers.Add(secondMapper);

        var result = options.GetExceptionMapper(new InvalidOperationException(), null!);

        Assert.Same(firstMapper, result);
    }

    [Fact]
    public void ExcludeStatusCodes_CanBeModified()
    {
        var options = new ExceptionHandlingOptions();

        options.ExcludeStatusCodes.Add(HttpStatusCode.BadRequest);

        Assert.Contains(HttpStatusCode.BadRequest, options.ExcludeStatusCodes);
    }

    [Fact]
    public void ExcludeException_CanBeModified()
    {
        var options = new ExceptionHandlingOptions();

        options.ExcludeException.Add(typeof(ArgumentNullException));

        Assert.Contains(typeof(ArgumentNullException), options.ExcludeException);
    }

    [Fact]
    public void Mappers_CanBeModified()
    {
        var options = new ExceptionHandlingOptions();
        var mapper = new CustomExceptionMapper();

        options.Mappers.Add(mapper);

        Assert.Contains(mapper, options.Mappers);
    }

    private class CustomExceptionMapper : IExceptionMapper
    {
        public bool CanMapException(Exception exception, Http.Services.RequestDescriptor requestDescriptor) => true;
        public AspNetConventions.ExceptionHandling.Models.ExceptionDescriptor MapException(Exception exception, Http.Services.RequestDescriptor requestDescriptor)
            => new();
    }
}
