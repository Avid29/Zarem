using System;

int[] board = new int[36];

Console.Write("Enter 6x6 board string (36 chars, 0 for empty):\n");
string input = Console.ReadLine() ?? "";

if (input.Length < 36)
{
    Console.Write("Input too short!\n");
    return;
}

// Load the board
for (int i = 0; i < 36; i++) board[i] = input[i] - '0';

// Solve and Print
if (Solve(0))
{
    Console.Write("\nSolved:\n");
    for (int i = 0; i < 36; i++)
    {
        Console.Write(board[i] + " ");
        if ((i + 1) % 6 == 0)
        {
            Console.Write("\n");
        }
    }
}
else
{
    Console.Write("No solution\n");
}

bool Solve(int index)
{
    if (index == 36)
        return true;

    if (board[index] != 0)
    {
        return Solve(index + 1);
    }

    for (int num = 1; num <= 6; num++)
    {
        if (IsValid(index, num))
        {
            board[index] = num;
            if (Solve(index + 1))
                return true;

            board[index] = 0; // Backtrack
        }
    }
    return false;
}

bool IsValid(int index, int num)
{
    int row = index / 6;
    int col = index % 6;

    // Row and Col check
    for (int i = 0; i < 6; i++)
    {
        if (board[row * 6 + i] == num)
            return false; // Row

        if (board[i * 6 + col] == num)
            return false; // Col
    }

    // 2x3 Box check
    int startRow = (row / 2) * 2;
    int startCol = (col / 3) * 3;

    for (int r = 0; r < 2; r++)
    {
        for (int c = 0; c < 3; c++)
        {
            int checkIndex = (startRow + r) * 6 + (startCol + c);
            if (board[checkIndex] == num)
                return false;
        }
    }

    return true;
}
