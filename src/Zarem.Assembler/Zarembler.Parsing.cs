// Avishai Dernis 2024

using CommunityToolkit.Diagnostics;
using System.Net.Sockets;
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
        line.Address = CurrentAddress;

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

            // Check for an extra comma at the end of the assembly line. זה אסור
            // Yes, all this just to check that
            if (line.Args.Count is > 0)
            {
                var lastArgIndex = line.Args.Count - 1;
                var extraComma = line.Args[lastArgIndex].ProceedingComma;
                if (extraComma is not null)
                {
                    _logger.Log(Severity.Error, LogId.UnexpectedToken, extraComma, "UnexpectedToken", extraComma.Source);
                }
            }

            // On the alignment pass, just reserve space for the instruction
            var size = _archHandler.GetInstructionSize(line);
            _activeSection.Reserve(size);
        }
        
        // Make allocations if directive is present
        // NOTE: Directive allocations are made in both passes
        // Issues are only logged on the second pass though
        if (line.Type is LineType.Directive)
            HandleDirective(line, 1);

        // If args is non-zero while the line type is none,
        // random garbage is in the file.
        if (line.Type is LineType.None && line.Args.Count is not 0)
        {
            _ = line.Args[0].Tokens[0].Type switch
            {
                TokenType.LabelDeclaration => _logger.Log(Severity.Error, LogId.UnexpectedToken, line.Args[0].Tokens[0], "MultipleLabels"),
                _ => _logger.Log(Severity.Error, LogId.UnexpectedToken, line.Args[0].Tokens[0], "UnexpectedToken", line.Args[0].Tokens[0]),
            };
        }

        // Log a debug line if this line changed the address
        if (line.Address != CurrentAddress && line.Count > 0)
        {
            _module.AddLineEntry(line.Address, line.Location);
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
                HandleDirective(line, 2);
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

        // Track relocatable references
        if (instruction.References is not null)
        {
            foreach (var reference in instruction.References)
                _activeSection.AddRelocation(reference);
        }

        // Append instruction to active segment
        _activeSection.Append(instruction.RealizeBytes());
    }

    private void HandleDirective(AssemblyLine line, int pass)
    {
        // Only log parser errors on the second pass
        var parser = new DirectiveParser(_module.Symbols, Config, pass is 2 ? _logger.Parent : null);

        var name = line.Directive;
        if (name is null || !parser.TryParseDirective(line, out var directive))
            return;

        Guard.IsNotNull(directive);
        ExecuteDirective(directive, line, pass);
    }

    private void ExecuteDirective(Directive directive, AssemblyLine line, int pass)
    {
        switch (directive)
        {
            case GlobalDirective global:
                var symbol = _module.GetOrCreateSymbol(global.Symbol);
                symbol.Binding = SymbolBinding.Global;
                break;
            case SectionDirective section:
                _activeSection = _module.GetOrCreateSection(section.Name);
                line.Address = CurrentAddress;                              // Override the line address 
                break;
            case AlignDirective align:
                _activeSection.Align((uint)(1 << (int)align.Boundary));    // TODO: Sort out typing here
                break;
            case DataDirective data:
                _activeSection.Append(data.Data);
                break;
            case DefineDirective define:
                if (pass is 2)
                {
                    // Only define constants on the second pass
                    DefineSymbol(define.Name, new Address(define.Value), SymbolType.Constant);
                }
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
