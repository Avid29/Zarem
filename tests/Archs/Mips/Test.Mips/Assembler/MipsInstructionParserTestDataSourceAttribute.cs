// Avishai Dernis 2026

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Test.Archs.Assembler;
using Test.Mips.Helpers;
using Zarem.Assembler;
using Zarem.Assembler.Models.Tables;
using Zarem.Mips.Assembler.Models.Meta;
using Zarem.Mips.Assembler.Models.Tables;
using Zarem.Mips.Models.Enums;
using Zarem.Mips.Models.Instructions.Enums;
using Zarem.Mips.Models.Instructions.Enums.Registers;
using Zarem.Mips.Models.Versioning;

namespace Test.Mips.Assembler;

public class MipsInstructionParserTestDataSourceAttribute : InstructionParserTestDataSourceAttribute
{
    private readonly MipsVersionInfo _versionInfo;

    public MipsInstructionParserTestDataSourceAttribute(string versionString)
    {
        _versionInfo = MipsVersionInfo.Parse(versionString);
    }

    public override IEnumerable<object?[]> GetData(MethodInfo methodInfo)
    {
        var formatTable = new FormatTable<MipsFloatFormat>();
        var table = new MipsInstructionTable(new(_versionInfo));
        var instructions = table.GetInstructions()
            .Where(i => i.IsValidFor(_versionInfo));

        foreach (var instruction in instructions)
        {
            // TODO: Disassembling pseudo instructions
            if (instruction is MipsPseudoInstructionMeta)
                continue;

            // Apply format to instruction name, if applicable
            var name = instruction.Name;
            if (instruction is MipsFloatInstructionMeta fMeta)
            {
                name = formatTable.ApplyFormat(name, ArgGenerator.RandomFormat(fMeta.SupportedFormats));
            }

            // Generate instruction
            StringBuilder line = new(name);
            line.Append(' ');

            foreach (var arg in instruction.ArgumentPattern)
            {
                var argString = GenerateArgumentString<MipsArgument, MipsGpRegister, MipsRegisterSet, MipsReferenceType>(arg, MipsTokenizerProfile.Default);
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
