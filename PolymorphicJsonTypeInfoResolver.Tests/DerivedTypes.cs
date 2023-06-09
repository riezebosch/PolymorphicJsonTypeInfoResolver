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
                .Type<B>(x => x
                    .DerivedTypes
                    .Add<C>("type-c")
                    .Verify<B>())
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
                .Type<IFormattable>(x => x
                    .DerivedTypes
                    .Add<E>("type-e")
                    .Verify<IFormattable, E>())
        };

        var json = JsonSerializer.Serialize(new D(new E()), options);

        json.Should().Contain("""
            "$type":"type-e"
            """);
    }

    [Fact]
    public static void AddAllAssignableTo() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x => x
                    .DerivedTypes
                    .AddAllAssignableTo<B>(t => t.Name))
        };

        var json = JsonSerializer.Serialize(new A(new C("cheap")), options);

        json.Should().Contain("""
            "$type":"C"
            """);
    }

    [Fact]
    public static void AddAllAssignableToOwnType() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<C>(x => x
                    .DerivedTypes
                    .AddAllAssignableTo<C>(t => t.Name))
        };

        var json = JsonSerializer.Serialize(new C("cheap"), options);

        json.Should().Contain("""
            "$type":"C"
            """);
    }

    [Fact]
    public static void AddAllAssignableToDoesNotIncludeGenerics() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x => x
                    .DerivedTypes
                    .AddAllAssignableTo<B>(t => t.Name))
        };

        var act = () => JsonSerializer
            .Serialize(new A(new G<int>(3)), options);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public static void AddAllAssignableToFromAssembly() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<IFormattable>(x => x
                    .DerivedTypes
                    .AddAllAssignableTo<IFormattable, E>(t => t.Name))
        };

        var json = JsonSerializer.Serialize(new D(new E()), options);

        json.Should().Contain("""
            "$type":"E"
            """);
    }

    [Fact]
    public static void Verify() {
        var act = () => new PolymorphicTypeInfoResolver()
            .Type<B>(x => x
                .DerivedTypes
                .Verify<B>());

        act.Should()
            .Throw<MissingDerivedTypesException>()
            .WithMessage("*+C");
    }

    [Fact]
    public static void VerifyFromAssembly() {
        var act = () => new PolymorphicTypeInfoResolver()
            .Type<IFormattable>(x => x
                .DerivedTypes
                .Verify<IFormattable, E>());

        act.Should()
            .Throw<MissingDerivedTypesException>()
            .WithMessage($"Missing derived types:* ? {typeof(E)}");
    }
}