// See https://aka.ms/new-console-template for more information
using Demo02TPL;
using System.Diagnostics;

Console.WriteLine("Exceptions - run without debugger!!!!!");
Console.WriteLine("Enter - Exceptions");
Console.ReadLine();

Console.WriteLine("-------------TPL");

ConcurrencyAndException.RunMe();

Console.WriteLine("-------------Async");

await ConcurrencyAndException.RunMeAsync();

Console.WriteLine("-------------Async1");

await ConcurrencyAndException.RunMeAsync();

Console.ReadLine();
