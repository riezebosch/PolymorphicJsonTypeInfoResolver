using System.Text.Json.Serialization.Metadata;

namespace PolymorphicJsonTypeInfoResolver;

public static class AddDerivedTypes {
    public static IList<JsonDerivedType> Add(this IList<JsonDerivedType> types, Type type, string? discriminator = null) {
        types.Add(new JsonDerivedType(type, discriminator ?? type.Name));
        return types;
    }

    public static IList<JsonDerivedType> Add<T>(this IList<JsonDerivedType> types, string? discriminator = null) =>
        types.Add(typeof(T), discriminator);

    public static IList<JsonDerivedType> AddAllAssignableTo(this IList<JsonDerivedType> types, Type type, Func<Type, string?>? discriminator = null) {
        foreach (var derived in type
                     .Assembly
                     .GetTypes()
                     .Where(t => t.IsAssignableTo(type))
                     .Where(t => !t.IsAbstract)) {
            types.Add(derived, discriminator?.Invoke(derived));
        }

        return types;
    }

    public static IList<JsonDerivedType> AddAllAssignableTo<T>(this IList<JsonDerivedType> types, Func<Type, string?>? discriminator = null) =>
        types.AddAllAssignableTo(typeof(T), discriminator);
}