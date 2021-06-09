using System.Linq;
using Microsoft.CodeAnalysis;

namespace DefaultEcs.Analyzer.Extensions
{
    public static class SymbolInfoExtension
    {
        public static T As<T>(this SymbolInfo symbolInfo) where T : ISymbol => symbolInfo.Symbol is T t ? t : symbolInfo.CandidateSymbols.OfType<T>().FirstOrDefault();
    }
}
