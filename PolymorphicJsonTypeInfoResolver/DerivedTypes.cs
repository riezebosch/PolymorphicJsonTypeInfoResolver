using System.Text.Json.Serialization.Metadata;

namespace PolymorphicJsonTypeInfoResolver;

public static class DerivedTypes {
    public static IList<JsonDerivedType> Add(this IList<JsonDerivedType> types, Type type, string discriminator) {
        types.Add(new JsonDerivedType(type, discriminator));
        return types;
    }

    public static IList<JsonDerivedType> Add<T>(this IList<JsonDerivedType> types, string discriminator) =>
        types.Add(typeof(T), discriminator);
}
