using System.Net;
using AspNetConventions.Http;
using AspNetConventions.Http.Models;
using AspNetConventions.Responses.Models;
using Xunit;

namespace AspNetConventions.Tests.ResponseFormatting;

public class ResponseFormattingTests
{
    [Theory]
    [InlineData(HttpStatusCode.OK,                  true,  "SUCCESS")]
    [InlineData(HttpStatusCode.Created,             true,  "SUCCESS")]
    [InlineData(HttpStatusCode.NoContent,           true,  "SUCCESS")]
    [InlineData(HttpStatusCode.BadRequest,          false, "CLIENT_ERROR")]
    [InlineData(HttpStatusCode.NotFound,            false, "CLIENT_ERROR")]
    [InlineData(HttpStatusCode.InternalServerError, false, "SERVER_ERROR")]
    public void ApiResult_IsSuccess_And_Type_DerivedFromStatusCode(HttpStatusCode code, bool isSuccess, string expectedType)
    {
        var result = new ApiResult<string>("value", code);

        Assert.Equal(isSuccess, result.IsSuccess);
        Assert.Equal(expectedType, result.Type);
    }

    [Fact]
    public void ApiResult_CustomType_OverridesStatusCodeDerivedType()
    {
        var result = new ApiResult<object?>(null, null, HttpStatusCode.BadRequest, "DOMAIN_ERROR");

        Assert.Equal("DOMAIN_ERROR", result.Type);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public void ApiResults_Factory_ProducesCorrectStatusCode(HttpStatusCode expected)
    {
        var result = ApiResults.Custom<object?>(null, expected);

        Assert.Equal(expected, result.StatusCode);
    }

    [Fact]
    public void ApiResults_Paginate_AttachesPaginationToCollectionResult()
    {
        var items = new[] { "a", "b", "c" };
        var result = ApiResults.Paginate(items, totalRecords: 50, pageNumber: 2, pageSize: 3);
        var collection = (CollectionResult<string>)result.GetValue()!;

        Assert.Equal(50, collection.TotalRecords);
        Assert.Equal(2, collection.PageNumber);
        Assert.Equal(3, collection.PageSize);
        Assert.Equal(3, collection.Count);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK,         "Success")]
    [InlineData(HttpStatusCode.Created,    "Success")]
    [InlineData(HttpStatusCode.BadRequest, "Failure")]
    [InlineData(HttpStatusCode.NotFound,   "Failure")]
    public void DefaultApiResponse_Status_ReflectsStatusCode(HttpStatusCode code, string expectedStatus)
    {
        var response = new DefaultApiResponse(code);

        Assert.Equal(expectedStatus, response.Status.ToString());
        Assert.Equal((int)code, response.StatusCode);
    }

    [Fact]
    public void DefaultApiResponse_Data_And_Message_ArePreserved()
    {
        var response = new DefaultApiResponse(HttpStatusCode.OK)
        {
            Data    = new { id = 1 },
            Message = "done"
        };

        Assert.NotNull(response.Data);
        Assert.Equal("done", response.Message);
    }

    [Fact]
    public void DefaultApiErrorResponse_NullErrors_ProducesEmptyCollection()
    {
        var response = new DefaultApiErrorResponse(HttpStatusCode.InternalServerError) { Type = "ERR" };

        Assert.Empty(response.Errors);
    }

    [Fact]
    public void DefaultApiErrorResponse_SingleObject_WrappedInCollection()
    {
        var response = new DefaultApiErrorResponse(HttpStatusCode.BadRequest, "invalid input") { Type = "ERR" };

        Assert.Single(response.Errors);
    }

    [Fact]
    public void DefaultApiErrorResponse_List_ExpandedIntoErrors()
    {
        var errors = new List<string> { "field1 required", "field2 too long" };
        var response = new DefaultApiErrorResponse(HttpStatusCode.BadRequest, errors) { Type = "ERR" };

        Assert.Equal(2, response.Errors.Count);
    }

    [Fact]
    public void DefaultApiErrorResponse_Dictionary_TreatedAsSingleError()
    {
        var errors = new Dictionary<string, string[]> { ["name"] = ["required"] };
        var response = new DefaultApiErrorResponse(HttpStatusCode.BadRequest, errors) { Type = "ERR" };

        Assert.Single(response.Errors);
    }

    [Fact]
    public void DefaultApiErrorResponse_ReadOnlyCollection_UsedDirectly()
    {
        IReadOnlyCollection<object> errors = new[] { (object)"e1", "e2", "e3" };
        var response = new DefaultApiErrorResponse(HttpStatusCode.BadRequest, errors) { Type = "ERR" };

        Assert.Equal(3, response.Errors.Count);
    }
}
