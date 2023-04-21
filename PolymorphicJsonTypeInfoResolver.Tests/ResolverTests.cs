using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;
using NSubstitute;

namespace PolymorphicJsonTypeInfoResolver.Tests;

public class ResolverTests {
    private record A(B Specification);

    private abstract record B(string Remarks);

    private record C(string Remarks) : B(Remarks);

    private record D(string Remarks, int InsulationArea) : C(Remarks);
    
    [Fact]
    public void Serialize() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x => x.DerivedTypes.Add<C>("c"))
        };
        
        var json = JsonSerializer.Serialize(new A(new C("cheap")), options);

        json.Should().Contain("""
            "$type":"c"
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
                .Type<B>(x => x.DerivedTypes.Add<C>("c"))
        };
        
        var result = JsonSerializer.Deserialize<A>(json, options)!;

        result
            .Specification
            .Should()
            .Be(new C("cheap"));
    }

    [Fact]
    public void Multiple() {
        const string json = """
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

        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x => x.DerivedTypes
                    .Add<C>("c")
                    .Add<D>("d"))
        };
        
        var result = JsonSerializer.Deserialize<A[]>(json, options)!;

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
        const string json = """
            {
                "Specification": {
                    "$TYPE$":"c",
                    "Remarks":"cheap"
                }
            }
            """;

        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x =>
                {
                    x.TypeDiscriminatorPropertyName = "$TYPE$";
                    x.DerivedTypes.Add<C>("c");
                })
        };
        
        var result = JsonSerializer.Deserialize<A>(json, options)!;

        result.Specification.Should().Be(new C("cheap"));
    }
    
    [Fact]
    public void DefaultName() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x => x.DerivedTypes.Add<C>())
        };
        
        var json = JsonSerializer.Serialize(new A(new C("cheap")), options);

        json.Should().Contain("""
            "$type":"C"
            """);
    }

    [Fact]
    public void AddAllAssignableTo() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x => x.DerivedTypes.AddAllAssignableTo<B>())
        };
        
        var json = JsonSerializer.Serialize(new A(new C("cheap")), options);

        json.Should().Contain("""
            "$type":"C"
            """);
    }

    [Fact]
    public void AddAllAssignableToCustomName() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x => x
                    .DerivedTypes
                    .AddAllAssignableTo<B>(t => t.Name.ToLowerInvariant()))
        };
        
        var json = JsonSerializer.Serialize(new A(new C("cheap")), options);

        json.Should().Contain("""
            "$type":"c"
            """);
    }
    
    [Fact]
    public void AddAllAssignableToOwnType() {
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
    public void DefaultOptions() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver(options: () => new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$TYPE"
                })
                .Type<C>(x => x.DerivedTypes.Add<C>())
        };
        
        var json = JsonSerializer.Serialize(new C(""), options);

        json.Should().Contain("""
            "$TYPE":"C"
            """);
    }
    
    [Fact]
    public void NoInfo() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver(resolver: Substitute.For<IJsonTypeInfoResolver>())
        };
        
        var act = () => JsonSerializer.Deserialize<C>("{}", options);
        act.Should().Throw<NotSupportedException>();
    }
}