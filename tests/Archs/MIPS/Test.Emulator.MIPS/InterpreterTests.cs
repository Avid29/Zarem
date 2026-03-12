// Avishai Dernis 2025

using System.IO;
using System.Threading.Tasks;
using Test.MIPS.Helpers;
using Zarem.Assembler;
using Zarem.Assembler.Config;
using Zarem.Assembler.Handlers;
using Zarem.Emulator;
using Zarem.Emulator.Config;
using Zarem.Emulator.TrapHandlers;
using Zarem.Linker;
using Zarem.Linker.Config;
using Zarem.Linker.Handler;

namespace Test.Emulator.MIPS;

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
        var asmHandler = new MipsAssmblerHandler(asmConfig);
        var result = await Zarembler.AssembleAsync(path, asmHandler, asmConfig);

        // Link
        var linkConfig = new MipsLinkerConfig();
        var linkHandler = new MipsLinkerHandler(linkConfig);
        var module = ZaLinker.Link(linkConfig, linkHandler, null, result.Module);

        // Setup emulator
        var emulatorConfig = new MIPSEmulatorConfig()
        {
            HostedTraps = true
        };
        var emulator = new MIPSEmulator(emulatorConfig);

        emulator.Load(module);

        // Setup interpreter
        var interpreter = new MARSTrapHandler(emulator.Computer);

        // Start the emulator
        emulator.Start();
    }
}
