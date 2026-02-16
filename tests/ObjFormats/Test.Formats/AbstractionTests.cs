// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Test.MIPS.Helpers;
using Zarem.Assembler;
using Zarem.Assembler.Config;
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
        await RunTest(stream, fileName, config);
    }

    protected static async Task RunTest(Stream stream, string? filename = null, MIPSAssemblerConfig? assemblerConfig = null, TConfig? formatConfig = null)
    {
        assemblerConfig ??= new();
        formatConfig ??= new();

        var assemblyResult = await MIPSAssembler.AssembleAsync(stream, filename, assemblerConfig);
        Guard.IsNotNull(assemblyResult.Module);

        // Extract
        var module = TModule.Create(assemblyResult.Module, formatConfig);
        Guard.IsNotNull(module);

        // 
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

        foreach(var @ref in original.References)
        {
            var matchingRef = compare.References.FirstOrDefault(r => r.Location == @ref.Location && r.Type == @ref.Type);

            Assert.IsNotNull(matchingRef);
            Assert.AreEqual(@ref.Symbol, matchingRef.Symbol);
        }
    }
}
