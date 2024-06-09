// See https://aka.ms/new-console-template for more information
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Xml.Linq;

Console.WriteLine("(P)Linq");
Stopwatch sw;

Debugger.Break();
var source = Enumerable.Range(1, 1000000000);
long cnt = source.Count();
var ct = new CancellationTokenSource(1000).Token;
try
{
    var testToCancel = from num in source.AsParallel().WithCancellation(ct)
                       where num % 2 == 0 || num % 5 == 0 && num < source.AsParallel().WithCancellation(ct).Max() / 2
                       select num;
    Console.WriteLine($"{testToCancel.Average()}");
} catch (OperationCanceledException) {
    Console.WriteLine("OperationCanceledException");
}
Debugger.Break();

Console.WriteLine("----------------- num % 2 == 0");

source = Enumerable.Range(1, 1000000000);
cnt = source.Count(); //.AsParallel().Count() - khem.... ...
sw = Stopwatch.StartNew();
var evenNums = from num in source
               where num % 2 == 0
               select num;
//Console.WriteLine(sw.ElapsedMilliseconds);//NOT HERE!!!!
Console.WriteLine("{0} even numbers out of {1} total",
                  evenNums.Count(), cnt);
Console.WriteLine(sw.ElapsedMilliseconds);

// Opt in to PLINQ with AsParallel.
sw = Stopwatch.StartNew();
var evenNumsP = from num in source.AsParallel()
               where num % 2 == 0
               select num;
Console.WriteLine("{0} even numbers out of {1} total",
                  evenNumsP.Count(), cnt);
Console.WriteLine($".AsParallel(): {sw.ElapsedMilliseconds}");

// Opt in to PLINQ with AsParallel.
sw = Stopwatch.StartNew();
var evenNumsP1 = from num in source.AsParallel().WithDegreeOfParallelism(1)
               where num % 2 == 0
               select num;
Console.WriteLine("{0} even numbers out of {1} total",
                  evenNumsP1.Count(), cnt);
Console.WriteLine($".AsParallel().WithDegreeOfParallelism(1): {sw.ElapsedMilliseconds}");



Debugger.Break();

source = Enumerable.Range(1, 10000);
cnt = source.Count();

Console.WriteLine("----------------- um % 2 == 0 || num % 5 == 0 && num < source.AsParallel().Max() / 2");

sw = Stopwatch.StartNew();
var test1 = from num in source.AsParallel()
            where num % 2 == 0 || num % 5 == 0 && num < source.AsParallel().Max() / 2
            select num;
Console.WriteLine("{0} out of {1} total",
                  test1.Count(), cnt);
Console.WriteLine($".AsParallel(): {sw.ElapsedMilliseconds}");

sw = Stopwatch.StartNew();
var test2 = from num in source
            where num % 2 == 0 || num % 5 == 0 && num < source.Max() / 2
            select num;
Console.WriteLine("{0} out of {1} total",
                  test2.Count(), cnt);
Console.WriteLine($"{sw.ElapsedMilliseconds}");

/* BUT:
550000 out of 1000000 total
21847
550000 out of 1000000 total
81026 
 */
Debugger.Break();

//Join :)
Console.WriteLine("----------------- um % 2 == 0 || num % 5 == 0 && num < source.AsParallel().Max() / 2");
var source1 = Enumerable.Range(1, 10000);
var source2 = Enumerable.Range(1, 10000);

sw = Stopwatch.StartNew();
var test3 = source1.AsParallel().Join(source2.AsParallel(),n1=>n1,n2=>n2, (n1,n2) => new { S1=n1,S2=n2} );
Console.WriteLine($"{test3.Count()}");
Console.WriteLine($"Time: {sw.ElapsedMilliseconds}");
foreach(var item in test3.Take(10))
{
    Console.WriteLine($"{item.S1} - {item.S2}");
}
Debugger.Break();
//Wait.... ORDER
sw = Stopwatch.StartNew();
var test4 = source1.AsParallel().Join(source2.AsParallel(), n1 => n1, n2 => n2, (n1, n2) => new { S1 = n1, S2 = n2 }).OrderBy(p=>p.S1).OrderBy(p=>p.S2);    
Console.WriteLine($"{test3.Count()}");
Console.WriteLine($"Time: {sw.ElapsedMilliseconds}");
foreach (var item in test4.Take(10))
{
    Console.WriteLine($"{item.S1} - {item.S2}");
}
Debugger.Break();

//Diggression: Many2Many
List<int> list1 = new List<int> { 3, 1, 2 }; List<int> list2 = new List<int> { 6, 4, 5 };
var crossJoin = list1
    .AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism) //Just to avoid too smart PLINQ
    .SelectMany(item1 => list2, (item1, item2) => new { Item1 = item1, Item2 = item2 })
    //.OrderBy(item => item.Item1)
    //.ThenBy(item => item.Item2)
    ;
