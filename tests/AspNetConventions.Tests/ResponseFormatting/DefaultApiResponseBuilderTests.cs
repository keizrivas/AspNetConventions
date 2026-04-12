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

public class DefaultApiResponseBuilderTests
{
    private static readonly DefaultApiResponseBuilder Builder =
        new(new AspNetConventionOptions(), NullLogger.Instance);

    private static RequestDescriptor Descriptor()
    {
        var ctx = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        return new RequestDescriptor(ctx, HttpStatusCode.OK);
    }

    [Fact]
    public void BuildResponse_MapsDataMessageAndStatusCode()
    {
        var result = new ApiResult<string>("hello", "all good", HttpStatusCode.Created);

        var response = (DefaultApiResponse)Builder.BuildResponse(result, Descriptor());

        Assert.Equal("hello", response.Data);
        Assert.Equal("all good", response.Message);
        Assert.Equal((int)HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public void BuildResponse_WithPaginationMetadata_PassesPaginationThrough()
    {
        var pagination = new PaginationMetadata(totalRecords: 100, pageNumber: 2, pageSize: 10);
        var result     = new ApiResult<string[]>(["a", "b"]);
        result.WithPagination(pagination);

        var response = (DefaultApiResponse)Builder.BuildResponse(result, Descriptor());

        Assert.NotNull(response.Pagination);
        Assert.Equal(100, response.Pagination!.TotalRecords);
        Assert.Equal(2, response.Pagination.PageNumber);
        Assert.Equal(10, response.Pagination.PageSize);
    }

    [Fact]
    public void BuildResponse_NullData_ProducesNullDataField()
    {
        var result = new ApiResult<object?>(null, HttpStatusCode.NoContent);

        var response = (DefaultApiResponse)Builder.BuildResponse(result, Descriptor());

        Assert.Null(response.Data);
    }

    [Fact]
    public void IsWrappedResponse_TrueForDefaultApiResponse()
    {
        Assert.True(Builder.IsWrappedResponse(new DefaultApiResponse(HttpStatusCode.OK)));
    }

    [Fact]
    public void IsWrappedResponse_FalseForArbitraryObjects()
    {
        Assert.False(Builder.IsWrappedResponse("raw string"));
        Assert.False(Builder.IsWrappedResponse(new { id = 1 }));
        Assert.False(Builder.IsWrappedResponse(default));
    }
}
