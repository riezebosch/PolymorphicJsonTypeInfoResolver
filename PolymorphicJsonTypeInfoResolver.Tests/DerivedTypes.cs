using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using FluentAssertions;

namespace PolymorphicJsonTypeInfoResolver.Tests;

public static class DerivedTypes {
    private record A(B Specification);
    private abstract record B;
    private record C(string Remark) : B;
    private record G<T>(T Something) : B;
    private record D(IFormattable Format);
    private class E : IFormattable {
        [ExcludeFromCodeCoverage] public string ToString(string? format, IFormatProvider? formatProvider) => "x";
    }

    [Fact]
    public static void Add() {
        const string json = """
            {
                "Specification": {
                    "$type":"type-c",
                    "Remark":"cheap"
                }
            }
            """;

        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .With<B>(x => x
                    .DerivedTypes
                    .Add<C>("type-c"))
                .Build()
        };

        var result = JsonSerializer.Deserialize<A>(json, options)!;

        result
            .Specification
            .Should()
            .Be(new C("cheap"));
    }

    [Fact]
    public static void AddFromAssembly() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .With<IFormattable>(x => x
                    .DerivedTypes
                    .Add<E>("type-e"))
                .Build()
        };

        var json = JsonSerializer.Serialize(new D(new E()), options);

        json.Should().Contain("""
            "$type":"type-e"
            """);
    }
}