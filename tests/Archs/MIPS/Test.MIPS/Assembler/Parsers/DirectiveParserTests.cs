// Avishai Dernis 2024

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using Zarem.Assembler;
using Zarem.Assembler.Models.Directives;
using Zarem.Assembler.Models.Directives.Abstract;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization;

namespace Test.MIPS.Assembler.Parsers;

// TODO: Test expressions in directive args

[TestClass]
public class DirectiveParserTests
{
    public sealed record DirectiveDataTestCase(
        string Input,
        params byte[] Expected);

    public static IEnumerable<object[]> DataTestsList
    {
        get
        {
            yield return [new DirectiveDataTestCase(".space 10", new byte[10])];
            yield return [new DirectiveDataTestCase(".byte 10", 10)];
            yield return [new DirectiveDataTestCase(".word 10", 0, 0, 0, 10)];
            yield return [new DirectiveDataTestCase(".byte 10, 10", 10, 10)];
            yield return [new DirectiveDataTestCase(".float 10", EnsureEndianness(BitConverter.GetBytes(10f)))];
            yield return [new DirectiveDataTestCase(".float 1.5", EnsureEndianness(BitConverter.GetBytes(1.5f)))];
            yield return [new DirectiveDataTestCase(".double 10", EnsureEndianness(BitConverter.GetBytes(10d)))];
            yield return [new DirectiveDataTestCase(".double 1.5", EnsureEndianness(BitConverter.GetBytes(1.5d)))];
            yield return [new DirectiveDataTestCase(".ascii \"Test String\"", Encoding.ASCII.GetBytes("Test String"))];
            yield return [new DirectiveDataTestCase(".asciiz \"Test String\"", Encoding.ASCII.GetBytes("Test String\0"))];
            yield return [new DirectiveDataTestCase(".utf16 \"Test String\"", Encoding.BigEndianUnicode.GetBytes("Test String"))];
            yield return [new DirectiveDataTestCase(".utf16z \"Test String\"", Encoding.BigEndianUnicode.GetBytes("Test String\0"))];
        }
    }

    private const string Global = ".globl main";
    private const string DefinePrintInt = ".def SYS_PRINT_INT, 1";

    [TestMethod(Global)]
    public void GlobalTest() => RunGlobalTest(Global, "main");

    [TestMethod(DefinePrintInt)]
    public void DefinePrintIntText() => RunDefineTest(DefinePrintInt, "SYS_PRINT_INT", 1);

    [DataTestMethod]
    [DynamicData(nameof(DataTestsList))]
    public void DirectiveDataTest(DirectiveDataTestCase @case) =>
        RunDataTest(@case.Input, @case.Expected);

    private static Directive ParseDirective(string input)
    {
        var parser = new DirectiveParser();

        // Tokenize directive
        var line = Tokenizer.TokenizeLine(input, MipsTokenizerProfile.Default, nameof(RunGlobalTest))[0];
        if (line.Directive is null)
            Assert.Fail();

        if (!parser.TryParseDirective(line, out var directive))
            Assert.Fail();

        if (directive is null)
            Assert.Fail();

        return directive;
    }

    private static void RunGlobalTest(string input, string expected)
    {
        // Get directive and validate type
        var directive = ParseDirective(input);
        if (directive is not GlobalDirective)
            Assert.Fail();

        var actual = ((GlobalDirective)directive).Symbol;
        Guard.IsNotNull(actual);

        Assert.AreEqual(expected, actual);
    }

    private static void RunDataTest(string input, params byte[] expected)
    {
        // Get directive and validate type
        var directive = ParseDirective(input);
        if (directive is not DataDirective datDir)
        {
            Assert.Fail();
            return;
        }

        var actual = datDir.Data;
        Guard.IsNotNull(actual);

        Assert.AreEqual(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i]);
        }
    }

    private static void RunDefineTest(string input, string name, long value)
    {
        // Get directive and validate type
        var directive = ParseDirective(input);
        if (directive is not DefineDirective defDir)
        {
            Assert.Fail();
            return;
        }

        Assert.AreEqual(name, defDir.Name.Source);
        Assert.AreEqual(value, defDir.Value);
    }

    private static byte[] EnsureEndianness(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        return bytes;
    }
}
