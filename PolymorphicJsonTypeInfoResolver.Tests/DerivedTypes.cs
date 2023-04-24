using System.Text.Json;
using FluentAssertions;

namespace PolymorphicJsonTypeInfoResolver.Tests;

public static class DerivedTypes {
    private record A(B Specification);
    private abstract record B;
    private record C(string Remark) : B;
    private record G<T> : B;
    private record D(IFormattable Format);
    private class E : IFormattable {
        public string ToString(string? format, IFormatProvider? formatProvider) => "x";
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
                    .Add<C>("type-c"))
        };

        var result = JsonSerializer.Deserialize<A>(json, options)!;

        result
            .Specification
            .Should()
            .Be(new C("cheap"));
    }

    [Fact]
    public static void AddWithDefaultName() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x => x.DerivedTypes
                    .Add<C>())
        };

        var json = JsonSerializer.Serialize(new A(new C("cheap")), options);

        json.Should().Contain("""
            "$type":"C"
            """);
    }

    [Fact]
    public static void AddAllAssignableTo() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x => x
                    .DerivedTypes
                    .AddAllAssignableTo<B>())
        };

        var json = JsonSerializer.Serialize(new A(new C("cheap")), options);

        json.Should().Contain("""
            "$type":"C"
            """);
    }

    [Fact]
    public static void AddAllAssignableToCustomName() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x => x
                    .DerivedTypes
                    .AddAllAssignableTo<B>(t => $"type:{t.Name}"))
        };

        var json = JsonSerializer.Serialize(new A(new C("cheap")), options);

        json.Should().Contain("""
            "$type":"type:C"
            """);
    }

    [Fact]
    public static void AddAllAssignableToOwnType() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<C>(x => x
                    .DerivedTypes
                    .AddAllAssignableTo<C>())
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
                    .AddAllAssignableTo<B>())
        };

        var act = () => JsonSerializer.Serialize(new A(new G<int>()), options);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public static void AddAllAssignableToFromAssembly() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<IFormattable>(x => x
                    .DerivedTypes
                    .AddAllAssignableTo<IFormattable, E>())
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
            .WithMessage("*+E");
    }
}