// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Zarem.Emulator.Machine;
using Zarem.Emulator.Models.Enums;
using Zarem.Serialization;

namespace Test.Zarem;

/// <summary>
/// Uses the demo projects as test sources
/// </summary>
public abstract class ProjectTestsBase
{
    public static async Task RunProjectTest(string projectPath)
    {
        var dir = Path.GetDirectoryName(projectPath);
        Assert.IsNotNull(dir);

        var expectedScriptPath = Path.Combine(dir, "Test.cs");
        var testInputsPath = Path.Combine(dir, "Tests.json");

        Assert.IsTrue(File.Exists(expectedScriptPath), "Test.cs missing.");
        var expectedScript = await File.ReadAllTextAsync(expectedScriptPath);

        string[][] testScenarios = [[""]];
        if (File.Exists(testInputsPath))
        {
            var json = await File.ReadAllTextAsync(testInputsPath);
            testScenarios = JsonSerializer.Deserialize<string[][]>(json) ?? [[""]];
        }

        foreach (var inputLines in testScenarios)
        {
            var simulatedInput = string.Join(Environment.NewLine, inputLines);

            // Run the actual emulator
            var actualOutput = await ExecuteWithRedirectedStreams(simulatedInput, async () => {
                await RunProjectAsync(projectPath);
            });

            // Run the expected C# script
            var expectedOutput = await ExecuteWithRedirectedStreams(simulatedInput, async () => {
                await CSharpScript.EvaluateAsync(expectedScript);
            });


            Assert.AreEqual(expectedOutput, actualOutput, $"Mismatch for input: {simulatedInput}");
        }
    }

    protected static IEnumerable<string> GetProjectPaths(string arch)
        => Directory.EnumerateFiles(Path.Combine(FindRootPath(), "demos", arch), "*.zrmp", SearchOption.AllDirectories);

    private static async Task<string> ExecuteWithRedirectedStreams(string input, Func<Task> action)
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;

        try
        {
            using var reader = new StringReader(input);
            using var writer = new StringWriter();

            Console.SetIn(reader);
            Console.SetOut(writer);

            await action();

            return writer.ToString();
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }
    }

    private static async Task RunProjectAsync(string projectPath)
    {
        // Load project
        var project = ProjectFactory.Load(projectPath);

        // Build project
        var buildResult = await project.BuildProjectAsync(true);
        var module = buildResult.OutputModule;
        Assert.IsNotNull(module);

        // Begin debug session
        var session = project.StartDebug(module);
        Assert.IsNotNull(session);

        // Setup comparision unpon completion
        var tcs = new TaskCompletionSource();
        session.Emulator.StateChanged += (s, state) =>
        {
            if (state is EmulatorState.Stopped)
                tcs.SetResult();
        };

        // Begin emulator and await completion
        // (With a timeout to prevent hanging and because input is not yet handled)
        session.Emulator.Start();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
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
