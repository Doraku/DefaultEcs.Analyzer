using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer.Extension
{
    internal static class AccessibilityExtension
    {
        public static string ToCode(this Accessibility accessibility) => accessibility switch
        {
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.Public => "public",
            _ => ""
        };
    }
}
