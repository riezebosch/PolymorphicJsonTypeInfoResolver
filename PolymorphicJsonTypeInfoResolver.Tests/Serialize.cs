using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;

namespace PolymorphicJsonTypeInfoResolver.Tests;

public static class Serialize {
    private record A(B Specification);

    private abstract record B;

    private record C : B;

    [Fact]
    public static void Test() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<B>(x => x.DerivedTypes.Add(new JsonDerivedType(typeof(C), "c")))
        };

        var json = JsonSerializer.Serialize(new A(new C()), options);

        json.Should().Contain("""
            "$type":"c"
            """);
    }

    [Fact]
    public static void Options() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver(options: () =>
                    new JsonPolymorphismOptions {
                        TypeDiscriminatorPropertyName = "$TYPE"
                    })
                .Type<C>(x => x
                    .DerivedTypes
                    .Add(new JsonDerivedType(typeof(C), "C")))
        };

        var json = JsonSerializer.Serialize(new C(), options);

        json.Should().Contain("""
            "$TYPE":"C"
            """);
    }
}