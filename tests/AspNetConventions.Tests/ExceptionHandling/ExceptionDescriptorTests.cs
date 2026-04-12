using System.Net;
using AspNetConventions.ExceptionHandling.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AspNetConventions.Tests.ExceptionHandling;

public class ExceptionDescriptorTests
{
    [Fact]
    public void Defaults_NullablePropertiesAreNull_ShouldLog_True_LogLevel_Error()
    {
        var descriptor = new ExceptionDescriptor();

        Assert.Null(descriptor.StatusCode);
        Assert.Null(descriptor.Type);
        Assert.Null(descriptor.Message);
        Assert.Null(descriptor.Value);
        Assert.Null(descriptor.Exception);
        Assert.True(descriptor.ShouldLog);
        Assert.Equal(LogLevel.Error, descriptor.LogLevel);
    }

    [Fact]
    public void GenericVariant_ExposesStronglyTypedValue()
    {
        var descriptor = new ExceptionDescriptor<ValidationErrors>
        {
            StatusCode = HttpStatusCode.BadRequest,
            Type = "VALIDATION_ERROR",
            Value = new ValidationErrors { Field = "email", Message = "Required" }
        };

        Assert.Equal(HttpStatusCode.BadRequest, descriptor.StatusCode);
        Assert.IsType<ValidationErrors>(descriptor.Value);
        Assert.Equal("email", descriptor.Value.Field);
    }

    private record ValidationErrors
    {
        public string Field { get; init; } = "";
        public string Message { get; init; } = "";
    }
}
