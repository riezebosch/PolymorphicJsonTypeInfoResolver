using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PolymorphicJsonTypeInfoResolver;

internal sealed class Resolver(IJsonTypeInfoResolver inner, IDictionary<Type, JsonPolymorphismOptions> types) : IJsonTypeInfoResolver {
    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) {
        var info = inner.GetTypeInfo(type, options);
        if (info != null && types.TryGetValue(info.Type, out var polymorphism))
            info.PolymorphismOptions = polymorphism;

        return info;
    }
}