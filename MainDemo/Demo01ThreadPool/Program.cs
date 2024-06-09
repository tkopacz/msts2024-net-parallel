// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

Console.WriteLine("Hello, World!");
int maxworkerThreads, maxcompletionPortThreads;
int minworkerThreads, mincompletionPortThreads;
int avlworkerThreads, avlcompletionPortThreads;

ThreadPool.GetMaxThreads(out maxworkerThreads, out maxcompletionPortThreads);
ThreadPool.GetMinThreads(out minworkerThreads, out mincompletionPortThreads);
ThreadPool.GetAvailableThreads(out avlworkerThreads, out avlcompletionPortThreads);
Console.WriteLine($"Min : {minworkerThreads,000000}, {mincompletionPortThreads,000000}");
Console.WriteLine($"Max : {maxworkerThreads,000000}, {maxcompletionPortThreads,000000}");
Console.WriteLine($"Aval: {maxworkerThreads,000000}, {maxcompletionPortThreads,000000}");

Console.WriteLine($"Current: {ThreadPool.ThreadCount},  {ThreadPool.PendingWorkItemCount}, {ThreadPool.CompletedWorkItemCount}");

Console.WriteLine("Enter - QueueUserWorkItem");
Debugger.Break();


ThreadPool.QueueUserWorkItem((state) =>
{
    Console.WriteLine($"Simple Start: {Thread.CurrentThread.ManagedThreadId}");
    Thread.Sleep(1000);
    Console.WriteLine($"Simple End: {Thread.CurrentThread.ManagedThreadId}");
});

ThreadPool.QueueUserWorkItem((state) =>
{
    Console.WriteLine($"Outer Start: {Thread.CurrentThread.ManagedThreadId}");
    Thread.Sleep(1000);
    ThreadPool.QueueUserWorkItem<int>((state) =>
    {
        Console.WriteLine($"Inner Start: {Thread.CurrentThread.ManagedThreadId}, State: {state}");
        Thread.Sleep(2000);
        Console.WriteLine($"Inner End: {Thread.CurrentThread.ManagedThreadId}");
    }, 99, preferLocal: true); //current thread pool thread’s local queue (if available).
    Thread.Sleep(1000);
    Console.WriteLine($"Outer End: {Thread.CurrentThread.ManagedThreadId}");
});



Console.WriteLine("Enter - QueueUserWorkItem,1000");
Debugger.Break();

Console.WriteLine("Start 1000");
for (int i = 0; i < 1000; i++)
{
    ThreadPool.QueueUserWorkItem((state) =>
    {
        Thread.Sleep(1000);
    });
}
Console.WriteLine($"Current: {ThreadPool.ThreadCount},  {ThreadPool.PendingWorkItemCount}, {ThreadPool.CompletedWorkItemCount}");
Thread.Sleep(100);
Console.WriteLine($"100,Current: {ThreadPool.ThreadCount},  {ThreadPool.PendingWorkItemCount}, {ThreadPool.CompletedWorkItemCount}");
Thread.Sleep(1000);
Console.WriteLine($"1100, Current: {ThreadPool.ThreadCount},  {ThreadPool.PendingWorkItemCount}, {ThreadPool.CompletedWorkItemCount}");

Console.WriteLine("Enter");
Debugger.Break();
