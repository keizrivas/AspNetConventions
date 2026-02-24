using System.Net;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Configuration.Options.Response;
using Xunit;

namespace AspNetConventions.Tests.ResponseFormatting;

public class ResponseFormattingOptionsTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var options = new ResponseFormattingOptions();

        Assert.True(options.IsEnabled);
        Assert.True(options.IncludeMetadata);
        Assert.NotNull(options.Pagination);
        Assert.NotNull(options.ErrorResponse);
        Assert.NotNull(options.CollectionResultAdapters);
        Assert.Null(options.ResponseBuilder);
        Assert.Null(options.ErrorResponseBuilder);
        Assert.NotNull(options.Hooks);
    }

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var options = new ResponseFormattingOptions
        {
            IsEnabled = false,
            IncludeMetadata = false,
            Pagination = new PaginationOptions
            {
                IncludeMetadata = false,
                IncludeLinks = false,
                PageNumberParameterName = "p",
                PageSizeParameterName = "size",
                DefaultPageSize = 10
            },
            ErrorResponse = new ErrorResponseOptions
            {
                DefaultStatusCode = HttpStatusCode.BadRequest,
                DefaultErrorType = "CUSTOM_ERROR",
                DefaultErrorMessage = "Custom error message",
                MaxStackTraceDepth = 50
            }
        };

        var cloned = (ResponseFormattingOptions)options.Clone();

        Assert.False(cloned.IsEnabled);
        Assert.False(cloned.IncludeMetadata);
        Assert.False(cloned.Pagination.IncludeMetadata);
        Assert.False(cloned.Pagination.IncludeLinks);
        Assert.Equal("p", cloned.Pagination.PageNumberParameterName);
        Assert.Equal("size", cloned.Pagination.PageSizeParameterName);
        Assert.Equal(10, cloned.Pagination.DefaultPageSize);
        Assert.Equal(HttpStatusCode.BadRequest, cloned.ErrorResponse.DefaultStatusCode);
        Assert.Equal("CUSTOM_ERROR", cloned.ErrorResponse.DefaultErrorType);
    }

    [Fact]
    public void Pagination_DefaultValues_AreSetCorrectly()
    {
        var pagination = new PaginationOptions();

        Assert.True(pagination.IncludeMetadata);
        Assert.True(pagination.IncludeLinks);
        Assert.Equal("page", pagination.PageNumberParameterName);
        Assert.Equal("pageSize", pagination.PageSizeParameterName);
        Assert.Equal(20, pagination.DefaultPageSize);
    }

    [Fact]
    public void ErrorResponse_DefaultValues_AreSetCorrectly()
    {
        var errorResponse = new ErrorResponseOptions();

        Assert.Equal(HttpStatusCode.InternalServerError, errorResponse.DefaultStatusCode);
        Assert.Equal("UNEXPECTED_ERROR", errorResponse.DefaultErrorType);
        Assert.Equal("An unexpected error occurred.", errorResponse.DefaultErrorMessage);
        Assert.Equal("VALIDATION_ERROR", errorResponse.DefaultValidationType);
        Assert.Equal("One or more validation errors occurred.", errorResponse.DefaultValidationMessage);
        Assert.Null(errorResponse.IncludeExceptionDetails);
        Assert.Equal(25, errorResponse.MaxStackTraceDepth);
    }

    [Fact]
    public void AllowedProblemDetailsExtensions_CanBeModified()
    {
        var errorResponse = new ErrorResponseOptions();

        errorResponse.AllowedProblemDetailsExtensions.Add("custom");

        Assert.Contains("custom", errorResponse.AllowedProblemDetailsExtensions);
    }
}
