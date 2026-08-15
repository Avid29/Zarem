// Avishai Dernis 2026

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Test.Archs.Emulator;

[AttributeUsage(AttributeTargets.Method)]
public abstract class EmulatorTestDataSourceAttribute<TCase, TConfig> : Attribute, ITestDataSource
    where TCase : EmulatorTestCase<TConfig>
{
    public abstract IEnumerable<object?[]> GetData(MethodInfo methodInfo);

    public virtual string? GetDisplayName(MethodInfo methodInfo, TCase[] data) => data[0].Input;

    string? ITestDataSource.GetDisplayName(MethodInfo methodInfo, object?[]? data)
        => GetDisplayName(methodInfo, data?.OfType<TCase>().ToArray() ?? []);
}
