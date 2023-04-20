using System.Text.Json;
using FluentAssertions;

namespace PolymorphicJsonTypeInfoResolver.Tests;

public class ResolverTests {
    private record A(B Specification);

    private abstract record B(string Remarks);

    private record C(string Remarks) : B(Remarks);

    private record D(string Remarks, int InsulationArea) : B(Remarks);
    
    [Fact]
    public void Serialize() {
        var original = new A(new C("cheap"));
        var resolver = new PolymorphicTypeInfoResolver()
            .Type<B>(x => x
                .Has<C>("c"));

        var json = JsonSerializer.Serialize(original, new JsonSerializerOptions {
            TypeInfoResolver = resolver
        });

        json.Should().Contain("""
            "$type":"c"
            """);
    }

    [Fact]
    public void Deserialize() {
        var resolver = new PolymorphicTypeInfoResolver()
            .Type<B>(x => x
                .Has<C>("c"));

        var json = """
            {
                "Specification": {
                    "$type":"c",
                    "Remarks":"cheap"
                }
            }
            """;

        var result = JsonSerializer.Deserialize<A>(json, new JsonSerializerOptions {
            TypeInfoResolver = resolver
        })!;

        result.Specification.Should().Be(new C("cheap"));
    }

    [Fact]
    public void Multiple() {
        var resolver = new PolymorphicTypeInfoResolver()
            .Type<B>(x => x
                .Has<C>("c")
                .Has<D>("d"));

        var json = """
            [{
                "Specification": {
                    "$type":"c",
                    "Remarks":"cheap"
                }
            },
            {
                "Specification": {
                    "$type":"d",
                    "InsulationArea":2
                }
            }]
            """;

        var result = JsonSerializer.Deserialize<A[]>(json, new JsonSerializerOptions {
            TypeInfoResolver = resolver
        })!;

        result
            .Select(s => s.Specification)
            .Should()
            .BeEquivalentTo(new B[] {
                new C("cheap"),
                new D(null!, 2)
            });
    }

    [Fact]
    public void Discriminator() {
        var resolver = new PolymorphicTypeInfoResolver()
            .Type<B>(x => x
                .Discriminator("$TYPE$")
                .Has<C>("c"));

        var json = """
            {
                "Specification": {
                    "$TYPE$":"c",
                    "Remarks":"cheap"
                }
            }
            """;

        var result = JsonSerializer.Deserialize<A>(json, new JsonSerializerOptions {
            TypeInfoResolver = resolver
        })!;

        result.Specification.Should().Be(new C("cheap"));
    }
    
    [Fact]
    public void DefaultName() {
        A original = new A(new C("cheap"));
        var resolver = new PolymorphicTypeInfoResolver()
            .Type<B>(x => x.Has<C>());

        var json = JsonSerializer.Serialize(original, new JsonSerializerOptions {
            TypeInfoResolver = resolver
        });

        json.Should().Contain("""
            "$type":"C"
            """);
    }

    [Fact]
    public void AddAllDerived() {
        B original = new C("cheap");
        var resolver = new PolymorphicTypeInfoResolver()
            .Type<B>(x => x.AddAllDerived());

        var json = JsonSerializer.Serialize(original, new JsonSerializerOptions {
            TypeInfoResolver = resolver
        });

        json.Should().Contain("""
            "$type":"C"
            """);
    }

    [Fact]
    public void AddAllDerivedCustomName() {
        var original = new A(new C("cheap"));
        var resolver = new PolymorphicTypeInfoResolver()
            .Type<B>(x => x.AddAllDerived(t => t.Name.ToLowerInvariant()));

        var json = JsonSerializer.Serialize(original, new JsonSerializerOptions {
            TypeInfoResolver = resolver
        });

        json.Should().Contain("""
            "$type":"c"
            """);
    }
}