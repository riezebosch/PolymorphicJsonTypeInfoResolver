# Polymorphic Json Type Info Resolver

[![nuget](https://img.shields.io/nuget/v/PolymorphicJsonTypeInfoResolver.svg)](https://www.nuget.org/packages/PolymorphicJsonTypeInfoResolver/)
[![stryker](https://img.shields.io/endpoint?style=flat&label=stryker&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Friezebosch%2FPolymorphicJsonTypeInfoResolver%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/riezebosch/PolymorphicJsonTypeInfoResolver/main)
[![codecov](https://codecov.io/gh/riezebosch/PolymorphicJsonTypeInfoResolver/branch/main/graph/badge.svg)](https://codecov.io/gh/riezebosch/PolymorphicJsonTypeInfoResolver)
[![maintainability](https://api.codeclimate.com/v1/badges/a1220004f50965c81331/maintainability)](https://codeclimate.com/github/riezebosch/PolymorphicJsonTypeInfoResolver/maintainability)
[![Build status](https://ci.appveyor.com/api/projects/status/vb4vs3l7a22rgfs2/branch/main?svg=true)](https://ci.appveyor.com/project/riezebosch/polymorphicjsontypeinforesolver/branch/main)

Polymorphic Json Type Info Resolver allows you to [configure polymorphism on the contract model](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0#configure-polymorphism-with-the-contract-model)
without polluting the domain model with attributes. This library leverages the polymorphic type serialization feature introduced in .NET7.

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

Here's an example of how to use Polymorphic Json Type Info Resolver:

```csharp
var options = new JsonSerializerOptions {
    TypeInfoResolver = new PolymorphicTypeInfoResolver()
        .Type<Shape>(x => x
            .DerivedTypes
            .Add<Square>("square")
            .Add<Circle>("circle"))
};
        
var json = JsonSerializer.Serialize(new Box(new Circle(10)), options);
```

In the above code snippet, we create a new instance of JsonSerializerOptions and set its TypeInfoResolver property to an instance of PolymorphicTypeInfoResolver. We then configure the resolver to serialize objects of type Shape with a $type property that specifies the derived type (Square or Circle). Finally, we serialize a Box object containing a Circle object with a radius of 10.

> **Remark**: the `Add` method can use the type name for discriminator, but I'm not sure that's a good idea after all.
Since it is part of your contract you may want to decouple it so it does not automatically change as part of refactoring. 

```json
{
    "Shape": {
        "$type":"circle",
        "Radius":10
    }
}
```

To supply a factory for the polymorphic JSON options, you can use the following code:

```csharp
var options = new JsonSerializerOptions {
    TypeInfoResolver = new PolymorphicTypeInfoResolver(options: () => new JsonPolymorphismOptions {
        TypeDiscriminatorPropertyName = "$TYPE"
    })
};
```

In the above code snippet, we create a new instance of `JsonSerializerOptions` and set its `TypeInfoResolver` property to an 
instance of `PolymorphicTypeInfoResolver`. We then supply a factory function that returns an instance of `JsonPolymorphismOptions`
with a custom `$TYPE` type discriminator property name.

To opt-in to all derived types from its own assembly or the specified assembly, you can use the following code:

```csharp
new JsonSerializerOptions {
    TypeInfoResolver = new PolymorphicTypeInfoResolver()
        .Type<Shape>(x => x
            .DerivedTypes
            .AddAllAssignableTo<Shape>(t => t.Name)
            .AddAllAssignableTo<Shape, Parallelogram>(t => t.Name))
};
```

In the above code snippet, we create a new instance of `JsonSerializerOptions` and set its `TypeInfoResolver` property
to an instance of `PolymorphicTypeInfoResolver`. We then configure the resolver to serialize objects of type `Shape`
with all derived types assignable to `Shape` from its own assembly and the assembly where `Parallelogram` is located with their name
as the type discriminator.

> **Remark**: this readme was peer-reviewed by ChatGPT.

Happy coding!