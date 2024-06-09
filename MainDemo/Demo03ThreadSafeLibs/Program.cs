// See https://aka.ms/new-console-template for more information
using System.Collections.Concurrent;
using System.Diagnostics;

Console.WriteLine("Collection");

Console.WriteLine("BlockingCollection = Consumer / producer");

using (var bc = new BlockingCollection<long>())
{
    // Spin up a Task to populate the BlockingCollection
    Task t1 = Task.Run(() =>
    {
        for (int i = 0; i < 10; i++)
        {
            bc.Add(1);
            Task.Delay(50 + Random.Shared.Next(10)).Wait();
            bc.Add(2);
            Task.Delay(50 + Random.Shared.Next(10)).Wait();
            bc.Add(3);
            Task.Delay(50 + Random.Shared.Next(10)).Wait();
        }
        bc.CompleteAdding(); //Always SLOWER :)
    });
    Task t1a = Task.Run(() =>
    {
        Parallel.For(0, 10, i =>
            {
                bc.Add(100 + i + 1 + Thread.CurrentThread.ManagedThreadId * 100000);
                Task.Delay(10 + Random.Shared.Next(10)).Wait();
                bc.Add(100 + i + 2 + Thread.CurrentThread.ManagedThreadId * 100000);
                Task.Delay(10 + Random.Shared.Next(10)).Wait();
                bc.Add(100 + i + 3 + Thread.CurrentThread.ManagedThreadId * 100000);
                Task.Delay(10 + Random.Shared.Next(10)).Wait();
            }
            );
        //bc.CompleteAdding();
    });

    // Spin up a Task to consume the BlockingCollection
    Task t2 = Task.Run(() =>
    {
        try
        {
            // Consume the BlockingCollection
            while (true) Console.WriteLine($"{Task.CurrentId}: { bc.Take()}");
        }
        catch (InvalidOperationException)
        {
            // An InvalidOperationException means that Take() was called on a completed collection
            //After CompleteAdding
            Console.WriteLine("That's All!");
        }
    });

    await Task.WhenAll(t1, t1a, t2);
}

Console.WriteLine("ConcurrentBag - unordered collection of objects, with duplicates.");
Debugger.Break();

ConcurrentBag<long> cb = new();
CancellationTokenSource cts = new CancellationTokenSource();
CancellationToken ct=cts.Token;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
Task.Run(() => {
    while(!ct.IsCancellationRequested)
        while (cb.TryTake(out long item))
        {
            Console.WriteLine($"{Task.CurrentId}: {item}");
        }
    Console.WriteLine("t1-end");
}, ct);

Task.Run(() => {
    while (!ct.IsCancellationRequested)
        while (cb.TryTake(out long item))
        {
            Console.WriteLine($"{Task.CurrentId}: {item}");
        }
    Console.WriteLine("t2-end");
}, ct);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

Parallel.For(0, 10, i =>
        {
            for (int j = 0; j < 100; j++)
            {
                cb.Add(100 + j + i + Thread.CurrentThread.ManagedThreadId * 100000);
                Task.Delay(10 + Random.Shared.Next(10)).Wait();
            }
        }
    );

Debugger.Break();
Thread.Sleep(1000); //Parallel - finished here, but let's wait for "normal" tasks
cts.Cancel();

Debugger.Break();

//Also:
ConcurrentDictionary<int, string> cd = new();
ConcurrentStack<int> cs = new();
ConcurrentQueue<int> cq = new();
