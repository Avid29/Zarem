// Avishai Dernis 2026

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Test.Archs.Assembler;
using Test.RiscV.Helpers;
using Zarem.Assembler.Models.Tables;
using Zarem.Models.Versioning;
using Zarem.RiscV.Assembler.Models.Meta;
using Zarem.RiscV.Assembler.Models.Meta.Extensions;
using Zarem.RiscV.Assembler.Models.Tables;
using Zarem.RiscV.Models.Enums;
using Zarem.RiscV.Models.Instructions.Enums;
using Zarem.RiscV.Models.Instructions.Enums.Registers;

namespace Test.RiscV.Assembler;

public class RiscVInstructionParserTestDataSource : InstructionParserTestDataSourceAttribute
{
    private readonly RiscVVersionInfo _versionInfo;

    public RiscVInstructionParserTestDataSource(string versionString)
    {
        _versionInfo = RiscVVersionInfo.Parse(versionString);
    }

    public override IEnumerable<object?[]> GetData(MethodInfo methodInfo)
    {
        var formatTable = new FormatTable<RiscVFloatFormat>();
        var roundingModeTable = new FormatTable<RiscVRoundingMode>("rm");
        var table = new RiscVInstructionTable(new(_versionInfo));
        var instructions = table.GetInstructions()
            .Where(i => i.IsValidFor(_versionInfo));

        foreach (var instruction in instructions)
        {
            // TODO: Disassembling pseudo instructions
            if (instruction is RiscVPseudoInstructionMeta)
                continue;

            // Apply format to instruction name, if applicable
            var name = instruction.Name;
            if (instruction is RiscVFloatInstructionMeta fMeta)
            {
                name = formatTable.ApplyFormat(name, RiscVFloatFormat.Single);
                name = roundingModeTable.ApplyFormat(name, RiscVRoundingMode.Dynamic);
            }

            // Generate instruction
            StringBuilder line = new(name);
            line.Append(' ');

            foreach (var arg in instruction.ArgumentPattern)
            {
                line.Append(arg switch
                {
                    RiscVArgument.RD or RiscVArgument.RS1 or RiscVArgument.RS2 or RiscVArgument.RDRS1 => GetRegisterString(ArgGenerator.RandomRegister(), RiscVRegisterSet.GeneralPurpose),
                    RiscVArgument.FRD or RiscVArgument.FRS1 or RiscVArgument.FRS2 or RiscVArgument.FRS3 => GetRegisterString(ArgGenerator.RandomRegister(), RiscVRegisterSet.FloatingPoints),
                    RiscVArgument.CompressedRD or RiscVArgument.CompressedRS1 or RiscVArgument.CompressedRS2 or RiscVArgument.CompressedRDRS1 => GetRegisterString(ArgGenerator.RandomCompressedRegister(), RiscVRegisterSet.CompressedGeneralPurpose),
                    RiscVArgument.Immediate or RiscVArgument.StoreOffset or RiscVArgument.Csr => $"{ArgGenerator.RandomImm12()}",
                    RiscVArgument.UpperImmediate => $"{ArgGenerator.RandomImm20()}",
                    RiscVArgument.BranchOffset => $"{ArgGenerator.RandomBranchOffset()}",
                    RiscVArgument.JumpOffset => $"{ArgGenerator.RandomJumpOffset()}",
                    RiscVArgument.FullImmediate => $"{ArgGenerator.RandomFullImm()}",
                    RiscVArgument.CompressedImmediate => $"{ArgGenerator.RandomCompressedImm()}",
                    RiscVArgument.CompressedBranchOffset => $"{ArgGenerator.RandomCompressedBranchOffset()}",
                    RiscVArgument.CompressedJumpOffset => $"{ArgGenerator.RandomCompressedJumpOffset()}",
                    RiscVArgument.UImm5 => $"{ArgGenerator.RandomShamt()}",
                    RiscVArgument.MemoryLoad or RiscVArgument.MemoryStore => $"{ArgGenerator.RandomImm12()}({GetRegisterString(ArgGenerator.RandomRegister(), RiscVRegisterSet.GeneralPurpose)})",
                    _ => throw new NotImplementedException(),
                });

                line.Append(", ");
            }

            // Remove final ", "
            if (instruction.ArgumentPattern.Length > 0)
                line.Remove(line.Length - 2, 2);

            // Return test case
            yield return [$"{line}"];
        }
    }

    private static string GetRegisterString(RiscVGpRegister register, RiscVRegisterSet set) => $"{RegisterTable<RiscVGpRegister, RiscVRegisterSet>.GetRegisterString(register, set)}";
}
