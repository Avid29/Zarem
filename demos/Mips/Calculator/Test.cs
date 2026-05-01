using System;
using System.Globalization;

while (true)
{
    // Get First Number
    Console.Write("Enter a number: ");
    float num1 = float.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

    // Get Operator
    Console.Write("Enter Operator (+, -, *, / or 'q' to quit): ");
    string opInput = Console.ReadLine();
    char op = opInput[0];

    // Check for quit condition
    if (op == 'q')
    {
        Console.Write("Exiting calculator. Goodbye!\n");
        return;
    }

    // Get Second Number
    Console.Write("Enter a number: ");
    float num2 = float.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

    // Perform Calculation
    float result = op switch
    {
        '+' => num1 + num2,
        '-' => num1 - num2,
        '*' => num1 * num2,
        '/' => num1 / num2,
        _ => 0f
    };

    // Print Result
    Console.Write("Result: ");
    Console.Write(result.ToString(CultureInfo.InvariantCulture));

    Console.Write("\n\n");
}
