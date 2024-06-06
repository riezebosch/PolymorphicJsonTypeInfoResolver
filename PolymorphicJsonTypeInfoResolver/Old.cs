using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PolymorphicJsonTypeInfoResolver;

/// <summary>
/// See <see cref="Builder"/> instead.
/// </summary>
[Obsolete("Use the Builder instead.")]
public class PolymorphicTypeInfoResolver : IJsonTypeInfoResolver {
    private readonly Builder _builder = new();
    private IJsonTypeInfoResolver? _resolver;

    public IJsonTypeInfoResolver Type<T>(Action<JsonPolymorphismOptions> use) {
        _builder.With<T>(use);
        return this;
    }

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) =>
        Resolver.GetTypeInfo(type, options);

    private IJsonTypeInfoResolver Resolver => _resolver ??= _builder.Build();
}