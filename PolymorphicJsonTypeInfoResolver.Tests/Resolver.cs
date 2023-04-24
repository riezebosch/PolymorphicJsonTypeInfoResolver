using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;
using NSubstitute;

namespace PolymorphicJsonTypeInfoResolver.Tests;

public class Resolver {
    private record A(B Specification);

    [JsonDerivedType(typeof(D), "type-d")]
    [JsonPolymorphic]
    private abstract record B;

    private record C(string Remarks) : B;

    private record D : B;


    [Fact]
    public static void Serialize() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .With<B>(new JsonPolymorphismOptions {
                    DerivedTypes = {
                        new (typeof(C), "c"),
                        new (typeof(D), "d")
                    }
                })
                .Build()
        };

        var json = JsonSerializer.Serialize(new A(new C("cheap")), options);

        json.Should().Contain("""
            "$type":"c"
            """);
    }

    [Fact]
    public static void WithAttributes() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Build()
        };

        var json = JsonSerializer.Serialize(new A(new D()), options);

        json.Should().Contain("""
            "$type":"type-d"
            """);
    }

    [Fact]
    public static void Options() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver(() =>
                    new JsonPolymorphismOptions {
                        TypeDiscriminatorPropertyName = "$TYPE"
                    })
                .With<C>(x => x
                    .DerivedTypes
                    .Add(new JsonDerivedType(typeof(C), "C")))
                .Build()
        };

        var json = JsonSerializer.Serialize(new C("cheap"), options);

        json.Should().Contain("""
            "$TYPE":"C"
            """);
    }

    [Fact]
    public void Deserialize() {
        const string json = """
            {
                "Specification": {
                    "$type":"c",
                    "Remarks":"cheap"
                }
            }
            """;

        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .With<B>(new JsonPolymorphismOptions {
                    DerivedTypes   = {
                        new (typeof(C), "c"),
                        new (typeof(D), "d")
                    }
                }).Build()
        };

        var result = JsonSerializer.Deserialize<A>(json, options)!;

        result
            .Specification
            .Should()
            .Be(new C("cheap"));
    }

    [Fact]
    public void NoTypeInfo() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Build(Substitute.For<IJsonTypeInfoResolver>())
        };

        var act = () => JsonSerializer.Deserialize<C>("{}", options);
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Verify() {
        var resolver = new PolymorphicTypeInfoResolver()
            .With<B>(x => x
                .DerivedTypes
                .Add<C>("c"));

        var act = () => resolver.Build();
        act.Should()
            .Throw<MissingDerivedTypesException>()
            .WithMessage($"Missing* ? {typeof(D)}");
    }

    [Fact]
    public void VerifyAll() {
        var resolver = new PolymorphicTypeInfoResolver()
            .With<B>(x => x
                .DerivedTypes
                .Add<C>("c"))
            .With<IFormattable>(x => x
                .DerivedTypes
                .Add<int>("x"));

        var act = () => resolver.Build();
        act.Should()
            .Throw<AggregateException>()
            .WithMessage($"*{typeof(B)}*{typeof(IFormattable)}*");
    }
}