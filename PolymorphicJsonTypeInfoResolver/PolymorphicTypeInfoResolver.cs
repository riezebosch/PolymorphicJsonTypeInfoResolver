using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PolymorphicJsonTypeInfoResolver;

/// <summary>
/// Polymorphism with the contract model: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0#configure-polymorphism-with-the-contract-model
/// Api inspired by EF: https://learn.microsoft.com/en-us/ef/core/modeling/inheritance
/// </summary>
public class PolymorphicTypeInfoResolver : IJsonTypeInfoResolver {
    private readonly DefaultJsonTypeInfoResolver _resolver;
    private readonly Dictionary<Type, JsonPolymorphismOptions> _types = new();

    public PolymorphicTypeInfoResolver() : this(new DefaultJsonTypeInfoResolver()) {
    }

    private PolymorphicTypeInfoResolver(DefaultJsonTypeInfoResolver resolver) =>
        _resolver = resolver;

    public JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options) {
        var info = _resolver.GetTypeInfo(type, options);
        info.PolymorphismOptions = _types.TryGetValue(info.Type, out var poly)
            ? poly
            : null;

        return info;
    }

    public PolymorphicTypeInfoResolver Type<T>(Action<Options<T>> use) {
        use(new(_types[typeof(T)] = new JsonPolymorphismOptions()));
        return this;
    }

    public class Options<T> {
        private readonly JsonPolymorphismOptions _options;

        public Options(JsonPolymorphismOptions options) =>
            _options = options;

        public Options<T> Has<TDerived>(string? type = null) where TDerived: T =>
            Has(typeof(TDerived), type);

        public Options<T> Has(Type t, string? type = null) {
            _options.DerivedTypes.Add(new JsonDerivedType(t, type ?? t.Name));
            return this;
        }

        public Options<T> Discriminator(string type) {
            _options.TypeDiscriminatorPropertyName = type;
            return this;
        }
    }
}