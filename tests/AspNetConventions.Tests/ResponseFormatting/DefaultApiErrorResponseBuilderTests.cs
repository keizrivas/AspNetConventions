using System.Net;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Http.Models;
using AspNetConventions.Http.Services;
using AspNetConventions.Responses.Builders;
using AspNetConventions.Responses.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AspNetConventions.Tests.ResponseFormatting;

public class DefaultApiErrorResponseBuilderTests
{
    private static readonly DefaultApiErrorResponseBuilder Builder =
        new(new AspNetConventionOptions(), NullLogger.Instance);

    private static RequestDescriptor Descriptor()
    {
        var ctx = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        return new RequestDescriptor(ctx, HttpStatusCode.BadRequest);
    }

    [Fact]
    public void BuildResponse_MapsTypeMessageAndStatusCode()
    {
        var result = new ApiResult<string>("email is required", "Validation failed", HttpStatusCode.BadRequest, "VALIDATION_ERROR");

        var response = (DefaultApiErrorResponse)Builder.BuildResponse(result, null, Descriptor());

        Assert.Equal("VALIDATION_ERROR", response.Type);
        Assert.Equal("Validation failed", response.Message);
        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void BuildResponse_ErrorsFromValue_NormalizedIntoCollection()
    {
        var errors = new List<string> { "field1 required", "field2 too long" };
        var result = new ApiResult<List<string>>(errors, null, HttpStatusCode.UnprocessableEntity, "VALIDATION_ERROR");

        var response = (DefaultApiErrorResponse)Builder.BuildResponse(result, null, Descriptor());

        Assert.Equal(2, response.Errors.Count);
    }

    [Fact]
    public void BuildResponse_WithException_DoesNotAffectShape()
    {
        var result    = new ApiResult<object?>(null, "Unexpected error", HttpStatusCode.InternalServerError, "SERVER_ERROR");
        var exception = new InvalidOperationException("boom");

        var response = (DefaultApiErrorResponse)Builder.BuildResponse(result, exception, Descriptor());

        Assert.Equal("SERVER_ERROR", response.Type);
        Assert.Equal("Unexpected error", response.Message);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void IsWrappedResponse_TrueForDefaultApiErrorResponse()
    {
        var wrapped = new DefaultApiErrorResponse(HttpStatusCode.BadRequest) { Type = "ERR" };

        Assert.True(Builder.IsWrappedResponse(wrapped));
    }

    [Fact]
    public void IsWrappedResponse_FalseForArbitraryObjects()
    {
        Assert.False(Builder.IsWrappedResponse(new DefaultApiResponse(HttpStatusCode.OK)));
        Assert.False(Builder.IsWrappedResponse("error string"));
        Assert.False(Builder.IsWrappedResponse(null));
    }
}
