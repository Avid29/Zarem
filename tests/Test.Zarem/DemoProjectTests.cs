// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Zarem.Elf;
using Zarem.Emulator;
using Zarem.Emulator.Interpreter;
using Zarem.Emulator.Models.Enums;
using Zarem.MIPS;
using Zarem.Registry;
using Zarem.Serialization;

namespace Test.Zarem;

/// <summary>
/// Uses the demo projects as test sources
/// </summary>
[TestClass]
public sealed class DemoProjectTests
{
    private static string DemoFilesPathBase => Path.Combine(FindRootPath(), "demos");

    [TestInitialize]
    public void TestInit()
    {
    }

    [TestMethod]
    public async Task HelloWorld() => await RunAndCompare(Path.Combine(DemoFilesPathBase, "HelloWorld", "HelloWorld.zrmp"), "Hello World!");

    [TestMethod]
    public async Task FizzBuzz() => await RunAndCompare(Path.Combine(DemoFilesPathBase, "FizzBuzz", "FizzBuzz.zrmp"), FizzBuzzText);

    private string FizzBuzzText
    {
        get
        {
            // This is a kinda strange implementation, but it matches the
            // MIPS code most closely
            var sb = new StringBuilder();
            for(int i = 1; i <= 100; i++)
            {
                bool fizz = i % 3 == 0;
                if (fizz)
                {
                    sb.Append("Fizz");
                }

                bool buzz = i % 5 == 0;
                if (buzz)
                {
                    sb.Append("Buzz");
                }
                else if (!fizz)
                {
                    sb.Append(i);
                    sb.AppendLine();
                    continue;
                }

                sb.Append('\n');
            }

            return sb.ToString();
        }
    }

    private async Task RunAndCompare(string projectPath, string expectedOutput)
    {
        // Register plugins
        ZaremRegistry.RegisterArchitecture(new MIPSArchitectureDescriptor());
        ZaremRegistry.Formats.Register(new ElfModuleDescriptor());

        // ReDirect console output
        var consoleOutput = new StringBuilder();
        Console.SetOut(new StringWriter(consoleOutput));
        consoleOutput.Clear();

        // Load project
        var project = ProjectFactory.Load(projectPath);
        
        // Build project
        var buildResult = await project.BuildProjectAsync(true);
        var module = buildResult.OutputModule;
        Assert.IsNotNull(module);

        // Begin debug session
        var session = project.StartDebug(module);
        Assert.IsNotNull(session);

        if (session.Emulator is not MIPSEmulator mipsEmu)
        {
            Assert.Fail();
            return;
        }

        _ = new MARSTrapHandler(mipsEmu.Computer);

        // Setup comparision unpon completion
        var tcs = new TaskCompletionSource();
        mipsEmu.StateChanged += (s, state) =>
        {
            if (state is EmulatorState.Stopped)
                tcs.SetResult();
        };

        // Begin emulator and await completion
        session.Emulator.Start();
        await tcs.Task;

        Assert.AreEqual(expectedOutput, $"{consoleOutput}");
    }

    private static string FindRootPath()
    {
        string path = Directory.GetCurrentDirectory();

        // Find the Zarem root directory
        var dir = new DirectoryInfo(path);
        while (dir is not null && dir.Name != "Zarem")
            dir = dir.Parent;

        Guard.IsNotNull(dir);
        return dir.FullName;
    }
}
