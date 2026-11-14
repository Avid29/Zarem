// Avishai Dernis 2025

using System.Threading.Tasks;
using Test.Mips.Helpers;
using Test.ObjFormats;
using Zarem.Assembler.Logging.Enum;
using Zarem.Mips.Models.Enums;
using Zarem.Mips.Models.Versioning.Enums;

namespace Test.Elf;

[TestClass]
public class ElfTests : AssemblerTests
{
    private const string InvalidInstruction = "xkcd $t0, $t1, 0";
    private const string MissingInstruction = "$s2, $t4, $t2";
    private const string InvalidLabelNum = "2point: nop";
    private const string ExtraArgError = "add $s0, $t0, $t2, 2";
    private const string MissingArgError = "add $s0, $t0";
    private const string NumericalRegister = "addi $s0, $16, 16";
    private const string NumericalRegisterError = "addi $s0, $56, 16";
    private const string DisabledPseudoInstructions = "move $s0, $t0";
    private const string NotInVersion = "sync";

    [TestMethod(DisplayName = nameof(InvalidInstruction))]
    public async Task InvalidInstructionTest() => await RunStringTest(InvalidInstruction, expected: LogId.InvalidInstructionName);

    [TestMethod(DisplayName = nameof(MissingInstruction))]
    public async Task MissingInstructionTest() => await RunStringTest(MissingInstruction, expected: LogId.UnexpectedToken);

    [TestMethod(DisplayName = nameof(InvalidLabelNum))]
    public async Task InvalidLabelNumTest() => await RunStringTest(InvalidLabelNum, expected: LogId.IllegalSymbolName);

    [TestMethod(DisplayName = nameof(ExtraArgError))]
    public async Task ExtraArgErrorTest() => await RunStringTest(ExtraArgError, expected: LogId.InvalidInstructionArgCount);

    [TestMethod(DisplayName = nameof(MissingArgError))]
    public async Task MissingArgErrorTest() => await RunStringTest(MissingArgError, expected: LogId.InvalidInstructionArgCount);

    [TestMethod(DisplayName = nameof(NumericalRegister))]
    public async Task NumericalRegisterTest() => await RunStringTest(NumericalRegister);

    [TestMethod(DisplayName = nameof(NumericalRegisterError))]
    public async Task NumericalRegisterErrorTest() => await RunStringTest(NumericalRegisterError, expected: LogId.InvalidRegisterArgument);

    [TestMethod(DisplayName = nameof(DisabledPseudoInstructions))]
    public async Task DisabledPseudoInstructionsErrorTest() =>
        await RunStringTest(DisabledPseudoInstructions, new() { PseudoInstructionPermissibility = PseudoInstructionPermissibility.Blacklist }, LogId.DisabledFeatureInUse);

    [TestMethod(DisplayName = nameof(NotInVersion))]
    public async Task NotInVersionTest() => await RunStringTest(NotInVersion, new(new(MipsGeneration.MipsI)), LogId.NotInVersion);

    [TestMethod(DisplayName = TestFilePathing.BranchLiteralFile)]
    public async Task BranchLiteralFileTest() => await RunFileTest(TestFilePathing.BranchLiteralFile);

    [TestMethod(DisplayName = TestFilePathing.BranchRelativeFile)]
    public async Task BranchRelativeFileTest() => await RunFileTest(TestFilePathing.BranchRelativeFile);

    [TestMethod(DisplayName = TestFilePathing.EmptyTestFile)]
    public async Task EmptyFileTest() => await RunFileTest(TestFilePathing.EmptyTestFile);

    [TestMethod(DisplayName = TestFilePathing.InstructionsTestFile)]
    public async Task InstructionsFileTest() => await RunFileTest(TestFilePathing.InstructionsTestFile);

    [TestMethod(DisplayName = TestFilePathing.PseudoInstructionsTestFile)]
    public async Task PseudoInstructionsFileTest() => await RunFileTest(TestFilePathing.PseudoInstructionsTestFile);

    [TestMethod(DisplayName = TestFilePathing.PlaygroundTestFile1)]
    public async Task PlaygroundTest() => await RunFileTest(TestFilePathing.PlaygroundTestFile1);

    [TestMethod(DisplayName = TestFilePathing.CompositeFailTestFile)]
    public async Task CompositeFailTest() => await RunFileTest(TestFilePathing.CompositeFailTestFile,
        (LogId.InvalidInstructionArgCount, 13),
        (LogId.InvalidInstructionName, 15),
        (LogId.UnexpectedToken, 18),
        (LogId.InvalidRegisterArgument, 23), // Debatably should be an InvalidAddressOffsetArgument error
        (LogId.ZeroRegWriteback, 28),
        (LogId.IntegerTruncated, 29),
        (LogId.InvalidInstructionArg, 30),
        (LogId.InvalidRegisterArgument, 31),
        (LogId.InvalidCharLiteral, 34));

    [TestMethod(DisplayName = TestFilePathing.DuplicateSymbolFile)]
    public async Task DuplicateSymbolTest() => await RunFileTest(TestFilePathing.DuplicateSymbolFile,
        (LogId.DuplicateSymbolDefinition, 14));

    [TestMethod(DisplayName = TestFilePathing.SubtractAddressFile)]
    public async Task SubtractAddressTest() => await RunFileTest(TestFilePathing.SubtractAddressFile,
        (LogId.InvalidExpressionOperation, 13));
}
