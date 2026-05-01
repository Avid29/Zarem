// Avishai Dernis 2026

using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        Assert.IsTrue(File.Exists(expectedScriptPath));

        var actualOutput = await RunProjectAsync(projectPath);
        var expectedScript = File.ReadAllText(expectedScriptPath);
        var expectedOutput = await RunExpectedAsync(expectedScript);
        Assert.AreEqual(expectedOutput, actualOutput);
    }

    protected static IEnumerable<string> GetProjectPaths(string arch)
        => Directory.EnumerateFiles(Path.Combine(FindRootPath(), "demos", arch), "*.zrmp", SearchOption.AllDirectories);

    private static async Task<string?> RunProjectAsync(string projectPath)
    {
        // Redirect console output
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

        if (session.Emulator.Computer is not MipsComputer mipsComp)
        {
            Assert.Fail();
            return null;
        }

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

        return $"{consoleOutput}";
    }

    private static async Task<string?> RunExpectedAsync(string scriptCode)
    {
        // Redirect console output
        var consoleOutput = new StringBuilder();
        Console.SetOut(new StringWriter(consoleOutput));
        consoleOutput.Clear();

        await CSharpScript.EvaluateAsync<string>(scriptCode);
        return $"{consoleOutput}";
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
