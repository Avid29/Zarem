// Avishai Dernis 2025

using System.IO;
using System.Threading.Tasks;
using Test.MIPS.Helpers;
using Zarem.Assembler;
using Zarem.Assembler.Config;
using Zarem.Assembler.Handlers;
using Zarem.Elf;
using Zarem.Elf.Config;
using Zarem.Emulator;
using Zarem.Emulator.Config;
using Zarem.Emulator.Interpreter;
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
        var stream = File.Open(path, FileMode.Open);

        // Run assembler, and assert successful assembly
        var config = new MIPSAssemblerConfig();
        var result = await Zarembler.AssembleAsync(stream, path, new MIPSAssmblerHandler(config), config);

        // Link
        var linkConfig = new LinkerConfig();
        var module = ZaLinker.Link(linkConfig, new MIPSLinkerHandler(), null, result.Module);
        module.EntryPoint = module.GetOrCreateSymbol("entry");

        // Format
        var elfConfig = new ElfConfig();
        var elfModule = ElfModule.Create(module, elfConfig);
        Assert.IsNotNull(elfModule);

        // Setup emulator
        var emulatorConfig = new MIPSEmulatorConfig()
        {
            HostedTraps = true
        };
        var emulator = new MIPSEmulator(emulatorConfig);
        emulator.Computer.Processor.ProgramCounter = elfModule.EntryAddress;
        emulator.Load(elfModule);

        // Setup interpreter
        var interpreter = new MARSTrapHandler(emulator.Computer);

        // Start the emulator
        emulator.Start();
    }
}
