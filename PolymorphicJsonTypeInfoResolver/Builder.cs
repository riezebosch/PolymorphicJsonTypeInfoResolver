using System.Text.Json.Serialization.Metadata;

namespace PolymorphicJsonTypeInfoResolver;

/// <summary>
/// Polymorphism with the contract model: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0#configure-polymorphism-with-the-contract-model
/// </summary>
public record Builder(Func<JsonPolymorphismOptions> Options, IJsonTypeInfoResolver Resolver) {
    private readonly Dictionary<Type, JsonPolymorphismOptions> _types = new();

    public Builder()
        : this(() => new JsonPolymorphismOptions(), new DefaultJsonTypeInfoResolver()) {
    }

    public Builder With<T>(Action<JsonPolymorphismOptions> use) {
        use(_types[typeof(T)] = Options());
        return this;
    }

    public Builder With<T>(JsonPolymorphismOptions options) {
        _types[typeof(T)] = options;
        return this;
    }

    public IJsonTypeInfoResolver Build() =>
        new Resolver(Resolver, _types);
}