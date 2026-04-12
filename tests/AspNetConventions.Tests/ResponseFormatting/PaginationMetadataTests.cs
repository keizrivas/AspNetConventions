using AspNetConventions.Http.Models;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace AspNetConventions.Tests.ResponseFormatting;

public class PaginationMetadataTests
{
    [Theory]
    [InlineData(50, 10, 5)]   // exact division
    [InlineData(51, 10, 6)]   // remainder adds one page
    [InlineData(1,  10, 1)]   // single item
    [InlineData(0,  10, 0)]   // no records → no pages
    public void TotalPages_CalculatedFromTotalRecordsAndPageSize(int total, int pageSize, int expectedPages)
    {
        var metadata = new PaginationMetadata(total, pageNumber: 1, pageSize);

        Assert.Equal(expectedPages, metadata.TotalPages);
    }

    [Fact]
    public void PageNumber_BelowOne_NormalizedToOne()
    {
        var metadata = new PaginationMetadata(totalRecords: 10, pageNumber: 0, pageSize: 10);

        Assert.Equal(1, metadata.PageNumber);
    }

    [Fact]
    public void BuildLinks_FirstPage_HasNopreviousPageAndNextPagePresent()
    {
        var metadata = new PaginationMetadata(totalRecords: 30, pageNumber: 1, pageSize: 10);
        metadata.BuildLinks(CreateHttpContext(), "pageSize", "page");

        Assert.NotNull(metadata.Links!.FirstPageUrl);
        Assert.NotNull(metadata.Links.LastPageUrl);
        Assert.NotNull(metadata.Links.NextPageUrl);
        Assert.Null(metadata.Links.PreviousPageUrl);
    }

    [Fact]
    public void BuildLinks_LastPage_HasNoPreviousNextAndPreviousPagePresent()
    {
        var metadata = new PaginationMetadata(totalRecords: 30, pageNumber: 3, pageSize: 10);
        metadata.BuildLinks(CreateHttpContext(), "pageSize", "page");

        Assert.NotNull(metadata.Links!.PreviousPageUrl);
        Assert.Null(metadata.Links.NextPageUrl);
    }

    [Fact]
    public void BuildLinks_MiddlePage_HasBothNextAndPreviousLinks()
    {
        var metadata = new PaginationMetadata(totalRecords: 50, pageNumber: 3, pageSize: 10);
        metadata.BuildLinks(CreateHttpContext(), "pageSize", "page");

        Assert.NotNull(metadata.Links!.NextPageUrl);
        Assert.NotNull(metadata.Links.PreviousPageUrl);
    }

    [Fact]
    public void BuildLinks_PreservesExistingQueryParameters()
    {
        var context = CreateHttpContext("/api/users", query: "?search=foo");
        var metadata = new PaginationMetadata(totalRecords: 20, pageNumber: 1, pageSize: 10);
        metadata.BuildLinks(context, "pageSize", "page");

        Assert.Contains("search=foo", metadata.Links!.NextPageUrl!.Query);
    }

    [Fact]
    public void BuildLinks_NullContext_Throws()
    {
        var metadata = new PaginationMetadata(10, 1, 10);

        Assert.Throws<ArgumentNullException>(() => metadata.BuildLinks(null!, "pageSize", "page"));
    }

    [Fact]
    public void CollectionResult_NullItems_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CollectionResult<string>(null!, totalRecords: 0));
    }

    [Fact]
    public void CollectionResult_PageNumber_NormalizedToMinimumOne()
    {
        var result = new CollectionResult<int>([], totalRecords: 0, pageNumber: 0, pageSize: 10);

        Assert.Equal(1, result.PageNumber);
    }

    private static HttpContext CreateHttpContext(string path = "/api/items", string query = "")
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = path;
        context.Request.QueryString = new QueryString(query);
        return context;
    }
}
