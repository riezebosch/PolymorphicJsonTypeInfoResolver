using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;
using NSubstitute;

namespace PolymorphicJsonTypeInfoResolver.Tests;

public class Resolver {
    private record Box(Shape Something);

    [JsonDerivedType(typeof(Circle), "circle")]
    [JsonPolymorphic]
    private interface Shape;

    private record Square(double Length) : Shape;

    private record Circle(double Diameter) : Shape;


    [Fact]
    public static void Serialize() {
        // Arrange
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new Builder()
                .With<Shape>(new JsonPolymorphismOptions {
                    DerivedTypes = {
                        new (typeof(Square), "c"),
                        new (typeof(Circle), "d")
                    }
                })
                .Build()
        };

        // Act
        var json = JsonSerializer.Serialize(new Box(new Square(2.0)), options);

        // Assert
        json.Should().Contain("""
            "$type":"c"
            """);
    }

    [Fact]
    public static void Add() {
        const string json = """
                            {
                                "Something": {
                                    "$type":"square",
                                    "Length":2.1
                                }
                            }
                            """;

        var options = new JsonSerializerOptions {
            TypeInfoResolver = new Builder()
                .With<Shape>(x => x
                    .DerivedTypes
                    .Add<Square>("square"))
                .Build()
        };

        var result = JsonSerializer.Deserialize<Box>(json, options)!;

        result
            .Something
            .Should()
            .Be(new Square(2.1));
    }

    [Fact]
    public static void AddAdd() {
        // Arrange
        const string json = """
                            [{
                                "Something": {
                                    "$type":"square",
                                    "Length":2
                                }
                            },
                            {
                                "Something": {
                                    "$type":"circle",
                                    "Diameter":1
                                }
                            }]
                            """;

        var options = new JsonSerializerOptions {
            TypeInfoResolver = new Builder()
                .With<Shape>(x => x
                    .DerivedTypes
                    .Add<Square>("square")
                    .Add<Circle>("circle"))
                .Build()
        };

        // Act
        var result = JsonSerializer.Deserialize<Box[]>(json, options)!;

        // Assert
        result[0].Should().Be(new Box(new Square(2)));
        result[1].Should().Be(new Box(new Circle(1)));
    }

    [Fact]
    public static void WithAttributes() {
        // Arrange
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new Builder()
                .Build()
        };

        // Act
        var json = JsonSerializer.Serialize(new Box(new Circle(2)), options);

        // Assert
        json.Should().Contain("""
            "$type":"circle"
            """);
    }

    [Fact]
    public static void Options() {
        // Arrange
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new Builder()
                .With<Square>(new JsonPolymorphismOptions {
                        TypeDiscriminatorPropertyName = "$TYPE",
                        DerivedTypes =  {
                            new JsonDerivedType(typeof(Square), "C")
                        }
                    })
                .Build()
        };

        // Act
        var json = JsonSerializer.Serialize(new Square(4), options);

        // Assert
        json.Should().Contain("""
                              "$TYPE":"C"
                              """);
    }

    [Fact]
    public static void Factory() {
        // Arrange
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new Builder {
                    Options = () => new() {
                        TypeDiscriminatorPropertyName = "$TYPE"
                    }
                }
                .With<Square>(options => options
                    .DerivedTypes
                    .Add(new JsonDerivedType(typeof(Square), "C")))
                .Build()
        };

        // Act
        var json = JsonSerializer.Serialize(new Square(2), options);

        // Assert
        json.Should().Contain("""
            "$TYPE":"C"
            """);
    }

    [Fact]
    public void Deserialize() {
        const string json = """
            {
                "Something": {
                    "$type":"c",
                    "Length":3
                }
            }
            """;

        var options = new JsonSerializerOptions {
            TypeInfoResolver = new Builder()
                .With<Shape>(new JsonPolymorphismOptions {
                    DerivedTypes   = {
                        new (typeof(Square), "c"),
                        new (typeof(Circle), "d")
                    }
                }).Build()
        };

        var result = JsonSerializer.Deserialize<Box>(json, options)!;

        result
            .Something
            .Should()
            .Be(new Square(3));
    }

    [Fact]
    public void NoTypeInfo() {
        var options = new JsonSerializerOptions {
            TypeInfoResolver = new Builder {
                    Resolver = Substitute.For<IJsonTypeInfoResolver>()
                }
                .Build()
        };

        var act = () => JsonSerializer.Deserialize<Square>("{}", options);
        act.Should().Throw<NotSupportedException>();
    }
}