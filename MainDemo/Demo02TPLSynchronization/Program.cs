// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Threading;

Console.WriteLine("Hello, World!");

// lock
object lockObject = new();

lock (lockObject)
{
    Console.WriteLine("Hello from lock!");
}

int protectedVariable = 0;

Console.WriteLine("No Lock");
var tasks = new List<Task>();
foreach (var item in Enumerable.Range(1, 10))
{
    tasks.Add(
    Task.Run(() =>
    {
        for (int i = 0; i < 10; i++)
        {
            protectedVariable++;
            if (protectedVariable >= 1) Console.WriteLine($"{Task.CurrentId}: {protectedVariable}");
            protectedVariable--;
        }
    }));
}

Task.WaitAll(tasks.ToArray());
Console.WriteLine($"Final value: {protectedVariable}");

Debugger.Break();

Console.WriteLine("Lock");
tasks = new List<Task>();
foreach (var item in Enumerable.Range(1, 10))
{
    tasks.Add(
    Task.Run(() =>
    {
        for (int i = 0; i < 10; i++)
        {
            lock (lockObject)
            {
                protectedVariable++;
                if (protectedVariable >= 1) Console.WriteLine($"{Task.CurrentId}: {protectedVariable}");
                protectedVariable--;
            }
        }
    }));
}

Task.WaitAll(tasks.ToArray());
Console.WriteLine($"Final value: {protectedVariable}");


Debugger.Break();

//And Mutex? - like lock, but Win32 object, and SLOW
Mutex mut = new Mutex();
for (int i = 0; i < 5; i++)
{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    Task.Run(() =>
    {
        for (int i = 0; i < 10; i++)
        {
            mut.WaitOne();
            protectedVariable++;
            if (protectedVariable >= 1) Console.WriteLine($"{Task.CurrentId}: {protectedVariable}");
            protectedVariable--;
            mut.ReleaseMutex();
        }
    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
}
Thread.Sleep(1000); //Enough
Console.WriteLine($"Final value: {protectedVariable}");
Debugger.Break();

//Interlocked - atomic operation on Variables; here - how to acces unsafe resources
//Why not always lock - Interlocked faster than lock because it doesn't involve context switching or kernel mode transitions. And is no blocking!
//(and in many scenarios we are simply doing atomic operations)

Console.WriteLine("Interlocked");
int resourceprotection = 0;
tasks = new List<Task>();
foreach (var item in Enumerable.Range(1, 10))
{
    tasks.Add(
    Task.Run(() =>
    {
        for (int i = 0; i < 10; i++)
        {
            if (Interlocked.Exchange(ref resourceprotection, 1) == 0)
            {
                protectedVariable++;
                if (protectedVariable >= 1) Console.WriteLine($"{Task.CurrentId}: {protectedVariable}");
                protectedVariable--;
                Interlocked.Exchange(ref resourceprotection, 0);
            }
            else { Console.WriteLine($"{Task.CurrentId}: Resource busy"); }
        }
    }));
}

Task.WaitAll(tasks.ToArray());
Debugger.Break();

Console.WriteLine("Barrier");
Barrier barrier = new Barrier(5
    , (b) =>
        { 
            Console.WriteLine($"Post-phase action, {b.CurrentPhaseNumber}"); 
        }

    );
tasks = new List<Task>();
for (int i = 0; i < 5; i++)
{
    tasks.Add(
    Task.Run(() =>
    {
        Console.WriteLine($"{Task.CurrentId}: Before barrier");
        Thread.Sleep(500+Random.Shared.Next(1000));
        barrier.SignalAndWait();
        Console.WriteLine($"{Task.CurrentId}: After barrier 1");
        Thread.Sleep(500 + Random.Shared.Next(1000));
        barrier.SignalAndWait();
        Console.WriteLine($"{Task.CurrentId}: After barrier 2");
        Thread.Sleep(500 + Random.Shared.Next(1000));
        barrier.SignalAndWait();
        Console.WriteLine($"{Task.CurrentId}: After barrier 3");
    }));
}
Task.WaitAll(tasks.ToArray());

Debugger.Break();

//Important - In TASK, we can achive similar effect with Task, waiting, combination with await - and it is usually easier to debug
//Also: SemaphoreSlim(0, 3); (0-3 can enter semaphore)
//Also: LockCookie | ReaderWriterLock single writers and multiple readers
//Also: ManualResetEvent(Slim), AutoResetEvent 
