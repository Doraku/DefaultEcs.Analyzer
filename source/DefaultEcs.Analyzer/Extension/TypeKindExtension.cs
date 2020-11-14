using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class TypeKindExtension
    {
        public static string ToCode(this TypeKind typeKind) => typeKind switch
        {
            TypeKind.Class => "class",
            TypeKind.Struct => "struct",
            _ => ""
        };
    }
}
