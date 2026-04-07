// Avishai Dernis 2025

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Mips.Helpers;
using Zarem.Assembler;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Logging.Enum;

namespace Test.ObjFormats;

public class AssemblerTests
{
    protected static async Task RunFileTest(string fileName, params (LogId, long)[] expected)
    {
        // Load the file
        var path = TestFilePathing.GetAssemblyFilePath(fileName);
        var stream = File.Open(path, FileMode.Open);
        var moduleId = Path.GetFileNameWithoutExtension(path);

        // Run the test
        await RunTest(stream, moduleId, null, expected);
    }

    protected static async Task RunStringTest(string str, MipsAssemblerConfig? config = null, params LogId[] expected)
    {
        // Wrap the test in a stream and run the test
        var stream = new MemoryStream(Encoding.Default.GetBytes(str));
        await RunTest(stream, null, config, [.. expected.Select((x) => (x, 0L))]);
    }

    protected static async Task RunTest(Stream stream, string? moduleId, MipsAssemblerConfig? config = null, params (LogId, long)[] expected)
    {
        // Load output file
        //var output = TestFilePathing.GetMatchingObjectFilePath(filename);
        //Stream result = File.Open(output, FileMode.OpenOrCreate);

        // Run assembler
        config ??= new();
        var result = await Zarembler.AssembleAsync(stream, moduleId, new MipsAssemblerHandler(config), config);

        // Find expected errors, warnings, and messages
        if (expected.Length == result.Logs.Count)
        {
            foreach (var (code, line) in expected)
            {
                var logEntry = result.Logs.FirstOrDefault(x => x.Code.Id == (uint)code && x.Location?.Line == line);
                Assert.IsNotNull(logEntry, $"Could not find matching {code} error on line {line + 1}");
            }
        }

        // Don't run output validation for fileless tests
        if (moduleId is null)
            return;

        // Assembly failed. No expected output
        if (result.Failed)
            return;

        // Write the module and assert validity

        // TODO:
    }
}
