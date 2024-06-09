// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Runtime.InteropServices;

[DllImport("kernel32.dll")]
static extern uint GetCurrentThreadId();


Console.WriteLine($"Threads, main thread: {Thread.CurrentThread.ManagedThreadId}; physical: {GetCurrentThreadId()}");

Thread t01 = new Thread(() => Console.WriteLine($"Hello {Thread.CurrentThread.ManagedThreadId}; physical: {GetCurrentThreadId()}"));
t01.Start();
Thread.Sleep(1000);

Debugger.Break();
Console.WriteLine("Simple threads");

Thread t02 = new(() => {
    Console.WriteLine("t02 - start");
    Thread.SpinWait(100000000);
    Console.WriteLine("t02 - end");
    }
);
t02.Start();

CancellationTokenSource cts = new();

Thread t03 = new((object? token) => {
    if (token is null) return;
    CancellationToken ct = (CancellationToken)token;
    Console.WriteLine("t03 - start");

    while (!ct.IsCancellationRequested)
    {
        Console.WriteLine("t03 - running");
        Thread.Sleep(1000);
    }
    Console.WriteLine("t03 - end");
}
);
t03.Start(cts.Token);

Thread t04 = new((object? token) => {
    if (token is null) return;
    CancellationToken ct = (CancellationToken)token;
    Console.WriteLine("t04 - start");
    while (!ct.IsCancellationRequested)
    {
        Console.WriteLine("t04 - running");
        Thread.SpinWait(1000000);
    }
    Console.WriteLine("t04 - end");
}
);
t04.Start(cts.Token);

Thread.SpinWait(100000000/3);
cts.Cancel();
// t03.Abort();
// t03.Cancel();


cts.Dispose();///

Debugger.Break();
Console.WriteLine("Passing parameters");

List<Thread> arrT = new();
for (int i = 0; i < 10; i++)
{
    Thread t = new Thread(()=>myThreadMethod(i)); //What really happening here?
    t.Start();
    arrT.Add(t);
}

Console.WriteLine("Passing parameters - better");

for (int i = 0; i < 10; i++)
{
    Thread t = new Thread(new ParameterizedThreadStart(myThreadMethod));
    t.Start(i);
    arrT.Add(t);
}

/*
Passing parameters
myThreadMethod 4, ManagedThreadId:20, GetCurrentThreadId:44296
myThreadMethod 4, ManagedThreadId:19, GetCurrentThreadId:62724
myThreadMethod 4, ManagedThreadId:3, GetCurrentThreadId:40600
myThreadMethod 4, ManagedThreadId:18, GetCurrentThreadId:65564
myThreadMethod 5, ManagedThreadId:21, GetCurrentThreadId:19512
myThreadMethod 6, ManagedThreadId:22, GetCurrentThreadId:9172
myThreadMethod 7, ManagedThreadId:23, GetCurrentThreadId:42880
myThreadMethod 9, ManagedThreadId:24, GetCurrentThreadId:50124
myThreadMethod 9, ManagedThreadId:25, GetCurrentThreadId:58596
myThreadMethod 10, ManagedThreadId:26, GetCurrentThreadId:19048
myThreadMethod 0, ManagedThreadId:27, GetCurrentThreadId:34436
myThreadMethod 1, ManagedThreadId:28, GetCurrentThreadId:38256
myThreadMethod 2, ManagedThreadId:29, GetCurrentThreadId:65476
myThreadMethod 3, ManagedThreadId:30, GetCurrentThreadId:46996
myThreadMethod 4, ManagedThreadId:31, GetCurrentThreadId:42304
myThreadMethod 5, ManagedThreadId:32, GetCurrentThreadId:39476
myThreadMethod 6, ManagedThreadId:33, GetCurrentThreadId:32224
myThreadMethod 7, ManagedThreadId:34, GetCurrentThreadId:2732
Process
myThreadMethod 8, ManagedThreadId:35, GetCurrentThreadId:7556
myThreadMethod 9, ManagedThreadId:36, GetCurrentThreadId:56900
 */

Console.WriteLine("Process");
Debugger.Break();
// Create a new process start info
var startInfo = new ProcessStartInfo("notepad.exe")
{
    // Configure the start info as needed
    UseShellExecute = false,
    RedirectStandardOutput = true,
};

// Start the process
var process = Process.Start(startInfo);
if (process != null) { 
    Debugger.Break();
    // Optionally, wait for the process to exit
    process.Kill(true);
    process.WaitForExit();
    //process.Threads
    Debugger.Break();
}


Console.WriteLine("Too many threads (kill me fast!)");
Debugger.Break();

List<Thread> arrTLarge = new();
for (int i = 0; i < 10000; i++)
{
    Thread t = new Thread(() => {
        //Thread.Sleep(Timeout.Infinite);
        while (true)
        {
            Thread.SpinWait(1000000);
        }
    });
    t.Name = $"Thread{i}";
    t.Start();
    arrTLarge.Add(t);
}

Console.WriteLine("Enter4");
Console.ReadLine();


void myThreadMethod(object? param)
{
#pragma warning disable CS8605 // Unboxing a possibly null value.
    int i = (int)param;
#pragma warning restore CS8605 // Unboxing a possibly null value.
    Console.WriteLine($"myThreadMethod {i}, ManagedThreadId:{Thread.CurrentThread.ManagedThreadId}, GetCurrentThreadId:{GetCurrentThreadId()}");
}