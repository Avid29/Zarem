// Avishai Dernis 2026

using System.Text.RegularExpressions;
using Zarem.Assembler.Tokenization.Profiles;

namespace Test.Zarem;

internal class DummyProfile : ITokenizerProfile
{
    public char CommentPrefix => '\0';

    public char ImmediatePrefix => '\0';

    public char RegisterPrefix => '\0';

    public Regex RegisterRegex => throw new System.NotImplementedException();
}
