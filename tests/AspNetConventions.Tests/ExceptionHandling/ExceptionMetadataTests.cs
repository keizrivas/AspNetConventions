using AspNetConventions.ExceptionHandling.Models;
using Xunit;

namespace AspNetConventions.Tests.ExceptionHandling;

public class ExceptionMetadataTests
{
    [Fact]
    public void Constructor_WithException_SetsTypeAndMessage()
    {
        var exception = new InvalidOperationException("Test error message");

        var metadata = new ExceptionMetadata(exception);

        Assert.Equal("InvalidOperationException", metadata.Type);
        Assert.Equal("Test error message", metadata.Message);
    }

    [Fact]
    public void Constructor_WithNullException_ThrowsNullReferenceException()
    {
        Assert.Throws<NullReferenceException>(() => new ExceptionMetadata(null!));
    }

    [Fact]
    public void Type_ReturnsExceptionName()
    {
        var exception = new ArgumentNullException("param");

        var metadata = new ExceptionMetadata(exception);

        Assert.Equal("ArgumentNullException", metadata.Type);
    }

    [Fact]
    public void Type_ReturnsCustomExceptionName()
    {
        var metadata = new ExceptionMetadata(new TestExceptionWithoutFullName());

        Assert.Equal("TestExceptionWithoutFullName", metadata.Type);
    }

    [Fact]
    public void Message_ReturnsExceptionMessage()
    {
        var exception = new KeyNotFoundException("Key was not found");

        var metadata = new ExceptionMetadata(exception);

        Assert.Equal("Key was not found", metadata.Message);
    }

    [Fact]
    public void Message_ReturnsEmptyForExceptionWithNoMessage()
    {
        var exception = new Exception(string.Empty);

        var metadata = new ExceptionMetadata(exception);

        Assert.Equal(string.Empty, metadata.Message);
    }

    [Fact]
    public void StackTrace_WithDefaultDepth_ReturnsFrames()
    {
        var exception = new InvalidOperationException("Test");
        try
        {
            throw exception;
        }
        catch
        {
            var metadata = new ExceptionMetadata(exception);

            Assert.NotNull(metadata.StackTrace);
            Assert.NotEmpty(metadata.StackTrace);
        }
    }

    [Fact]
    public void StackTrace_WithCustomMaxDepth_LimitsFrames()
    {
        var exception = new InvalidOperationException("Test");
        try
        {
            throw exception;
        }
        catch
        {
            var metadata = new ExceptionMetadata(exception, 1);

            Assert.NotNull(metadata.StackTrace);
            Assert.True(metadata.StackTrace.Count <= 1);
        }
    }

    [Fact]
    public void StackTrace_WithZeroDepth_ReturnsEmptyCollection()
    {
        var exception = new InvalidOperationException("Test");
        try
        {
            throw exception;
        }
        catch
        {
            var metadata = new ExceptionMetadata(exception, 0);

            Assert.NotNull(metadata.StackTrace);
            Assert.Empty(metadata.StackTrace);
        }
    }

    [Fact]
    public void StackTrace_IsReadOnly()
    {
        var exception = new Exception("Test");

        var metadata = new ExceptionMetadata(exception);

        Assert.IsAssignableFrom<IReadOnlyList<string>>(metadata.StackTrace);
    }

    [Fact]
    public void StackTrace_WithDeepException_HasMultipleFrames()
    {
        var exception = GetNestedException(3);

        var metadata = new ExceptionMetadata(exception, 10);

        Assert.NotNull(metadata.StackTrace);
    }

    [Fact]
    public void Constructor_AllowsMaxStackTraceDepth()
    {
        var exception = new Exception("Test");

        var metadataDefault = new ExceptionMetadata(exception);
        var metadataCustom = new ExceptionMetadata(exception, 5);

        Assert.NotNull(metadataDefault.StackTrace);
        Assert.NotNull(metadataCustom.StackTrace);
    }

    private static Exception GetNestedException(int depth)
    {
        if (depth == 0)
        {
            try
            {
                throw new InvalidOperationException("Deep exception");
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        try
        {
            throw new InvalidOperationException("Wrapper " + depth, GetNestedException(depth - 1));
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private class TestExceptionWithoutFullName : Exception
    {
    }
}
