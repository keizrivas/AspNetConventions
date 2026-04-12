using AspNetConventions.ExceptionHandling.Models;
using Xunit;

namespace AspNetConventions.Tests.ExceptionHandling;

public class ExceptionMetadataTests
{
    [Fact]
    public void CapturesTypeName_And_Message_FromException()
    {
        var exception = new InvalidOperationException("State is invalid");

        var metadata = new ExceptionMetadata(exception);

        Assert.Equal("InvalidOperationException", metadata.Type);
        Assert.Equal("State is invalid", metadata.Message);
    }

    [Fact]
    public void StackTrace_RespectsMaxDepth_And_ZeroDepthReturnsEmpty()
    {
        Exception exception;
        try { throw new InvalidOperationException("test"); }
        catch (Exception ex) { exception = ex; }

        var limited = new ExceptionMetadata(exception, 1);
        var none    = new ExceptionMetadata(exception, 0);

        Assert.True(limited.StackTrace.Count <= 1);
        Assert.Empty(none.StackTrace);
    }

    [Fact]
    public void NullException_Throws()
    {
        Assert.Throws<NullReferenceException>(() => new ExceptionMetadata(null!));
    }
}
