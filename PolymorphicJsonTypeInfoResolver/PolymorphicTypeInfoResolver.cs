using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PolymorphicJsonTypeInfoResolver;

/// <summary>
/// Polymorphism with the contract model: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0#configure-polymorphism-with-the-contract-model
/// </summary>
public class PolymorphicTypeInfoResolver {
    private readonly Func<JsonPolymorphismOptions> _options;
    private readonly Dictionary<Type, JsonPolymorphismOptions> _types = new();

    public PolymorphicTypeInfoResolver(Func<JsonPolymorphismOptions> options) {
        _options = options;
    }

    public PolymorphicTypeInfoResolver()
        : this(() => new JsonPolymorphismOptions()) {
    }

    public PolymorphicTypeInfoResolver With<T>(Action<JsonPolymorphismOptions> use) {
        use(_types[typeof(T)] = _options());
        return this;
    }

    public PolymorphicTypeInfoResolver With<T>(JsonPolymorphismOptions options) {
        _types[typeof(T)] = options;
        return this;
    }

    public IJsonTypeInfoResolver Build(IJsonTypeInfoResolver resolver) =>
        new Resolver(resolver, Verify(_types));

    public IJsonTypeInfoResolver Build() =>
        Build(new DefaultJsonTypeInfoResolver());

    private static IDictionary<Type, JsonPolymorphismOptions> Verify(IDictionary<Type, JsonPolymorphismOptions> types)
    {
        var exceptions = new List<Exception>();
        foreach (var (type, options) in types)
        {
            var missing = Missing(options, type).ToList();
            if (missing.Any())
            {
                exceptions.Add(new MissingDerivedTypesException(type, missing));
            }
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }

        return types;
    }

    private static IEnumerable<Type> Missing(JsonPolymorphismOptions options, Type type) =>
        options.DerivedTypes
            .Select(t => t.DerivedType.Assembly)
            .Distinct()
            .AssignableTo(type)
            .Except(options.DerivedTypes.Select(t => t.DerivedType));

    private sealed class Resolver : IJsonTypeInfoResolver {
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