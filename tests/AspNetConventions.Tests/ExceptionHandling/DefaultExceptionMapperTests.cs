using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security;
using AspNetConventions.ExceptionHandling.Mappers;
using Xunit;

namespace AspNetConventions.Tests.ExceptionHandling;

public class DefaultExceptionMapperTests
{
    private readonly DefaultExceptionMapper _mapper = new();

    [Fact]
    public void CanMapException_ForArgumentNullException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new ArgumentNullException("param"), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForArgumentOutOfRangeException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new ArgumentOutOfRangeException("param"), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForArgumentException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new ArgumentException("message"), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForUnauthorizedAccessException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new UnauthorizedAccessException(), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForSecurityException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new SecurityException(), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForKeyNotFoundException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new KeyNotFoundException(), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForFileNotFoundException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new FileNotFoundException("file"), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForDirectoryNotFoundException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new DirectoryNotFoundException(), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForInvalidOperationException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new InvalidOperationException(), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForObjectDisposedException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new ObjectDisposedException("obj"), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForNotImplementedException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new NotImplementedException(), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForTimeoutException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new TimeoutException(), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForTaskCanceledException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new TaskCanceledException(), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForOperationCanceledException_ReturnsTrue()
    {
        var result = _mapper.CanMapException(new OperationCanceledException(), null!);

        Assert.True(result);
    }

    [Fact]
    public void CanMapException_ForUnhandledException_ReturnsFalse()
    {
        var result = _mapper.CanMapException(new Exception("unknown"), null!);

        Assert.False(result);
    }

    [Fact]
    public void MapException_ArgumentNullException_ReturnsBadRequest()
    {
        var result = _mapper.MapException(new ArgumentNullException("param"), null!);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("ARGUMENT_NULL", result.Type);
    }

    [Fact]
    public void MapException_ArgumentOutOfRangeException_ReturnsBadRequest()
    {
        var result = _mapper.MapException(new ArgumentOutOfRangeException("param"), null!);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("ARGUMENT_OUT_OF_RANGE", result.Type);
    }

    [Fact]
    public void MapException_ArgumentException_ReturnsBadRequest()
    {
        var result = _mapper.MapException(new ArgumentException("message"), null!);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("INVALID_ARGUMENT", result.Type);
    }

    [Fact]
    public void MapException_UnauthorizedAccessException_ReturnsUnauthorized()
    {
        var result = _mapper.MapException(new UnauthorizedAccessException(), null!);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        Assert.Equal("UNAUTHORIZED", result.Type);
    }

    [Fact]
    public void MapException_SecurityException_ReturnsForbidden()
    {
        var result = _mapper.MapException(new SecurityException(), null!);

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Equal("FORBIDDEN", result.Type);
    }

    [Fact]
    public void MapException_KeyNotFoundException_ReturnsNotFound()
    {
        var result = _mapper.MapException(new KeyNotFoundException(), null!);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("NOT_FOUND", result.Type);
    }

    [Fact]
    public void MapException_InvalidOperationException_ReturnsConflict()
    {
        var result = _mapper.MapException(new InvalidOperationException(), null!);

        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal("INVALID_OPERATION", result.Type);
    }

    [Fact]
    public void MapException_NotImplementedException_ReturnsNotImplemented()
    {
        var result = _mapper.MapException(new NotImplementedException(), null!);

        Assert.Equal(HttpStatusCode.NotImplemented, result.StatusCode);
        Assert.Equal("NOT_IMPLEMENTED", result.Type);
    }

    [Fact]
    public void MapException_TimeoutException_ReturnsRequestTimeout()
    {
        var result = _mapper.MapException(new TimeoutException(), null!);

        Assert.Equal(HttpStatusCode.RequestTimeout, result.StatusCode);
        Assert.Equal("TIMEOUT", result.Type);
    }

    [Fact]
    public void MapException_UnknownException_ReturnsNullStatusCode()
    {
        var result = _mapper.MapException(new Exception("unknown"), null!);

        Assert.Null(result.StatusCode);
        Assert.Null(result.Type);
    }

    [Fact]
    public void MapException_ValidationException_WithErrors_ReturnsValidationErrors()
    {
        var validationResult = new ValidationResult("Error message", ["PropertyName"]);
        var validationException = new ValidationException(validationResult, null, null);

        var result = _mapper.MapException(validationException, null!);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("VALIDATION_ERROR", result.Type);
    }
}
