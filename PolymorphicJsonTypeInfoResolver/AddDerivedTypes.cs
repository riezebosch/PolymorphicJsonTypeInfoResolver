using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace PolymorphicJsonTypeInfoResolver;

public static class AddDerivedTypes {
    public static IList<JsonDerivedType> Add(this IList<JsonDerivedType> types, Type type, string discriminator) {
        types.Add(new JsonDerivedType(type, discriminator));
        return types;
    }

    public static IList<JsonDerivedType> Add<T>(this IList<JsonDerivedType> types, string discriminator) =>
        types.Add(typeof(T), discriminator);

    /// <remarks>For stable contracts, use the overload specifying the discriminator to ensure reliable differentiation between types.</remarks>
    public static IList<JsonDerivedType> Add<T>(this IList<JsonDerivedType> types) =>
        types.Add<T>(typeof(T).Name);

    /// <remarks>For stable contracts, add types individually and choose a reliable discriminator.</remarks>
    public static IList<JsonDerivedType> AddAllAssignableTo(this IList<JsonDerivedType> types, Type type, Assembly assembly, Func<Type, string>? discriminator = null) {
        discriminator ??= t => t.Name;
        foreach (var derived in Types(type, assembly)) {
            types.Add(derived, discriminator(derived));
        }

        return types;
    }

    /// <remarks>For stable contracts, add types individually and choose a reliable discriminator.</remarks>
    public static IList<JsonDerivedType> AddAllAssignableTo<T, TAssembly>(this IList<JsonDerivedType> types, Func<Type, string>? discriminator = null) =>
        types.AddAllAssignableTo(typeof(T), typeof(TAssembly).Assembly, discriminator);

    /// <remarks>For stable contracts, add types individually and choose a reliable discriminator.</remarks>
    public static IList<JsonDerivedType> AddAllAssignableTo<T>(this IList<JsonDerivedType> types, Func<Type, string>? discriminator = null) =>
        types.AddAllAssignableTo<T, T>(discriminator);


    public static IList<JsonDerivedType> Verify(this IList<JsonDerivedType> types, Type type, Assembly assembly) {
        var missing = Types(type, assembly)
            .Except(types.Select(t => t.DerivedType))
            .ToList();

        if (missing.Any()) {
            throw new MissingDerivedTypesException(missing);
        }

        return types;
    }

    public static IList<JsonDerivedType> Verify<T, TAssembly>(this IList<JsonDerivedType> types) =>
        types.Verify(typeof(T), typeof(TAssembly).Assembly);

    public static IList<JsonDerivedType> Verify<T>(this IList<JsonDerivedType> types) =>
        types.Verify<T, T>();

    private static IEnumerable<Type> Types(Type type, Assembly assembly) =>
        assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsGenericType: false } && t.IsAssignableTo(type));
}
