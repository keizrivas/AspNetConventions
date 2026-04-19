using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetConventions.Configuration.Options;
using AspNetConventions.Core.Enums;
using AspNetConventions.Core.Enums.Json;
using AspNetConventions.Core.Hooks;
using AspNetConventions.Serialization.Adapters;
using Xunit;

namespace AspNetConventions.Tests.JsonSerialization;

public class SystemTextJsonAdapterTests
{
    // Each record type is unique to prevent static resolver cache collisions across tests.
    private record CamelCaseRecord(string FirstName, int AgeYears);
    private record PascalCaseRecord(string FirstName, int AgeYears);
    private record SnakeCaseRecord(string FirstName, int AgeYears);
    private record KebabCaseRecord(string FirstName, int AgeYears);
    private record WriteIndentedRecord(string Title);
    private record CaseInsensitiveRecord(string Name);
    private record TrailingCommaRecord(string City);
    private record GlobalNullIgnoreRecord(string? Optional, string Required);
    private record GlobalDefaultIgnoreRecord(int Counter, string Label);
    private record AlwaysIgnoreRecord(string HiddenProp, string VisibleProp);
    private record WhenNullIgnoreRecord(string? MaybeNull, string Definite);
    private record NameOverrideRecord(string InternalName);
    private record OrderRecord(string C, string A, string B);
    private enum Color { Red, Green, Blue }
    private record EnumStringRecord(Color Shade);
    private record EnumNumberRecord(Color Shade);
    private record NumberAsStringRecord(int Quantity);
    private sealed class SensitivePayload { public string Data { get; set; } = ""; }
    private record TypeIgnoreRecord(SensitivePayload Secret, string Name);
    private record GlobalNameIgnoreRecord(string Password, string UserName);
    private record ShouldSerializeTypeRecord(string Classified, string Public);
    private record ResolvePropertyNameRecord(string OriginalKey);
    private record ShouldSerializePropertyRecord(string Name, string? SkipMe);
    private record OnTypeResolvedRecord(string Alpha, string Beta);
    private record RoundtripRecord(string Name, int Value);

