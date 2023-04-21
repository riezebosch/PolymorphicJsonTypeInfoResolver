# Polymorphic Json Type Info Resolver

[![nuget](https://img.shields.io/nuget/v/PolymorphicJsonTypeInfoResolver.svg)](https://www.nuget.org/packages/PolymorphicJsonTypeInfoResolver/)
[![stryker](https://img.shields.io/endpoint?style=flat&label=stryker&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Friezebosch%2FPolymorphicJsonTypeInfoResolver%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/riezebosch/PolymorphicJsonTypeInfoResolver/main)
[![codecov](https://codecov.io/gh/riezebosch/PolymorphicJsonTypeInfoResolver/branch/main/graph/badge.svg)](https://codecov.io/gh/riezebosch/PolymorphicJsonTypeInfoResolver)
[![maintainability](https://api.codeclimate.com/v1/badges/a1220004f50965c81331/maintainability)](https://codeclimate.com/github/riezebosch/PolymorphicJsonTypeInfoResolver/maintainability)
[![Build status](https://ci.appveyor.com/api/projects/status/vb4vs3l7a22rgfs2/branch/main?svg=true)](https://ci.appveyor.com/project/riezebosch/polymorphicjsontypeinforesolver/branch/main)

Using [polymorphism with the contract model](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0#configure-polymorphism-with-the-contract-model)
available since `.NET7+`.

```csharp
var options = new JsonSerializerOptions {
    TypeInfoResolver = new PolymorphicTypeInfoResolver()
        .Type<B>(x => x.DerivedTypes.Add<C>("c"))
}
        
var json = JsonSerializer.Serialize(new A(new C("cheap")), options);
```

```json
{
    "Specification": {
        "$type":"C",
        "Remarks":"cheap"
    }
}
```

Factory for default options:

```csharp
var options = new JsonSerializerOptions {
    TypeInfoResolver = new PolymorphicTypeInfoResolver(options: () => new JsonPolymorphismOptions {
        TypeDiscriminatorPropertyName = "$TYPE"
    })
};
```

Opt-in for all derived types (from the same assemby):

```csharp
new JsonSerializerOptions {
    TypeInfoResolver = new PolymorphicTypeInfoResolver()
        .Type<C>(x => x
            .DerivedTypes
            .AddAllAssignableTo<C>())
};
```