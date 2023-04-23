using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PolymorphicJsonTypeInfoResolver;

/// <summary>
/// Polymorphism with the contract model: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0#configure-polymorphism-with-the-contract-model
/// </summary>
public class PolymorphicTypeInfoResolver : IJsonTypeInfoResolver {
    private readonly Func<JsonPolymorphismOptions> _options;
    private readonly IJsonTypeInfoResolver _resolver;
    private readonly Dictionary<Type, JsonPolymorphismOptions> _types = new();

    public PolymorphicTypeInfoResolver(IJsonTypeInfoResolver? resolver = null,
        Func<JsonPolymorphismOptions>? options = null) {
        _resolver = resolver ?? new DefaultJsonTypeInfoResolver();
        _options = options ?? (() => new JsonPolymorphismOptions());
    }

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) {
        var info = _resolver.GetTypeInfo(type, options);
        if (info == null) return info;

        info.PolymorphismOptions = _types.TryGetValue(info.Type, out var poly)
            ? poly
            : null;

        return info;
    }

    public PolymorphicTypeInfoResolver Type<T>(Action<JsonPolymorphismOptions> use) {
        use(_types[typeof(T)] = _options());
        return this;
    }

    public PolymorphicTypeInfoResolver Type<T>(JsonPolymorphismOptions options) {
        _types[typeof(T)] = options;
        return this;
    }
}