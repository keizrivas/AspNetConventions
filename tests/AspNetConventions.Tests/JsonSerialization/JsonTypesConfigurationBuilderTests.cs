using System;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Serialization.Adapters;
using AspNetConventions.Serialization.Configuration;
using Xunit;

namespace AspNetConventions.Tests.JsonSerialization;

public class JsonTypesConfigurationBuilderTests
{
    public class PersonModel { public string Name { get; set; } = ""; public int Age { get; set; } }
    public class IgnorablePayload { public string Data { get; set; } = ""; }
    public class ModelWithPayload { public IgnorablePayload? Payload { get; set; } public string Label { get; set; } = ""; }
    public class Container<T> { public T? Value { get; set; } public string Tag { get; set; } = ""; }
    public class NonGenericModel { public string Field { get; set; } = ""; }
    public class GlobalIgnoreModel { public string Password { get; set; } = ""; public string Username { get; set; } = ""; }
    public class ScannedModel { public string Title { get; set; } = ""; public string Secret { get; set; } = ""; }

    // Discovered by ScanAssembly — ignores Secret on ScannedModel.
    public sealed class ScannedModelConfiguration : JsonTypeConfiguration<ScannedModel>
    {
        public override void Configure(IJsonTypeRuleBuilder<ScannedModel> rule)
        {
            rule.Property(x => x.Secret).Ignore();
        }
    }

    [Fact]
    public void ConfigureTypes_Type_IgnoreProperty_OmitsItFromOutput()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.Type<PersonModel>(t => t.Property(x => x.Age).Ignore())
        });

        var json = adapter.Serialize(new PersonModel { Name = "Alice", Age = 30 });

        Assert.Contains("name", json);
        Assert.DoesNotContain("age", json);
    }

    [Fact]
    public void ConfigureTypes_Type_MultipleRules_AllApplied()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.Type<PersonModel>(t =>
            {
                t.Property(x => x.Name).Name("fullName");
                t.Property(x => x.Age).Ignore();
            })
        });

        var json = adapter.Serialize(new PersonModel { Name = "Alice", Age = 30 });

        Assert.Contains("\"fullName\"", json);
        Assert.DoesNotContain("age", json);
    }

    [Fact]
    public void ConfigureTypes_OpenGenericType_NonGenericType_ThrowsInvalidOperationException()
    {
        var options = new JsonSerializationOptions
        {
            ConfigureTypes = b => b.OpenGenericType<NonGenericModel>(t => t.Property(x => x.Field).Ignore())
        };

        Assert.Throws<InvalidOperationException>(() => options.GetSerializerOptions());
    }

    [Fact]
    public void ConfigureTypes_OpenGenericType_RuleAppliesToAllClosedVariants()
    {
        // Rule registered via Container<string> must also apply to Container<int>
        // because it is stored under the open generic Container<>.
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.OpenGenericType<Container<string>>(t => t.Property(x => x.Value).Ignore())
        });

        var json = adapter.Serialize(new Container<int> { Value = 42, Tag = "test" });

        Assert.DoesNotContain("value", json);
        Assert.Contains("tag", json);
    }

    [Fact]
    public void ConfigureTypes_IgnoreType_AllPropertiesOfThatTypeOmitted()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.IgnoreType<IgnorablePayload>()
        });

        var json = adapter.Serialize(new ModelWithPayload { Payload = new IgnorablePayload { Data = "secret" }, Label = "visible" });

        Assert.DoesNotContain("payload", json);
        Assert.Contains("label", json);
    }

    [Fact]
    public void ConfigureTypes_IgnorePropertyName_CaseInsensitiveMatch_OmitsProperty()
    {
        // Rule registered as "password" (lowercase) must match CLR property "Password" (PascalCase).
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.IgnorePropertyName("password")
        });

        var json = adapter.Serialize(new GlobalIgnoreModel { Password = "hunter2", Username = "alice" });

        Assert.DoesNotContain("password", json);
        Assert.Contains("username", json);
    }

    [Fact]
    public void ConfigureTypes_IgnorePropertyName_DefaultConditionIsAlways()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.IgnorePropertyName("Password")
        });

        var json = adapter.Serialize(new GlobalIgnoreModel { Password = "s3cr3t", Username = "bob" });

        Assert.DoesNotContain("password", json);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ConfigureTypes_IgnorePropertyName_NullOrEmpty_ThrowsArgumentException(string? name)
    {
        var options = new JsonSerializationOptions
        {
            ConfigureTypes = b => b.IgnorePropertyName(name!)
        };

        Assert.ThrowsAny<ArgumentException>(() => options.GetSerializerOptions());
    }

    [Fact]
    public void ScanAssemblies_DiscoversConcrete_JsonTypeConfiguration_AndAppliesRules()
    {
        var options = new JsonSerializationOptions();
        options.ScanAssemblies(typeof(JsonTypesConfigurationBuilderTests).Assembly);
        var adapter = new SystemTextJsonAdapter(options);

        var json = adapter.Serialize(new ScannedModel { Title = "Hello", Secret = "hidden" });

        Assert.Contains("title", json);
        Assert.DoesNotContain("secret", json);
    }
}
