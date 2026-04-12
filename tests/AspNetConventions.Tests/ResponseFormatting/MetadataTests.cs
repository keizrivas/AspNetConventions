using AspNetConventions.ExceptionHandling.Models;
using AspNetConventions.Http.Models;
using Xunit;

namespace AspNetConventions.Tests.ResponseFormatting;

public class MetadataTests
{
    [Fact]
    public void StandardProperties_StoredUnderExpectedKeys()
    {
        var metadata = new Metadata
        {
            [Metadata.TraceIdKey]     = "abc",
            [Metadata.PathKey]        = "/api/users",
            [Metadata.RequestTypeKey] = "GET",
        };

        Assert.Equal("abc", metadata[Metadata.TraceIdKey]);
        Assert.Equal("/api/users", metadata[Metadata.PathKey]);
        Assert.Equal("GET", metadata[Metadata.RequestTypeKey]);
    }

    [Fact]
    public void Timestamp_StoredAndRetrievedAsDateTime()
    {
        var now      = DateTime.UtcNow;
        var metadata = new Metadata { [Metadata.TimestampKey] = now };

        Assert.Equal(now, metadata[Metadata.TimestampKey]);
    }

    [Theory]
    [InlineData(Metadata.TraceIdKey)]
    [InlineData(Metadata.PathKey)]
    [InlineData(Metadata.RequestTypeKey)]
    [InlineData(Metadata.TimestampKey)]
    [InlineData(Metadata.ExceptionKey)]
    public void Remove_BuiltInKey_DisappearsFromDictionary(string key)
    {
        var metadata = new Metadata
        {
            [Metadata.TraceIdKey]     = "x",
            [Metadata.PathKey]        = "/x",
            [Metadata.RequestTypeKey] = "GET",
            [Metadata.TimestampKey]   = DateTime.UtcNow,
            [Metadata.ExceptionKey]   = new ExceptionMetadata(new Exception("e")),
        };

        metadata.Remove(key);

        Assert.False(metadata.ContainsKey(key));
    }

    [Fact]
    public void CustomEntry_AddedViaIndexer_AppearsInDictionary()
    {
        var metadata = new Metadata();
        metadata["correlationId"] = "corr-123";
        metadata["tenant"]        = "acme";

        Assert.Equal("corr-123", metadata["correlationId"]);
        Assert.Equal("acme", metadata["tenant"]);
    }

    [Fact]
    public void CustomEntry_Remove_DisappearsFromDictionary()
    {
        var metadata = new Metadata { ["correlationId"] = "corr-123" };

        metadata.Remove("correlationId");

        Assert.False(metadata.ContainsKey("correlationId"));
    }

    [Fact]
    public void CustomizeMetadata_CanRemoveBuiltInAndAddCustomEntries()
    {
        var metadata = new Metadata
        {
            [Metadata.TraceIdKey] = "trace-abc",
            [Metadata.PathKey]    = "/api/orders",
        };

        metadata.Remove(Metadata.TraceIdKey);
        metadata["correlationId"] = "corr-xyz";

        Assert.False(metadata.ContainsKey(Metadata.TraceIdKey));
        Assert.Equal("corr-xyz", metadata["correlationId"]);
        Assert.Equal("/api/orders", metadata[Metadata.PathKey]);
    }
}
