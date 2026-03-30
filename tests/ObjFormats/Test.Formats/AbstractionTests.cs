// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Test.MIPS.Helpers;
using Zarem.Assembler;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Models;
using Zarem.Config;
using Zarem.Linker;
using Zarem.Linker.Config;
using Zarem.Linker.Handler;

namespace Test.ObjFormats;

public class AbstractionTests<TModule, TConfig>
    where TModule : IBuildModule<TModule, TConfig>
    where TConfig : FormatConfig, new()
{
    protected static async Task RunFileTest(string fileName, MipsAssemblerConfig? config = null)
    {
        // Load the file
        var path = TestFilePathing.GetAssemblyFilePath(fileName);

        // Run the test
        await RunTest(path, config);
    }

    protected static async Task RunTest(string filePath, MipsAssemblerConfig? assemblerConfig = null, TConfig? formatConfig = null)
    {
        assemblerConfig ??= new();
        formatConfig ??= new();

        var assemblyResult = await Zarembler.AssembleAsync(filePath, new MipsAssmblerHandler(assemblerConfig), assemblerConfig);
        Guard.IsNotNull(assemblyResult.Module);

        // Link
        var module = assemblyResult.Module;
        var linkConfig = new MipsLinkerConfig();
        var linkHandler = new MipsLinkerHandler(linkConfig);
        module = ZaLinker.Link(linkConfig, linkHandler, null, module);
        Guard.IsNotNull(module);

        // Extract
        var elfModule = TModule.Create(module, formatConfig);
        Guard.IsNotNull(elfModule);

        // Save (to nothing)
        var elfStream = new MemoryStream();
        await elfModule.SaveAsync(elfStream);
        elfStream.Position = 0;
        elfModule = TModule.Open("Anonymous", elfStream);
        Guard.IsNotNull(elfModule);

        // Unextract
        var reconvertedAbstractModule = elfModule.Abstract(formatConfig);
        Guard.IsNotNull(reconvertedAbstractModule);

        // Compare original and compare
        var original = module;
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
