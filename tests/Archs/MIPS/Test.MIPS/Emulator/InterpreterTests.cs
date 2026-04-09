// Avishai Dernis 2025

using System.Threading.Tasks;
using Test.Mips.Helpers;
using Zarem.Assembler;
using Zarem.Emulator;
using Zarem.Emulator.Config;
using Zarem.Emulator.Machine;
using Zarem.Emulator.TrapHandlers;
using Zarem.Linker;
using Zarem.Linker.Config;

namespace Test.MIPS.Emulator;

[TestClass]
public class InterpreterTests
{
    [TestMethod]
    public async Task RunPrintIntTest()
    {
        // Load the file
        var path = TestFilePathing.GetAssemblyFilePath("emulator_tests/usercode_tests/hello_world.asm");

        // Run assembler, and assert successful assembly
        var asmConfig = new MipsAssemblerConfig();
        var asmHandler = new MipsAssemblerHandler(asmConfig);
        var result = await Zarembler.AssembleAsync(path, asmHandler, asmConfig);

        // Link
        var linkConfig = new MipsLinkerConfig();
        var linkHandler = new MipsLinkerHandler(linkConfig);
        var module = ZaLinker.Link(linkConfig, linkHandler, null, result.Module);

        // Setup emulator
        var emulatorConfig = new MIPSEmulatorConfig()
        {
            TrapHost = new ZaremTrapHandler(),
        };
        var computer = new MipsComputer(emulatorConfig);
        var emulator = new Zaremulator(computer);

        emulator.Load(module);

        // Start the emulator
        emulator.Start();
    }
}
