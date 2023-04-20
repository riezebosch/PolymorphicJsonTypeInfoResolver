namespace PolymorphicJsonTypeInfoResolver;

public static class Options {
    public static PolymorphicTypeInfoResolver.Options<T> AddAllDerived<T>(this PolymorphicTypeInfoResolver.Options<T> options)  =>
        AddAllDerived(options, t => t.Name);

    public static PolymorphicTypeInfoResolver.Options<T> AddAllDerived<T>(this PolymorphicTypeInfoResolver.Options<T> options, Func<Type, string> type) {
        var targetType = typeof(T);
        foreach(var derived in targetType
                    .Assembly
                    .GetTypes()
                    .Where(t => t.IsAssignableTo(targetType))
                    .Where(t => !t.IsAbstract)) {
            options.Has(derived, type(derived));
        }
        return options;
    }
}