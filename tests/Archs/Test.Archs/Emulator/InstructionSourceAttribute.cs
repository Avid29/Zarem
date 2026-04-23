// Avishai Dernis 2026

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Test.Archs.Emulator;

[AttributeUsage(AttributeTargets.Method)]
public abstract class InstructionSourceAttribute<TConfig> : Attribute, ITestDataSource
{
    public abstract IEnumerable<object?[]> GetData(MethodInfo methodInfo);

    public abstract string? GetDisplayName(MethodInfo methodInfo, object?[]? data);
}
