// Adam Dernis 2024

using CommunityToolkit.Diagnostics;
using Zarem.Assembler.Extensions;
using Zarem.Assembler.Extensions.System;
using Zarem.Assembler.Logging.Enum;
using Zarem.Assembler.Models.Directives;
using Zarem.Assembler.Models.Directives.Abstract;
using Zarem.Assembler.Parsers;
using Zarem.Assembler.Tokenization.Models;
using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Models;
using Zarem.Models.Tables.Enums;

namespace Zarem.Assembler;

public partial class Zarembler
{
    private void AlignmentPass(AssemblyLine line)
    {
        // Parse as macro
        if (line.Type is LineType.Macro)
        {
            HandleMacro(line);
            return;
        }

        // Create symbol if line is labeled
        if (line.Label is not null)
            DefineLabel(line.Label);

        // Pad instruction sized allocation if instruction is present
        if (line.Type is LineType.Instruction)
        {
            Guard.IsNotNull(line.Instruction);

            // On the alignment pass, just reserve space for the instruction
            var size = _archHandler.GetInstructionSize(line);
            _activeSection.Reserve(size);
        }
        
        // Make allocations if directive is present
        // NOTE: Directive allocations are made in both passes
        // Issues are only logged on the second pass though
        if (line.Type is LineType.Directive)
            HandleDirective(line, false);

        // If args is non-zero while the line type is none,
        // random garbage is in the file.
        if (line.Type is LineType.None && line.Args.Count is not 0)
        {
            _logger.Log(Severity.Error, LogId.UnexpectedToken, line.Args[0].Tokens[0], "UnexpectedToken", line.Args[0].Tokens[0]);
        }
    }

    private void RealizationPass(AssemblyLine line)
    {
        switch (line.Type)
        {
            case LineType.Instruction:
                RealizeInstruction(line);
                return;
                
            // Make allocations if directive is present
            // NOTE: Directive allocations are made in both passes
            case LineType.Directive:
                HandleDirective(line);
                return;

            // Macros can be skipped on realization pass
            case LineType.Macro:
                return;
        }
    }

    private void HandleMacro(AssemblyLine line)
    {
        // Grab name and expression
        var name = line.Macro;
        var expression = line.Args[0].Tokens;
        expression = expression.TrimType(TokenType.Assign, out var trimmed);

        // Ensure the name is not null, and that an assignment
        // token was trimmed from the expression.
        Guard.IsNotNull(name);
        Guard.IsNotNull(trimmed);

        if (expression.IsEmpty)
        {
            _logger.Log(Severity.Error, LogId.MacroMissingValue, expression[0], "SymbolMissingValue", name);
            return;
        }
        
        if (!ExpressionParser.TryParse(expression, out var result, _module.Symbols, _logger.Parent))
            return;
        
        if (result.IsSymbolic)
        {
            _logger.Log(Severity.Error, LogId.MacroCannotBeRelocatable, expression[0], "NoRelocatableMacros");
            return;
        }

        // TODO: Macros

        //var macroSym = new MacroSymbol(name, )
        //DefineSymbol(name, result.Addend, SymbolType.Macro);
    }

    private void RealizeInstruction(AssemblyLine line)
    {
        // Try to parse the line
        var instruction = _archHandler.ParseInstruction(line, CurrentAddress, _module.Symbols, _logger?.Parent);
        if (instruction is null)
        {
            // Instruction parsing failed. Append a NOP, and get on it with it
            _activeSection.Append(_archHandler.GetNOP());
            return;
        }

        // Track relocatable reference
        if (instruction.Reference is not null)
        {
            _activeSection.AddRelocation(instruction.Reference);
        }

        // Append instruction to active segment
        _activeSection.Append(instruction.RealizeBytes());
    }

    private void HandleDirective(AssemblyLine line, bool log = true)
    {
        var parser = new DirectiveParser(_module.Symbols, Config, log ? _logger.Parent : null);

        var name = line.Directive;
        if (name is null || !parser.TryParseDirective(line, out var directive))
            return;

        Guard.IsNotNull(directive);
        ExecuteDirective(directive);
    }

    private void ExecuteDirective(Directive directive)
    {
        switch (directive)
        {
            case GlobalDirective global:
                var symbol = _module.GetOrCreateSymbol(global.Symbol);
                symbol.Binding = SymbolBinding.Global;
                break;
            case SectionDirective section:
                _activeSection = _module.GetOrCreateSection(section.Name);
                break;
            case AlignDirective align:
                _activeSection.Align((uint)(1 << (int)align.Boundary));    // TODO: Sort out typing here
                break;
            case DataDirective data:
                _activeSection.Append(data.Data);
                break;
        }
    }

    /// <summary>
    /// Defines a label at the current address.
    /// </summary>
    /// <remarks>
    /// At this stage, the label is expected to be passed in with a tailing ':' that will be trimmed.
    /// The method will still work if the semicolon is pre-trimmed.
    /// </remarks>
    /// <param name="label">The name of the symbol.</param>
    private bool DefineLabel(Token label) => DefineSymbol(label, _activeSection.CurrentAddress, SymbolType.Label);

    /// <summary>
    /// Defines a symbol.
    /// </summary>
    /// <param name="label">The name of the symbol.</param>
    /// <param name="address">The value of the symbol.</param>
    /// <param name="type">The symbol type.</param>
    /// <returns>True if successful, false on failure.</returns>
    private bool DefineSymbol(Token label, Address address, SymbolType type)
    {
        // Ensure the symbol has a valid name
        if (!ValidateSymbolName(label, out var name))
            return false;

        // Define the symbol or update by adding flags, address or type.
        // NOTE: The type can only be updated if it is currently unknown
        //       and the address can only be updated if it's undeclared/external.
        if (_module.Symbols.TryGetValue(name, out var existing) && existing.IsDefined)
        {
            _logger?.Log(Severity.Error, LogId.DuplicateSymbolDefinition, label, "SymbolAlreadyDefined", name);
            return false;
        }

        var symbol = _module.GetOrCreateSymbol(name);
        symbol.Address = address;
        symbol.Type = type;

        return true;
    }

    private bool ValidateSymbolName(Token symbol, out string name)
    {
        name = symbol.Source.TrimEnd(':');
        if (char.IsDigit(name[0]))
        {
            _logger?.Log(Severity.Error, LogId.IllegalSymbolName, symbol, "SymbolsCannotBeginWithDigits", name);
            return false;
        }

        foreach (char c in name)
        {
            if (!char.IsLetterOrDigit(c) && c is not '_')
            {
                _logger?.Log(Severity.Error, LogId.IllegalSymbolName, symbol, "SymbolCannotContain", name, c);
                return false;
            }
        }

        return true;
    }
}
