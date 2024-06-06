using System.Text.Json;
using FluentAssertions;

namespace PolymorphicJsonTypeInfoResolver.Tests;

public class Old {
    [Fact]
    [Obsolete("Testing the old API")]
    public void Test() {
        const string json = """
                            {
                                "$type":"circle",
                                "Radius":2
                            }
                            """;

        var options = new JsonSerializerOptions {
            TypeInfoResolver = new PolymorphicTypeInfoResolver()
                .Type<Shape>(x => x
                    .DerivedTypes
                    .Add<Circle>("circle"))
        };

        var result = JsonSerializer.Deserialize<Shape>(json, options)!;

        result
            .Should()
            .Be(new Circle(2));
    }

    public record Circle(double Radius) : Shape;
    public record Shape;
}