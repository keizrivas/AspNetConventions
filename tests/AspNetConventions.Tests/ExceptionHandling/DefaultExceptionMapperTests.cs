using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security;
using AspNetConventions.ExceptionHandling.Mappers;
using Xunit;

namespace AspNetConventions.Tests.ExceptionHandling;

public class DefaultExceptionMapperTests
{
    private readonly DefaultExceptionMapper _mapper = new();

    public static TheoryData<Exception, HttpStatusCode, string> ExceptionMappings => new()
    {
        { new ArgumentNullException("param"),       HttpStatusCode.BadRequest,     "ARGUMENT_NULL" },
        { new ArgumentOutOfRangeException("param"), HttpStatusCode.BadRequest,     "ARGUMENT_OUT_OF_RANGE" },
        { new ArgumentException("msg"),             HttpStatusCode.BadRequest,     "INVALID_ARGUMENT" },
        { new UnauthorizedAccessException(),        HttpStatusCode.Unauthorized,   "UNAUTHORIZED" },
        { new SecurityException(),                  HttpStatusCode.Forbidden,      "FORBIDDEN" },
        { new KeyNotFoundException(),               HttpStatusCode.NotFound,       "NOT_FOUND" },
        { new FileNotFoundException(),              HttpStatusCode.NotFound,       "FILE_NOT_FOUND" },
        { new DirectoryNotFoundException(),         HttpStatusCode.NotFound,       "DIRECTORY_NOT_FOUND" },
        { new InvalidOperationException(),          HttpStatusCode.Conflict,       "INVALID_OPERATION" },
        { new ObjectDisposedException("obj"),       HttpStatusCode.Gone,           "OBJECT_DISPOSED" },
        { new NotImplementedException(),            HttpStatusCode.NotImplemented, "NOT_IMPLEMENTED" },
        { new TimeoutException(),                   HttpStatusCode.RequestTimeout, "TIMEOUT" },
        { new TaskCanceledException(),              HttpStatusCode.RequestTimeout, "REQUEST_CANCELLED" },
        { new OperationCanceledException(),         HttpStatusCode.RequestTimeout, "OPERATION_CANCELLED" },
    };

    [Theory, MemberData(nameof(ExceptionMappings))]
    public void Maps_KnownException_ToExpectedStatusAndType(
        Exception exception, HttpStatusCode expectedStatus, string expectedType)
    {
        Assert.True(_mapper.CanMapException(exception, null!));

        var descriptor = _mapper.MapException(exception, null!);

        Assert.Equal(expectedStatus, descriptor.StatusCode);
        Assert.Equal(expectedType, descriptor.Type);
    }

    [Fact]
    public void Maps_ValidationException_ExtractsErrorsByMember()
    {
        var result = new ValidationResult("Required", ["Email"]);
        var exception = new ValidationException(result, null, null);

        var descriptor = _mapper.MapException(exception, null!);

        Assert.Equal(HttpStatusCode.BadRequest, descriptor.StatusCode);
        Assert.Equal("VALIDATION_ERROR", descriptor.Type);
        Assert.NotNull(descriptor.Value);
    }

    [Fact]
    public void DoesNotMap_UnknownException_ReturnsNullStatusAndType()
    {
        var exception = new Exception("unhandled");

        Assert.False(_mapper.CanMapException(exception, null!));

        var descriptor = _mapper.MapException(exception, null!);

        Assert.Null(descriptor.StatusCode);
        Assert.Null(descriptor.Type);
    }
}
