# Polymorphic Json Type Info Resolver

[![nuget](https://img.shields.io/nuget/v/PolymorphicJsonTypeInfoResolver.svg)](https://www.nuget.org/packages/PolymorphicJsonTypeInfoResolver/)
[![stryker](https://img.shields.io/endpoint?style=flat&label=stryker&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Friezebosch%2FPolymorphicJsonTypeInfoResolver%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/riezebosch/PolymorphicJsonTypeInfoResolver/main)

Using [polymorphism with the contract model](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0#configure-polymorphism-with-the-contract-model) available since `.NET7+`.

```csharp
var resolver = new PolymorphicTypeInfoResolver()
    .Type<B>(x => x
        .Has<C>()
        .Has<D>("d"));
        
var json = JsonSerializer.Serialize(new A(new C("cheap")), new JsonSerializerOptions {
    TypeInfoResolver = resolver
});
```

```json
{
    "Specification": {
        "$type":"C",
        "Remarks":"cheap"
    }
}
```

```csharp
var data = JsonSerializer.Deserialize<A>(json, new JsonSerializerOptions {
    TypeInfoResolver = resolver
});
```

Api inspired by [EF model inheritance](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance).

```csharp
new PolymorphicTypeInfoResolver()
    .Type<B>(x => x
        .Discriminator("$TYPE")
        .Has<C>()
        .Has<D>());
```

Opt-in for all derived types (from the same assemby):

```csharp
new PolymorphicTypeInfoResolver()
    .Type<C>(x => x.AddAllDerived())
    .Type<D>(x => x.AddAllDerived(t => t.Name.ToLowerInvariant()));
```