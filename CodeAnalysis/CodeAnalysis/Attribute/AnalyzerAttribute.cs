using System;

// ReSharper disable ALL

namespace Roslyn
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class AnalyzerAttribute : Attribute
    {
        public AnalyzerAttribute(int index) => Index = index;

        public int Index { get; }
    }
}