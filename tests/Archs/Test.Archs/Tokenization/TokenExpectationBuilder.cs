// Avishai Dernis 2026

using Zarem.Assembler.Tokenization.Models.Enums;
using Zarem.Assembler.Tokenization.Profiles;

namespace Test.Archs.Tokenization;

public class TokenExpectationBuilder
{
    private List<(string, TokenType)> _tokens = [];
    private ITokenizerProfile _profile;

    public TokenExpectationBuilder(ITokenizerProfile profile)
    {
        _profile = profile;
    }

    public (string, TokenType)[] Build() => [.._tokens];

    public TokenExpectationBuilder Append(string text, TokenType type)
    {
        _tokens.Add((text, type));
        return this;
    }

    public TokenExpectationBuilder Instruction(string instruction)
        => Append(instruction, TokenType.Instruction);

    public TokenExpectationBuilder Reg(string reg, bool prefix = false)
    {
        if (prefix && _profile.RegisterPrefix is not '\0')
        {
            Append($"{_profile.RegisterPrefix}", TokenType.RegisterPrefix);
        }

        return Append(reg, TokenType.Register);
    }

    public TokenExpectationBuilder Imm(string imm, bool prefix = false)
    {
        if (prefix && _profile.ImmediatePrefix is not '\0')
        {
            Append($"{_profile.ImmediatePrefix}", TokenType.ImmediatePrefix);
        }

        return Append(imm, TokenType.Immediate);
    }

    public TokenExpectationBuilder Reloc(string reloc, bool prefix = false)
    {
        if (prefix && _profile.RelocationPrefix is not '\0')
        {
            Append($"{_profile.RelocationPrefix}", TokenType.RelocationPrefix);
        }

        return Append(reloc, TokenType.Relocation);
    }

    public TokenExpectationBuilder Comma()
        => Append(",", TokenType.Comma);

    public TokenExpectationBuilder Open()
        => Append("(", TokenType.OpenParenthesis);

    public TokenExpectationBuilder Close()
        => Append(")", TokenType.CloseParenthesis);
}
