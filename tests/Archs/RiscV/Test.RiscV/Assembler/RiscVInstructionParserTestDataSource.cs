// Avishai Dernis 2026

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Test.Archs.Assembler;
using Zarem.Assembler;
using Zarem.Assembler.Models.Tables;
using Zarem.Models.Versioning;
using Zarem.RiscV.Assembler.Models.Meta;
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
                var argString = GenerateArgumentString<RiscVArgument, RiscVGpRegister, RiscVRegisterSet, RiscVReferenceType>(arg, RiscVTokenizerProfile.Default);
                line.Append(argString);
                line.Append(", ");
            }

            // Remove final ", "
            if (instruction.ArgumentPattern.Length > 0)
                line.Remove(line.Length - 2, 2);

            // Return test case
            yield return [$"{line}"];
        }
    }
}
