// Adam Dernis 2024

using System.Text;
using Test.Assembler.MIPS.Live.Enums;
using Zarem.Assembler;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Disassembler;
using Zarem.Models.Instructions;
using Zarem.Models.Instructions.Enums;
using Zarem.Assembler.Handlers;
using Zarem.Assembler.Config;
using Zarem.Assembler.Tokenization;
using Zarem.Assembler.Models;

#if DEBUG
using Zarem.Disassembler.Services;
using Zarem.Services;
#endif

namespace Test.Assembler.MIPS.Live;

public class Program()
{
    private TestMode _mode = TestMode.Line;

    static async Task Main()
    {
#if DEBUG
        ServiceCollection.DisassemblerService = new MIPSDisassemblerService(new());
#endif
        var program = new Program();
        while (true)
        {
            await program.Loop();
        }
    }

    private async Task Loop()
    {
        var message = _mode switch
        {
            TestMode.Line => "Enter line of assembly:",
            TestMode.Expression => "Enter expression:",
            _ => throw new ArgumentOutOfRangeException(nameof(_mode)),
        };

        Console.WriteLine(message);
        var line = Console.ReadLine();

        if (line is null)
            return;

        if (line.StartsWith('>'))
        {
            HandleCommand(line);
            return;
        }

        _ = _mode switch
        {
            TestMode.Line => await TestLine(line),
            TestMode.Expression => TestExpression(line),
            _ => false,
        };
    }

    private async Task<bool> TestLine(string line)
    {
        var stream = new MemoryStream(Encoding.Default.GetBytes(line));

        var config = new MIPSAssemblerConfig();
        var result = await Zarembler.AssembleAsync(stream, null, new MIPSAssmblerHandler(config), config);

        if (!result.Failed)
        {
            var textSect = result.Module.GetOrCreateSection(".text");
            var textStream = textSect.Stream;
            textStream.Position = 0;

            Console.Write("\nBinary: ");
            uint inst = 0;
            for (int i = 0; textStream.Position != textStream.Length; i++)
            {
                int x = textStream.ReadByte();

                Console.Write($"{x:X2} ");
                if (i % 4 is 3)
                    Console.Write("  ");
                if (i % 8 is 7)
                    Console.WriteLine();

                inst = inst << 8;
                inst += (uint)x;
            }

            Console.Write("\n\nDisassembly: ");
            var disassembly = new MIPSDisassembler(new()).Disassemble((MIPSInstruction)inst);
            Console.Write(disassembly);
        }
        else
        {
            Console.WriteLine("\nAssembly failed:");
        }

        Console.WriteLine();
        foreach (var error in result.Logs)
        {
            (string message, ConsoleColor color) = error.Severity switch 
            {
                Severity.Error => ("Error", ConsoleColor.Red),
                Severity.Warning => ("Warning", ConsoleColor.Yellow),
                Severity.Message => ("Message", ConsoleColor.Blue),
                _ => ("Something", ConsoleColor.Magenta)
            };

            Console.ForegroundColor = color;
            Console.WriteLine($"{message} - {error.Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        Console.WriteLine("\n");
        return !result.Failed;
    }

    private bool TestExpression(string line)
    {
        var tokens = Tokenizer.TokenizeLine(line, mode: TokenizerMode.Expression);
        bool success = ExpressionParser.TryParse(tokens.Tokens, out var result, null, null);

        var message = success ? $"{line} evaluated to {result}" :
                                $"{line} could not be evaluated";
        Console.WriteLine(message);

        return success;
    }

    void HandleCommand(string line)
    {
        line = line.Trim('>');
        line = line.Trim();
        var cmdArgs = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        switch (cmdArgs[0])
        {
            case "mode":
                if (cmdArgs.Length != 2)
                {
                    Console.WriteLine("Mode command requires exactly 1 argument 'mode'.");
                    return;
                }

                SwapMode(cmdArgs[1]);
                break;
            case "dump":
                if (cmdArgs.Length is not (2 or 3))
                {
                    Console.WriteLine("Dump command requires 1 or 2 argument 'table' and 'MIPS Version'.");
                    return;
                }

                var version = MipsVersion.MipsII;
                if (cmdArgs.Length is 3)
                {
                    version = (MipsVersion)int.Parse(cmdArgs[2]);
                }

                Dump(cmdArgs[1], version);
                break;
        }
    }

    void SwapMode(string mode)
    {
        mode = mode.Trim().ToLower();
        TestMode? newMode = mode switch
        {
            "line" => TestMode.Line,
            "expression" => TestMode.Expression,
            _ => null,
        };

        if (newMode is null)
        {
            Console.WriteLine($"Unrecognized mode {mode}. Mode not changed");
            return;
        }

        _mode = newMode.Value;
        Console.WriteLine($"Mode swapped to {_mode} mode");
    }

    void Dump(string tableArg, MipsVersion version = MipsVersion.MipsII)
    {
        tableArg = tableArg.Trim().ToLower();
        switch (tableArg)
        {
            case "instructions":
                var instructions = new InstructionTable(new(version)).GetInstructions().OrderBy(x => x.Name);
                foreach (var instr in instructions)
                {
                    if (instr.IsPseudoInstruction)
                        Console.Write("* ");

                    Console.WriteLine(instr.UsagePattern);
                }
                Console.WriteLine("* Pseudo Instruction");
                break;
        }
    }
}
