// Avishai Dernis 2026

namespace Test.Archs.Emulator;

public abstract record EmulatorTestCase<TConfig>
{
    public EmulatorTestCase(TConfig config, string input)
    {
        Config = config;
        Input = input;
    }

    public TConfig Config { get; }

    public string Input { get; }
}
