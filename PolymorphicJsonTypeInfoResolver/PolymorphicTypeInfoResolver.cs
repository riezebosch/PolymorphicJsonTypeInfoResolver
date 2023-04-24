using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PolymorphicJsonTypeInfoResolver;

/// <summary>
/// Polymorphism with the contract model: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0#configure-polymorphism-with-the-contract-model
/// </summary>
public class PolymorphicTypeInfoResolver {
    private readonly Func<JsonPolymorphismOptions> _options;
    private readonly IJsonTypeInfoResolver _resolver;
    private readonly Dictionary<Type, JsonPolymorphismOptions> _types = new();

    public PolymorphicTypeInfoResolver(IJsonTypeInfoResolver? resolver = null, Func<JsonPolymorphismOptions>? options = null) {
        _resolver = resolver ?? new DefaultJsonTypeInfoResolver();
        _options = options ?? (() => new JsonPolymorphismOptions());
    }

    public PolymorphicTypeInfoResolver Type<T>(Action<JsonPolymorphismOptions> use) {
        use(_types[typeof(T)] = _options());
        return this;
    }

    public PolymorphicTypeInfoResolver Type<T>(JsonPolymorphismOptions options) {
        _types[typeof(T)] = options;
        return this;
    }

    public IJsonTypeInfoResolver Build() {
        var exceptions = new List<Exception>();
        foreach (var (type, options) in _types) {
            try {
                options.DerivedTypes.Verify(type, options
                    .DerivedTypes
                    .Select(t => t.DerivedType.Assembly)
                    .Distinct());
            }
            catch (Exception ex) {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Any()) {
            throw  new AggregateException(exceptions);
        }

        return new Resolver(_resolver, _types);
    }

    private class Resolver : IJsonTypeInfoResolver {
        private readonly IJsonTypeInfoResolver _resolver;
        private readonly IDictionary<Type, JsonPolymorphismOptions> _types;

        public Resolver(IJsonTypeInfoResolver resolver, IDictionary<Type, JsonPolymorphismOptions> types) {
            _resolver = resolver;
            _types = types;
        }

        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) {
            var info = _resolver.GetTypeInfo(type, options);
            if (info == null) return info;

            info.PolymorphismOptions = !_types.TryGetValue(info.Type, out var poly)
                ? info.PolymorphismOptions
                : poly;

            return info;
        }
    }
}