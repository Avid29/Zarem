// Avishai Dernis 2024

using System.CommandLine;
using System.Globalization;
using Zarem.Console.Commands;

namespace Zarem.Console;

internal class Program
{
    private static void Main(string[] args)
    {
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        var rootCommand = new RootCommand
        {
            new Assemble(),
            new Build(),
            new Create(),
        };

        rootCommand.Parse(args).Invoke();
    }
}
