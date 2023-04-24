using System.Text;

namespace PolymorphicJsonTypeInfoResolver;

public class MissingDerivedTypesException : Exception {
    public MissingDerivedTypesException(Type type, IEnumerable<Type> missing) : base(Format(type, missing)){
    }

    private static string Format(Type type, IEnumerable<Type> missing) {
        var sb = new StringBuilder($"Missing derived types for '{type}':")
            .AppendLine();

        foreach (var t in missing) {
            sb.Append(" * ");
            sb.AppendLine(t.FullName);
        }

        return sb.ToString();
    }
}