foreach (var item in crossJoin) Console.WriteLine($"Item1: {item.Item1}, Item2: {item.Item2}");

Debugger.Break();

Console.WriteLine("order - details");
source = Enumerable.Range(1, 1000);
int i = 0;
var data = from num in source.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
           select new DemoStructure { Pos = i++, Num = num, Square = num * num, Text = new string('A', num % 10) }
           ;
Console.WriteLine("foreach (var item in data)");
foreach (var item in data)
{
    Console.WriteLine($"{item.Pos}, {item.Num}");
}
Debugger.Break();

var dataseq = from num in source
           select new DemoStructure { Pos = i++, Num = num, Square = num * num, Text = new string('A', num % 10) }
           ;
Console.WriteLine("(SEQ) foreach (var item in data)");
foreach (var item in dataseq)
{
    Console.WriteLine($"{item.Pos}, {item.Num}");
}
Debugger.Break();


//Wait - why started with 892 (or similar). Because no lock on i ;)
var duplicates = data // .AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
    .GroupBy(p => p.Pos).Where(group => group.Count()>1)
    //.SelectMany(group => group) //flatten
    //.Select(p=>p.Pos) //select only Pos
    ;
Console.WriteLine($"Duplicates:{duplicates.Count()}");

//var example = duplicates.FirstOrDefault();
Debugger.Break();


Console.WriteLine("foreach (var item in data.OrderBy(p => p.Pos))");
foreach (var item in data.OrderBy(p => p.Pos))
{
    Console.WriteLine($"{item.Pos}, {item.Num}");
}
Debugger.Break();

Console.WriteLine("data.AsParallel().Where(p => p.MyMethod1() > 10)");
var test5 = data.AsParallel().Where(p => p.MyMethod1() > 10);
Console.WriteLine(test5.Count()); //998
Console.WriteLine("data.Where(p => p.MyMethod1() > 10)");
test5 = data.Where(p => p.MyMethod1() > 10); //why different threads???? Explain FAST how LINQ Tree is working!
Console.WriteLine(test5.Count()); //998
Debugger.Break();

var dArray = data.ToArray();
var test6 = dArray.Where(p => p.MyMethod1() > 10); //Finally SYNC
Console.WriteLine(test6.Count()); //998

Debugger.Break();

Console.WriteLine("Parallel.ForEach(data, p => p.MyMethod1())");
Parallel.ForEach(data, p => p.MyMethod1());
Debugger.Break();

//Well, if you REALLY KNOW what you are doing...

Console.WriteLine("LoadBalancingPartitioner<DemoStructure>(dataseq.ToArray())");
var partitioner = new LoadBalancingPartitioner<DemoStructure>(dataseq.ToArray());
var query = partitioner.AsParallel().Select(item => item.MyMethod1());
Console.WriteLine(query.Count());
//And - compare "Real" partitioner: https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Collections.Concurrent/src/System/Collections/Concurrent/PartitionerStatic.cs 
//And read comments, changes etc....
//And that one: https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Collections.Concurrent/src/System/Collections/Concurrent/OrderablePartitioner.cs
Debugger.Break();

public class DemoStructure
{
    public int Num { get; set; } = 0;
    public int Square { get; set; } = 0;
    public string Text { get; set; } = String.Empty;

    public int Pos { get; set; }
    public int MyMethod1()
    {
        Console.WriteLine($"ManagedThreadId: {Thread.CurrentThread.ManagedThreadId}, Task.CurrentId: {Task.CurrentId}, Pos:{Pos}, Num:{Num}");
        return Num+ Square;
    }
}


public class LoadBalancingPartitioner<T> : Partitioner<T>
{
    private readonly T[] _data;

    public LoadBalancingPartitioner(T[] data)
    {
        _data = data;
    }

    public override bool SupportsDynamicPartitions => false;

    public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
    {
        var result = new List<IEnumerator<T>>(partitionCount);

        int batchSize = (int)Math.Ceiling((double)_data.Length / partitionCount);
        int remaining = _data.Length;

        for (int i = 0; i < partitionCount; i++)
        {
            int currentBatchSize = Math.Min(batchSize, remaining);
            int start = i * batchSize;
            int end = start + currentBatchSize;

            result.Add(GetItems(start, end).GetEnumerator());

            remaining -= currentBatchSize;
        }

        return result;
    }

    private IEnumerable<T> GetItems(int start, int end)
    {
        for (int i = start; i < end; i++)
        {
            yield return _data[i];
        }
    }
}