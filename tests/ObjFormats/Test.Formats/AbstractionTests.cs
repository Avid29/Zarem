// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Test.MIPS.Helpers;
using Zarem.Assembler;
using Zarem.Assembler.Config;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Models;
using Zarem.Config;

namespace Test.ObjFormats;

public class AbstractionTests<TModule, TConfig>
    where TModule : IBuildModule<TModule, TConfig>
    where TConfig : FormatConfig, new()
{
    protected static async Task RunFileTest(string fileName, MIPSAssemblerConfig? config = null)
    {
        // Load the file
        var path = TestFilePathing.GetAssemblyFilePath(fileName);
        var stream = File.Open(path, FileMode.Open);

        // Run the test
        await RunTest(stream, path, config);
    }

    protected static async Task RunTest(Stream stream, string? filePath = null, MIPSAssemblerConfig? assemblerConfig = null, TConfig? formatConfig = null)
    {
        assemblerConfig ??= new();
        formatConfig ??= new();

        var assemblyResult = await Zarembler.AssembleAsync(stream, filePath, new MIPSAssmblerHandler(assemblerConfig), assemblerConfig);
        Guard.IsNotNull(assemblyResult.Module);

        // Extract
        var module = TModule.Create(assemblyResult.Module, formatConfig);
        Guard.IsNotNull(module);

        // Save (to nothing)
        await module.SaveAsync(new MemoryStream());

        // Unextract
        var reconvertedAbstractModule = module.Abstract(formatConfig);
        Guard.IsNotNull(reconvertedAbstractModule);

        // Compare original and compare
        var original = assemblyResult.Module;
        var compare = reconvertedAbstractModule;

        foreach (var (key, value) in original.Symbols)
        {
            if (!compare.Symbols.TryGetValue(key, out var symbol))
                Assert.Fail();

            Assert.AreEqual(value.IsDefined, symbol.IsDefined);
        }

        var sourceRelocations = original.Sections.Values.SelectMany(x => x.Relocations);
        var compareRelocations = compare.Sections.Values.SelectMany(x => x.Relocations);
        foreach(var @ref in sourceRelocations)
        {
            var matchingRef = compareRelocations.FirstOrDefault(r =>
                r.Location.Section?.Name == @ref.Location.Section?.Name &&
                r.Location.Offset == @ref.Location.Offset &&
                r.Type == @ref.Type);

            Assert.IsNotNull(matchingRef);
            Assert.AreEqual(@ref.SymbolName, matchingRef.SymbolName);
        }
    }
}
