using System;
using System.Text;

Console.Write("What is your name? (36 chars)\n");
var input = Console.ReadLine();

var encoding = Encoding.BigEndianUnicode;
byte[] inputBytes = encoding.GetBytes(input);
int byteCount = Math.Min(36 * 2, inputBytes.Length);
string result = encoding.GetString(inputBytes, 0, byteCount);

Console.Write($"Hello {result}\n");
