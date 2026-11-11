// Avishai Dernis 2026

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Runtime.CompilerServices;
using Zarem.Assembler.Models.Tables;
using Zarem.Assembler.Tokenization.Profiles;
using Zarem.Attributes.Arguments;

namespace Test.Archs.Assembler;

[AttributeUsage(AttributeTargets.Method)]
public abstract class InstructionParserTestDataSourceAttribute : Attribute, ITestDataSource
{
    public abstract IEnumerable<object?[]> GetData(MethodInfo methodInfo);

    public virtual string? GetDisplayName(MethodInfo methodInfo, object?[]? data)
        => (string?)data?[0];

    protected static string GenerateArgumentString<TArg, TReg, TSet, TRef>(TArg arg, ITokenizerProfile profile)
        where TArg : unmanaged, Enum
        where TReg : unmanaged, Enum
        where TSet : unmanaged, Enum
        where TRef : unmanaged, Enum
    {
        var attr = ArgumentTable<TArg>.GetAttribute(arg);

        return attr switch
        {
            ImmediateArgumentAttribute<TRef> imm => GenerateImmediateArgument(arg, imm, profile),
            RegisterArgumentAttribute<TSet> reg => GenerateRegisterArgument<TArg, TReg, TSet>(arg, reg, profile),
            SplitArgumentAttribute<TArg> split => GenerateSplitArgument<TArg, TReg, TSet, TRef>(split, profile),
            _ => throw new NotImplementedException(),
        };
    }

    private static string GenerateImmediateArgument<TArg, TRef>(TArg arg, ImmediateArgumentAttribute<TRef> immAttr, ITokenizerProfile profile)
        where TArg : unmanaged, Enum
        where TRef : unmanaged, Enum
    {
        int rawValue;

        if (immAttr.BitCount >= 32)
        {
            rawValue = Random.Shared.Next();
        }
        else if (immAttr.Signed)
        {
            int max = 1 << (immAttr.BitCount - 1);
            int min = -max;
            rawValue = Random.Shared.Next(min, max);
        }
        else
        {
            int max = 1 << immAttr.BitCount;
            rawValue = Random.Shared.Next(0, max);
        }

        int scaledValue = rawValue << immAttr.ShiftAmount;
        return profile.ImmediatePrefix is '\0' ? $"{scaledValue}" : $"{profile.ImmediatePrefix}{scaledValue}";
    }

    private static string GenerateRegisterArgument<TArg, TReg, TSet>(TArg arg, RegisterArgumentAttribute<TSet> regAttr, ITokenizerProfile profile)
        where TArg : unmanaged, Enum
        where TReg : unmanaged, Enum
        where TSet : unmanaged, Enum
    {
        var bound = RegisterTable<TReg, TSet>.GetRegisterCount(regAttr.RegisterSet, out var offset) + offset;
        var regIndex = Random.Shared.Next(offset, bound);
        var reg = Unsafe.As<int, TReg>(ref regIndex);
        var regString = $"{RegisterTable<TReg, TSet>.GetRegisterString(reg, regAttr.RegisterSet)}";
        return profile.RegisterPrefix is '\0' ? regString : $"{profile.RegisterPrefix}{regString}";
    }

    private static string GenerateSplitArgument<TArg, TReg, TSet, TRef>(SplitArgumentAttribute<TArg> splitAttr, ITokenizerProfile profile)
        where TArg : unmanaged, Enum
        where TReg : unmanaged, Enum
        where TSet : unmanaged, Enum
        where TRef : unmanaged, Enum
    {
        var imm = GenerateArgumentString<TArg, TReg, TSet, TRef>(splitAttr.ImmediateArgument, profile);
        var reg = GenerateArgumentString<TArg, TReg, TSet, TReg>(splitAttr.RegisterArgument, profile);
        return $"{imm}({reg})";
    }
}
