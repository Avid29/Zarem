// Avishai Dernis 2025

using System.Threading.Tasks;
using Test.Mips.Helpers;
using Test.ObjFormats;
using Zarem.Elf;
using Zarem.Elf.Config;

namespace Test.Elf;

[TestClass]
public class AbstractionTests : AbstractionTests<ElfModule, ElfConfig>
{
    [TestMethod(TestFilePathing.BranchLiteralFile)]
    public async Task BranchLiteralFileTest() => await RunFileTest(TestFilePathing.BranchLiteralFile);

    [TestMethod(TestFilePathing.BranchRelativeFile)]
    public async Task BranchRelativeFileTest() => await RunFileTest(TestFilePathing.BranchRelativeFile);

    [TestMethod(TestFilePathing.EmptyTestFile)]
    public async Task EmptyFileTest() => await RunFileTest(TestFilePathing.EmptyTestFile);

    [TestMethod(TestFilePathing.InstructionsTestFile)]
    public async Task InstructionsFileTest() => await RunFileTest(TestFilePathing.InstructionsTestFile);

    [TestMethod(TestFilePathing.PseudoInstructionsTestFile)]
    public async Task PseudoInstructionsFileTest() => await RunFileTest(TestFilePathing.PseudoInstructionsTestFile);

    [TestMethod(TestFilePathing.PlaygroundTestFile1)]
    public async Task PlaygroundTest() => await RunFileTest(TestFilePathing.PlaygroundTestFile1);
}
