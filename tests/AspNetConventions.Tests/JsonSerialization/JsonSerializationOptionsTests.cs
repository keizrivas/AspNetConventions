using System;
using System.Text.Json;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Enums;
using AspNetConventions.Core.Enums.Json;
using AspNetConventions.Serialization.Adapters;
using Xunit;

namespace AspNetConventions.Tests.JsonSerialization;

public class JsonSerializationOptionsTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var options = new JsonSerializationOptions();

        Assert.True(options.IsEnabled);
        Assert.Equal(CasingStyle.CamelCase, options.CaseStyle);
        Assert.Null(options.CaseConverter);
        Assert.Null(options.ConfigureTypes);
        Assert.Equal(IgnoreCondition.Never, options.IgnoreCondition);
        Assert.False(options.CaseInsensitive);
        Assert.False(options.WriteIndented);
        Assert.Equal(0, options.MaxDepth);
        Assert.False(options.AllowTrailingCommas);
        Assert.Equal(NumberHandling.Strict, options.NumberHandling);
        Assert.True(options.UseStringEnumConverter);
        Assert.Empty(options.Converters);
        Assert.NotNull(options.Hooks);
    }

    [Fact]
    public void GetSerializerAdapter_NoCustomAdapter_ReturnsSystemTextJsonAdapter()
    {
        var options = new JsonSerializationOptions();

        Assert.IsType<SystemTextJsonAdapter>(options.GetSerializerAdapter());
    }

    [Fact]
    public void GetSerializerAdapter_CalledTwice_ReturnsSameInstance()
    {
        var options = new JsonSerializationOptions();

        Assert.Same(options.GetSerializerAdapter(), options.GetSerializerAdapter());
    }

    [Fact]
    public void GetSerializerOptions_ReturnsJsonSerializerOptions()
    {
        var options = new JsonSerializationOptions();

        Assert.IsType<JsonSerializerOptions>(options.GetSerializerOptions());
    }

    [Fact]
    public void ScanAssemblies_NullArgument_Throws()
    {
        var options = new JsonSerializationOptions();

        Assert.Throws<ArgumentNullException>(() => options.ScanAssemblies(null!));
    }

    [Fact]
    public void Clone_BasicProperties_AreIndependent()
    {
        var options = new JsonSerializationOptions
        {
            IsEnabled = false,
            CaseStyle = CasingStyle.SnakeCase,
            WriteIndented = true,
            CaseInsensitive = true,
        };

        var clone = (JsonSerializationOptions)options.Clone();
        options.IsEnabled = true;
        options.CaseStyle = CasingStyle.PascalCase;

        Assert.False(clone.IsEnabled);
        Assert.Equal(CasingStyle.SnakeCase, clone.CaseStyle);
        Assert.True(clone.WriteIndented);
        Assert.True(clone.CaseInsensitive);
    }

    [Fact]
    public void Clone_Hooks_IsDeepCloned()
    {
        var options = new JsonSerializationOptions();
        options.Hooks.ShouldSerializeType = _ => true;

        var clone = (JsonSerializationOptions)options.Clone();
        options.Hooks.ShouldSerializeType = null;

        Assert.NotNull(clone.Hooks.ShouldSerializeType);
    }
}
