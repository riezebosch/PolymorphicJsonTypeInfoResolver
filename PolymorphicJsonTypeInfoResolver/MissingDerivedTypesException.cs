using System.Text;

namespace PolymorphicJsonTypeInfoResolver;

public class MissingDerivedTypesException : Exception {
    public MissingDerivedTypesException(IEnumerable<Type> missing) : base(Format(missing)){
    }

    private static string Format(IEnumerable<Type> missing) {
        var sb = new StringBuilder("Missing derived types:")
            .AppendLine();

        foreach (var type in missing) {
            sb.Append(" * ");
            sb.AppendLine(type.FullName);
        }

        return sb.ToString();
    }
}