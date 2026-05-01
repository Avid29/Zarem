using System;

// This is a kinda strange implementation, but it matches the
// MIPS code most closely
for (int i = 1; i <= 100; i++)
{
    bool fizz = i % 3 == 0;
    if (fizz)
    {
        Console.Write("Fizz");
    }

    bool buzz = i % 5 == 0;
    if (buzz)
    {
        Console.Write("Buzz");
    }
    else if (!fizz)
    {
        Console.Write(i);
    }

    Console.Write('\n');
}