    [Fact]
    public void Serialize_CamelCase_LowercasesFirstLetter()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions());
        var json = adapter.Serialize(new CamelCaseRecord("John", 30));

        Assert.Contains("\"firstName\"", json);
        Assert.Contains("\"ageYears\"", json);
    }

    [Fact]
    public void Serialize_PascalCase_PreservesUppercaseFirstLetter()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions { CaseStyle = CasingStyle.PascalCase });
        var json = adapter.Serialize(new PascalCaseRecord("John", 30));

        Assert.Contains("\"FirstName\"", json);
        Assert.Contains("\"AgeYears\"", json);
    }

    [Fact]
    public void Serialize_SnakeCase_InsertsUnderscores()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions { CaseStyle = CasingStyle.SnakeCase });
        var json = adapter.Serialize(new SnakeCaseRecord("John", 30));

        Assert.Contains("\"first_name\"", json);
        Assert.Contains("\"age_years\"", json);
    }

    [Fact]
    public void Serialize_KebabCase_InsertsHyphens()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions { CaseStyle = CasingStyle.KebabCase });
        var json = adapter.Serialize(new KebabCaseRecord("John", 30));

        Assert.Contains("\"first-name\"", json);
        Assert.Contains("\"age-years\"", json);
    }

    [Fact]
    public void Serialize_WriteIndented_FormatsWithNewlines()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions { WriteIndented = true });
        var json = adapter.Serialize(new WriteIndentedRecord("Hello"));

        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void Deserialize_CaseInsensitive_MatchesPropertiesIgnoringCase()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions { CaseInsensitive = true });
        var result = adapter.Deserialize<CaseInsensitiveRecord>("{\"NAME\":\"Alice\"}");

        Assert.NotNull(result);
        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public void Deserialize_AllowTrailingCommas_ParsesSuccessfully()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions { AllowTrailingCommas = true });
        var result = adapter.Deserialize<TrailingCommaRecord>("{\"city\":\"London\",}");

        Assert.NotNull(result);
        Assert.Equal("London", result.City);
    }

    [Fact]
    public void Deserialize_TrailingCommas_WithoutFlag_ThrowsJsonException()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions());

        Assert.Throws<JsonException>(() => adapter.Deserialize<TrailingCommaRecord>("{\"city\":\"London\",}"));
    }

    [Fact]
    public void Serialize_GlobalIgnoreConditionWhenWritingNull_OmitsNullProperties()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            IgnoreCondition = IgnoreCondition.WhenWritingNull
        });
        var json = adapter.Serialize(new GlobalNullIgnoreRecord(null, "present"));

        Assert.DoesNotContain("optional", json);
        Assert.Contains("required", json);
    }

    [Fact]
    public void Serialize_GlobalIgnoreConditionWhenWritingDefault_OmitsDefaultValues()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            IgnoreCondition = IgnoreCondition.WhenWritingDefault
        });
        var json = adapter.Serialize(new GlobalDefaultIgnoreRecord(0, "present"));

        Assert.DoesNotContain("counter", json);
        Assert.Contains("label", json);
    }

    [Fact]
    public void Serialize_UseStringEnumConverter_SerializesEnumAsString()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions { UseStringEnumConverter = true });
        var json = adapter.Serialize(new EnumStringRecord(Color.Green));

        Assert.Contains("\"green\"", json);
    }

    [Fact]
    public void Serialize_UseStringEnumConverterFalse_SerializesEnumAsNumber()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions { UseStringEnumConverter = false });
        var json = adapter.Serialize(new EnumNumberRecord(Color.Blue));

        Assert.Contains("2", json);
        Assert.DoesNotContain("\"blue\"", json);
    }

    [Fact]
    public void Serialize_NumberHandlingWriteAsString_SerializesNumbersAsJsonStrings()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            NumberHandling = NumberHandling.WriteAsString
        });
        var json = adapter.Serialize(new NumberAsStringRecord(42));

        Assert.Contains("\"42\"", json);
    }

    [Fact]
    public void Serialize_ConfigureTypes_IgnoreAlways_OmitsProperty()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.Type<AlwaysIgnoreRecord>(t =>
                t.Property(x => x.HiddenProp).Ignore(IgnoreCondition.Always))
        });
        var json = adapter.Serialize(new AlwaysIgnoreRecord("secret", "public"));

        Assert.DoesNotContain("hiddenProp", json);
        Assert.Contains("visibleProp", json);
    }

    [Fact]
    public void Serialize_ConfigureTypes_IgnoreWhenWritingNull_OmitsNullProperty_KeepsNonNull()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.Type<WhenNullIgnoreRecord>(t =>
                t.Property(x => x.MaybeNull).Ignore(IgnoreCondition.WhenWritingNull))
        });
        var jsonWithNull = adapter.Serialize(new WhenNullIgnoreRecord(null, "here"));
        var jsonWithValue = adapter.Serialize(new WhenNullIgnoreRecord("value", "here"));

        Assert.DoesNotContain("maybeNull", jsonWithNull);
        Assert.Contains("maybeNull", jsonWithValue);
    }

    [Fact]
    public void Serialize_ConfigureTypes_NameOverride_UsesCustomJsonKey()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.Type<NameOverrideRecord>(t =>
                t.Property(x => x.InternalName).Name("alias"))
        });
        var json = adapter.Serialize(new NameOverrideRecord("value"));

        Assert.Contains("\"alias\"", json);
        Assert.DoesNotContain("internalName", json);
    }

    [Fact]
    public void Serialize_ConfigureTypes_Order_PropertiesAppearInSpecifiedSequence()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.Type<OrderRecord>(t =>
            {
                t.Property(x => x.C).Order(3);
                t.Property(x => x.A).Order(1);
                t.Property(x => x.B).Order(2);
            })
        });
        var json = adapter.Serialize(new OrderRecord("third", "first", "second"));

        var posA = json.IndexOf("\"a\"", StringComparison.Ordinal);
        var posB = json.IndexOf("\"b\"", StringComparison.Ordinal);
        var posC = json.IndexOf("\"c\"", StringComparison.Ordinal);

        Assert.True(posA < posB && posB < posC);
    }

    [Fact]
    public void Serialize_ConfigureTypes_IgnoreType_OmitsAllPropertiesOfThatType()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.IgnoreType<SensitivePayload>()
        });
        var json = adapter.Serialize(new TypeIgnoreRecord(new SensitivePayload { Data = "private" }, "John"));

        Assert.DoesNotContain("secret", json);
        Assert.Contains("name", json);
    }

    [Fact]
    public void Serialize_ConfigureTypes_GlobalIgnorePropertyName_OmitsMatchingPropertiesAcrossTypes()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            ConfigureTypes = b => b.IgnorePropertyName("Password")
        });
        var json = adapter.Serialize(new GlobalNameIgnoreRecord("hunter2", "alice"));

        Assert.DoesNotContain("password", json);
        Assert.Contains("userName", json);
    }

    [Fact]
    public void Serialize_HookShouldSerializeType_ReturnsFalse_SkipsConfiguredRules()
    {
        var optionsWithHook = new JsonSerializationOptions
        {
            ConfigureTypes = b => b.Type<ShouldSerializeTypeRecord>(t =>
                t.Property(x => x.Classified).Ignore(IgnoreCondition.Always)),
            Hooks = new JsonSerializationHooks { ShouldSerializeType = _ => false }
        };
        var json = new SystemTextJsonAdapter(optionsWithHook)
            .Serialize(new ShouldSerializeTypeRecord("hidden", "visible"));

        // Rules were skipped because the hook returned false — Classified must be present.
        Assert.Contains("classified", json);
    }

    [Fact]
    public void Serialize_HookResolvePropertyName_OverridesJsonKey()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            Hooks = new JsonSerializationHooks
            {
                ResolvePropertyName = (clrName, _) => clrName == "OriginalKey" ? "renamed" : null
            }
        });
        var json = adapter.Serialize(new ResolvePropertyNameRecord("value"));

        Assert.Contains("\"renamed\"", json);
        Assert.DoesNotContain("originalKey", json);
    }

    [Fact]
    public void Serialize_HookShouldSerializeProperty_ReturnsFalse_OmitsProperty()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            Hooks = new JsonSerializationHooks
            {
                ShouldSerializeProperty = (_, _, clrName, _) => clrName != "SkipMe"
            }
        });
        var json = adapter.Serialize(new ShouldSerializePropertyRecord("Alice", "secret"));

        Assert.Contains("name", json);
        Assert.DoesNotContain("skipMe", json);
    }

    [Fact]
    public void Serialize_HookOnTypeResolved_InvokedWithFinalJsonPropertyNames()
    {
        var resolvedNames = new List<string>();
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions
        {
            Hooks = new JsonSerializationHooks
            {
                OnTypeResolved = (_, names) => resolvedNames.AddRange(names)
            }
        });

        adapter.Serialize(new OnTypeResolvedRecord("a", "b"));

        Assert.Contains("alpha", resolvedNames);
        Assert.Contains("beta", resolvedNames);
    }

    [Fact]
    public void GetOptions_CalledMultipleTimes_ReturnsSameInstance()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions());

        Assert.Same(adapter.GetOptions(), adapter.GetOptions());
    }

    [Fact]
    public void Serialize_Deserialize_Roundtrip_PreservesValues()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions());
        var original = new RoundtripRecord("world", 99);

        var json = adapter.Serialize(original);
        var result = adapter.Deserialize<RoundtripRecord>(json);

        Assert.Equal(original, result);
    }

    [Fact]
    public async Task SerializeAsync_DeserializeAsync_Roundtrip_PreservesValuesAsync()
    {
        var adapter = new SystemTextJsonAdapter(new JsonSerializationOptions());
        var original = new RoundtripRecord("hello", 42);

        using var stream = new MemoryStream();
        await adapter.SerializeAsync(stream, original);
        stream.Position = 0;
        var result = await adapter.DeserializeAsync<RoundtripRecord>(stream);

        Assert.NotNull(result);
        Assert.Equal(original.Name, result.Name);
        Assert.Equal(original.Value, result.Value);
    }
}
