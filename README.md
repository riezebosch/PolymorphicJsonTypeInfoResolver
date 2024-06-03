# Polymorphic Json Type Info Resolver

[![nuget](https://img.shields.io/nuget/v/PolymorphicJsonTypeInfoResolver.svg)](https://www.nuget.org/packages/PolymorphicJsonTypeInfoResolver/)
[![stryker](https://img.shields.io/endpoint?style=flat&label=stryker&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Friezebosch%2FPolymorphicJsonTypeInfoResolver%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/riezebosch/PolymorphicJsonTypeInfoResolver/main)
[![codecov](https://codecov.io/gh/riezebosch/PolymorphicJsonTypeInfoResolver/branch/main/graph/badge.svg)](https://codecov.io/gh/riezebosch/PolymorphicJsonTypeInfoResolver)
[![maintainability](https://api.codeclimate.com/v1/badges/a1220004f50965c81331/maintainability)](https://codeclimate.com/github/riezebosch/PolymorphicJsonTypeInfoResolver/maintainability)
[![Build status](https://ci.appveyor.com/api/projects/status/vb4vs3l7a22rgfs2/branch/main?svg=true)](https://ci.appveyor.com/project/riezebosch/polymorphicjsontypeinforesolver/branch/main)

Polymorphic Json Type Info Resolver allows you to [configure polymorphism on the contract model](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-8-0#configure-polymorphism-with-the-contract-model)
without polluting the domain model with attributes. This library leverages the polymorphic type serialization feature introduced in .NET7.

## Breaking Changes v8

In version 8, the automatic inclusion of all subtypes for a specified type and the validation logic have been removed. This change simplifies the library and reduces its scope. These features may be reintroduced in separate packages in the future, but for now, they are not part of this library.

**What does this mean for you?**

- **Manual Subtype Registration**: You will now need to manually register each subtype using the `With<T>` method as shown in the usage section below.
- **No Validation Logic**: Any custom validation logic will need to be implemented separately if required.
- **Updated Method Names**: The method previously named `Type` has been renamed to `With`.
- **Builder Pattern**: You now need to close the builder with a `Build` method.
- 
## Installation

You can install the Polymorphic Json Type Info Resolver via NuGet Package Manager or by using the .NET CLI.

NuGet Package Manager:

```mathematica
1. Search for "PolymorphicJsonTypeInfoResolver" in the NuGet Package Manager in Visual Studio.
2. Click "Install".
```

.NET CLI:

```csharp
dotnet add package PolymorphicJsonTypeInfoResolver
```

## Usage

Define your domain model:

```csharp
record Box(Shape Something);
interface Shape;
record Square(double Length) : Shape;
record Circle(double Radius) : Shape;
```

Here's an example of how to use Polymorphic Json Type Info Resolver:

```csharp
var options = new JsonSerializerOptions {
    TypeInfoResolver = new PolymorphicJsonTypeInfoResolver.Builder()
        .With<Shape>(x => x
            .DerivedTypes
            .Add<Square>("square")
            .Add<Circle>("circle"))
        .Build()
};
        
var json = JsonSerializer.Serialize(new Box(new Circle(10)), options);
```

The result will look like this:

```json
{
    "Something": {
        "$type":"circle",
        "Radius":10
    }
}
```

In the above code snippet, we create a new instance of JsonSerializerOptions and set its TypeInfoResolver property to an
instance of PolymorphicTypeInfoResolver. We then configure the resolver to serialize objects of type Shape with a $type
property that specifies the derived type (Square or Circle).

## Specify Options

For advanced configurations, you can specify the full JsonPolymorphismOptions when specifying a type:

```csharp
 new Builder()
    .With<Shape>(new JsonPolymorphismOptions {
        TypeDiscriminatorPropertyName = "$TYPE",
        DerivedTypes =  {
            new JsonDerivedType(typeof(Circle), "circle")
        }
    })
    .Build()
```

Note: It is not possible to use the cleaner syntax Add<T> in this context.

## Use a Factory for Options

To supply a factory for the polymorphic json options, you can use the following code:

```csharp
new Builder(() => new JsonPolymorphismOptions {
        TypeDiscriminatorPropertyName = "$TYPE"
    })
    .With<Shape>(...)
    .Build()
```

In the above code snippet, we create a new instance of JsonSerializerOptions and set its `TypeInfoResolver` property to an
instance of `PolymorphicTypeInfoResolver`. We then supply a factory function that returns an instance of `JsonPolymorphismOptions`
with a custom `$TYPE` type discriminator property name.

> **Remark**: this readme was peer-reviewed by ChatGPT.

Happy coding!