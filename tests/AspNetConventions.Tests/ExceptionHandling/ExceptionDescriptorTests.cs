using System.Net;
using AspNetConventions.ExceptionHandling.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AspNetConventions.Tests.ExceptionHandling;

public class ExceptionDescriptorTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var descriptor = new ExceptionDescriptor();

        Assert.Null(descriptor.StatusCode);
        Assert.Null(descriptor.Type);
        Assert.Null(descriptor.Message);
        Assert.Null(descriptor.Value);
        Assert.True(descriptor.ShouldLog);
        Assert.Equal(LogLevel.Error, descriptor.LogLevel);
        Assert.Null(descriptor.Exception);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var exception = new InvalidOperationException("Test error");
        var descriptor = new ExceptionDescriptor
        {
            StatusCode = HttpStatusCode.BadRequest,
            Type = "CUSTOM_ERROR",
            Message = "Custom error message",
            Value = new { Error = "Details" },
            ShouldLog = false,
            LogLevel = LogLevel.Warning,
            Exception = exception
        };

        Assert.Equal(HttpStatusCode.BadRequest, descriptor.StatusCode);
        Assert.Equal("CUSTOM_ERROR", descriptor.Type);
        Assert.Equal("Custom error message", descriptor.Message);
        Assert.NotNull(descriptor.Value);
        Assert.False(descriptor.ShouldLog);
        Assert.Equal(LogLevel.Warning, descriptor.LogLevel);
        Assert.Same(exception, descriptor.Exception);
    }

    [Fact]
    public void ExceptionDescriptorOfT_InheritsFromBase()
    {
        var descriptor = new ExceptionDescriptor<string>
        {
            StatusCode = HttpStatusCode.NotFound,
            Type = "NOT_FOUND",
            Message = "Resource not found",
            Value = "Specific value"
        };

        Assert.Equal(HttpStatusCode.NotFound, descriptor.StatusCode);
        Assert.Equal("NOT_FOUND", descriptor.Type);
        Assert.Equal("Specific value", descriptor.Value);
    }

    [Fact]
    public void ExceptionDescriptorOfT_ValueHidesBaseProperty()
    {
        var descriptor = new ExceptionDescriptor<CustomError>
        {
            Value = new CustomError { Code = 123 }
        };

        Assert.NotNull(descriptor.Value);
        Assert.IsType<CustomError>(descriptor.Value);
        Assert.Equal(123, ((CustomError)descriptor.Value).Code);
    }

    [Fact]
    public void StatusCode_CanBeNull()
    {
        var descriptor = new ExceptionDescriptor();

        Assert.Null(descriptor.StatusCode);
    }

    [Fact]
    public void StatusCode_CanBeSetToVariousCodes()
    {
        var codes = new[]
        {
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError
        };

        foreach (var code in codes)
        {
            var descriptor = new ExceptionDescriptor { StatusCode = code };
            Assert.Equal(code, descriptor.StatusCode);
        }
    }

    [Fact]
    public void ShouldLog_DefaultIsTrue()
    {
        var descriptor = new ExceptionDescriptor();
        Assert.True(descriptor.ShouldLog);
    }

    [Fact]
    public void LogLevel_DefaultIsError()
    {
        var descriptor = new ExceptionDescriptor();
        Assert.Equal(LogLevel.Error, descriptor.LogLevel);
    }

    [Fact]
    public void Value_CanBeAnyObject()
    {
        var descriptor = new ExceptionDescriptor();

        descriptor.Value = "string";
        Assert.Equal("string", descriptor.Value);

        descriptor.Value = 42;
        Assert.Equal(42, descriptor.Value);

        descriptor.Value = new[] { "a", "b", "c" };
        Assert.Equal(3, ((string[])descriptor.Value).Length);

        descriptor.Value = null;
        Assert.Null(descriptor.Value);
    }

    private class CustomError
    {
        public int Code { get; set; }
    }
}
