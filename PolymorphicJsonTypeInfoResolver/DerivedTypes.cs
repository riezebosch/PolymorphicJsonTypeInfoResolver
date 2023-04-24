using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace PolymorphicJsonTypeInfoResolver;

public static class DerivedTypes {
    public static IList<JsonDerivedType> Add(this IList<JsonDerivedType> types, Type type, string discriminator) {
        types.Add(new JsonDerivedType(type, discriminator));
        return types;
    }

    public static IList<JsonDerivedType> Add<T>(this IList<JsonDerivedType> types, string discriminator) =>
        types.Add(typeof(T), discriminator);

    /// <remarks>For stable contracts, add types individually and choose a reliable discriminator.</remarks>
    public static IList<JsonDerivedType> AddAllAssignableTo(this IList<JsonDerivedType> types, Type type, IEnumerable<Assembly> assemblies, Func<Type, string> discriminator) {
        foreach (var derived in assemblies.AssignableTo(type)) {
            types.Add(derived, discriminator(derived));
        }

        return types;
    }

    /// <remarks>For stable contracts, add types individually and choose a reliable discriminator.</remarks>
    public static IList<JsonDerivedType> AddAllAssignableTo<T>(this IList<JsonDerivedType> types, IEnumerable<Assembly> assemblies, Func<Type, string> discriminator) =>
        types.AddAllAssignableTo(typeof(T), assemblies, discriminator);

    /// <remarks>For stable contracts, add types individually and choose a reliable discriminator.</remarks>
    public static IList<JsonDerivedType> AddAllAssignableTo<T, TAssembly>(this IList<JsonDerivedType> types, Func<Type, string> discriminator) =>
        types.AddAllAssignableTo<T>(new[] { typeof(TAssembly).Assembly }, discriminator);

    /// <remarks>For stable contracts, add types individually and choose a reliable discriminator.</remarks>
    public static IList<JsonDerivedType> AddAllAssignableTo<T>(this IList<JsonDerivedType> types, Func<Type, string> discriminator) =>
        types.AddAllAssignableTo<T, T>(discriminator);

    public static IList<JsonDerivedType> Verify(this IList<JsonDerivedType> types, Type type, IEnumerable<Assembly> assemblies) {
        var missing = assemblies.AssignableTo(type)
            .Except(types.Select(t => t.DerivedType))
            .ToList();

        if (missing.Any()) {
            throw new MissingDerivedTypesException(type, missing);
        }

        return types;
    }

    public static IList<JsonDerivedType> Verify<T>(this IList<JsonDerivedType> types, IEnumerable<Assembly> assemblies) =>
        types.Verify(typeof(T), assemblies);

    public static IList<JsonDerivedType> Verify<T, TAssembly>(this IList<JsonDerivedType> types) =>
        types.Verify<T>(new [] { typeof(TAssembly).Assembly });

    public static IList<JsonDerivedType> Verify<T>(this IList<JsonDerivedType> types) =>
        types.Verify<T, T>();

    private static IEnumerable<Type> AssignableTo(this Assembly assembly, Type type) =>
        assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsGenericType: false } && t.IsAssignableTo(type));

    private static IEnumerable<Type> AssignableTo(this IEnumerable<Assembly> assemblies, Type type) =>
        assemblies.SelectMany(x => x.AssignableTo(type));
}
