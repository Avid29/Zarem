// Avishai Dernis 2026

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Test.Archs.Assembler;

[AttributeUsage(AttributeTargets.Method)]
public abstract class InstructionParserTestDataSourceAttribute : Attribute, ITestDataSource
{
    public abstract IEnumerable<object?[]> GetData(MethodInfo methodInfo);

    public virtual string? GetDisplayName(MethodInfo methodInfo, object?[]? data)
        => (string?)data?[0];
}
