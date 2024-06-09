// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

Console.WriteLine("Reactive Extensions");
//IObservable<int>
var source = Observable.Range(0, 10);

source.Subscribe(
    x => Console.WriteLine("sub1: OnNext:  {0}", x),
    ex => Console.WriteLine("OnError: {0}", ex.Message),
    () => Console.WriteLine("source - OnCompleted")
);

source.Subscribe(
    x => Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} - {x}")
);

IObservable<int> source2 =  source.Where(x => x % 2 == 0);
source2.Subscribe(
    x => Console.WriteLine("sub2: OnNext:  {0}", x),
    () => Console.WriteLine("source2 - OnCompleted")
);

await Task.Delay(1000);
Debugger.Break();

var source1 = new Subject<int>();

source1.Subscribe(
    x => Console.WriteLine($"{x}")
);
for (int i = 0; i < 10; i++)
{
    source1.OnNext(i);
}
Debugger.Break();

Console.WriteLine("(scheduler)");
var subject = new Subject<int>();

// Create a TaskPoolScheduler.
var taskFactory = new TaskFactory(TaskScheduler.Current);
var scheduler = new TaskPoolScheduler(taskFactory);

// Subscribe to the subject using the TaskPoolScheduler.
subject
    .ObserveOn(scheduler)
    .Subscribe(value =>
    {
        Console.WriteLine($"{value} on {Thread.CurrentThread.ManagedThreadId}, {Task.CurrentId}");
    });
subject
    .ObserveOn(scheduler)
    .Subscribe(value =>
    {
        Console.WriteLine($"{value} on {Thread.CurrentThread.ManagedThreadId}, {Task.CurrentId}");
    });
subject
    .ObserveOn(scheduler)
    .Subscribe(value =>
    {
        Console.WriteLine($"{value} on {Thread.CurrentThread.ManagedThreadId}, {Task.CurrentId}");
    });

// Manually send elements to the subject.
for (int i = 0; i < 10; i++)
{
    subject.OnNext(i);
}

await Task.Delay(1000);
// Complete the subject.
subject.OnCompleted();

Debugger.Break();

Console.WriteLine("Throttle"); // Different than in RxJs -
                               // Rx.NET’s Throttle is more like a
                               // self-resetting circuit breaker that
                               // shuts off completely during an overload
var subject2 = new Subject<int>();

var sub3 = subject2
    .Throttle(TimeSpan.FromMilliseconds(300))
    //.ObserveOn(scheduler)
    .Subscribe(value =>
    {
        Console.WriteLine($"{value} on {DateTime.Now.Second}");
    });

for (int i = 0; i < 100; i++)
{
    subject2.OnNext(i);
    await Task.Delay(10);
}
subject2.OnNext(1000);
await Task.Delay(310);
subject2.OnNext(2000);
await Task.Delay(310);

//await Task.Delay(10000);

Debugger.Break();
