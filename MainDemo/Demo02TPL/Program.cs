// See https://aka.ms/new-console-template for more information
using Demo02TPL;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

Stopwatch sw;

Console.WriteLine("TPL - and how to debug it??");
Debugger.Break();

await Task.Factory.StartNew(() => Console.WriteLine("Hello from task!"));

await Task.Delay(1000);

Task<DayOfWeek> taskA = Task.Run(() => DateTime.Today.DayOfWeek);
// Execute the continuation when the antecedent finishes.
await taskA.ContinueWith(antecedent => Console.WriteLine($"Today is {antecedent.Result}."));

await Task.Delay(1000);

var tWait = Task.Run(() =>
{
    Console.WriteLine("Hello from task (wait)!");
    Thread.Sleep(2000);
    return 11;
});

while (!tWait.IsCompleted)
{
    ; //Or SpinWait or....
}

Console.WriteLine(tWait.Result);

Debugger.Break();
var lstT = new List<Task<int>>();

for (int ctr = 1; ctr <= 10; ctr++)
{
    int baseValue = ctr;
    lstT.Add(Task.Factory.StartNew<int>(b => (int)b! * (int)b, baseValue)); //try with ctr here ;)
}

var result = await Task.WhenAny(lstT.ToArray());
Console.WriteLine(result.Result);
//result.IsFaulted
//result.IsCompleted
//result.IsCompletedSuccessfully
//result.RunSynchronously
var results = await Task.WhenAll(lstT);
foreach (var item in results)
{
    Console.Write($"{item},");
}
Debugger.Break();
//Task /Async to sync, why this is not ideal
Console.WriteLine(Task.Run<int>(() => { Console.WriteLine("Hello from task (sync)!"); Thread.Sleep(500); return 11; }).Result);

HttpClient clt = new HttpClient();
var str = clt.GetStringAsync("https://www.microsoft.com").Result;
Console.WriteLine(str.Substring(0, 100));

/*
 1. Deadlock probability (in WPF / MAUI / Win32 UI or ASP.NET  context)
 2. Performance (obvious)
 3. AggregateException vs real
 4.  By the way - WHY if we have asyc/await????
 */
var tproxy = clt.GetStringAsync("https://www.microsoft.com");
await tproxy;
Console.WriteLine(tproxy.Result.Substring(0,100));

Console.WriteLine("Simple loops");
Debugger.Break();

Parallel.ForEach<int>(Enumerable.Range (1,10), 
    (i) => Console.WriteLine($"{Task.CurrentId}: {i}"));



Console.WriteLine("Enter - Nested thread + await");
Debugger.Break();
Task t = Task.Factory.StartNew(
    async () =>
    {
        Console.WriteLine($"MAIN (START): {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine("Hello from task!");
        HttpClient clt = new HttpClient();
        await foreach (var item in RangeAsync(1,10).ConfigureAwait(false))
        {
            await clt.GetStringAsync("https://www.microsoft.com");
            Console.WriteLine($"{item}, {Thread.CurrentThread.ManagedThreadId}");
        }

        Console.WriteLine($"MAIN (END): {Thread.CurrentThread.ManagedThreadId}");
    }
    );
await t.ConfigureAwait(false); //

Console.WriteLine("Enter - QuickSort");
Debugger.Break();

var data = new int[ 5 * 1024 * 1024];
for (int i = 0; i < data.Length; i++) data[i] = Random.Shared.Next(Int32.MinValue, Int32.MaxValue);

// Sort sequentially and in parallel
var seqData = (int[])data.Clone();
var parData = (int[])data.Clone();
sw = Stopwatch.StartNew();
QuickSort.QuicksortSequential(seqData);
Console.WriteLine($"QuickSort(sequential) {sw.ElapsedMilliseconds}");
sw = Stopwatch.StartNew();
QuickSort.QuicksortParallel(seqData);
Console.WriteLine($"QuickSort(parallel) {sw.ElapsedMilliseconds}");

Console.WriteLine("Enter - NQueen");
Debugger.Break();

for (int size = 8; size <= 13; size++)
{
    for (int rep = 0; rep < 2; rep++)
    {
        // Run sequential algorithm
        sw = Stopwatch.StartNew();
        int resultSeq = NQueensSolver.NQueensSequential(size);
        Console.WriteLine("   Board size: {0}   Sequential Algorithm Time: {1}ms   Solutions: {2}", size, sw.ElapsedMilliseconds, resultSeq);

        // Run parallel algorithm
        sw = Stopwatch.StartNew();
        int resultPar = NQueensSolver.NQueensParallel(size);
        Console.WriteLine("   Board size: {0}   Parallel Algorithm Time: {1}ms   Solutions: {2}", size, sw.ElapsedMilliseconds, resultPar);
    }
}

Console.WriteLine();

Console.WriteLine("Enter - ISBNs (and beginning PLINQ)");
Debugger.Break();

for (int size = 10000; size <= 1000000; size *= 10)
{
    string[] a = new string[size];
    for (int i = 0; i < a.Length; i++) a[i] = MakePossiblyBadISBN();

    for (int rep = 0; rep < 3; rep++)
    {
        // Run sequential algorithm
        sw = Stopwatch.StartNew();
        int resultSeq = CountValidISBNs.CountValidISBNsSequential(a);
        Console.WriteLine("   Input Size: {0}   Sequential Algorithm Time: {1}ms   Result:{2}", size, sw.ElapsedMilliseconds, resultSeq);

        // Run parallel algorithm
        sw = Stopwatch.StartNew();
        int resultPar = CountValidISBNs.CountValidISBNsParallel(a);
        Console.WriteLine("   Input Size: {0}   Parallel Algorithm Time: {1}ms   Result:{2}", size, sw.ElapsedMilliseconds, resultPar);
    }
}

Console.WriteLine();

Console.WriteLine("Enter - Task and debug");
Debugger.Break();
TaskAndDebug.RUNme();

static async IAsyncEnumerable<int> RangeAsync(
  int start, int count,
  [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    for (int i = 0; i < count; i++)
    {
        await Task.Delay(i, cancellationToken);
        yield return start + i;
    }
}

static string MakePossiblyBadISBN()
{
    char[] s = new char[10];
    for (int i = 0; i < 9; i++)
    {
        s[i] = (char)('0' + Random.Shared.Next(10));
    }

    // Add a random check digit
    int check = Random.Shared.Next(11);
    s[9] = (char)((check == 10) ? 'X' : ('0' + check));
    return new string(s);
}
