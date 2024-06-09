// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

Console.WriteLine("Data Flow - example");

// Downloads the requested resource as a string.
var downloadString = new TransformBlock<string, string>(async uri =>
{
    Console.WriteLine("Downloading '{0}'...", uri);

    return await new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }).GetStringAsync(uri);
});

// Separates the specified text into an array of words.
var createWordList = new TransformBlock<string, string[]>(text =>
{
    Console.WriteLine("Creating word list...");

    // Remove common punctuation by replacing all non-letter characters
    // with a space character.
    char[] tokens = text.Select(c => char.IsLetter(c) ? c : ' ').ToArray();
    text = new string(tokens);

    // Separate the text into an array of words.
    return text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
});

// Removes short words and duplicates.
var filterWordList = new TransformBlock<string[], string[]>(words =>
{
    Console.WriteLine("Filtering word list...");

    return words
       .Where(word => word.Length > 3)
       .Distinct()
       .ToArray();
});

// Finds all words in the specified collection whose reverse also
// exists in the collection.
var findReversedWords = new TransformManyBlock<string[], string>(words =>
{
    Console.WriteLine("Finding reversed words...");

    var wordsSet = new HashSet<string>(words);

    return from word in words.AsParallel()
           let reverse = new string(word.Reverse().ToArray())
           where word != reverse && wordsSet.Contains(reverse)
           select word;
});

// Prints the provided reversed words to the console.
var printReversedWords = new ActionBlock<string>(reversedWord =>
{
    Console.WriteLine("Found reversed words {0}/{1}",
       reversedWord, new string(reversedWord.Reverse().ToArray()));
});

//
// Connect the dataflow blocks to form a pipeline.
//

var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

downloadString.LinkTo(createWordList, linkOptions);
    createWordList.LinkTo(filterWordList, linkOptions);
        filterWordList.LinkTo(findReversedWords, linkOptions);
            findReversedWords.LinkTo(printReversedWords, linkOptions);

//Main App:
// Process "The Iliad of Homer" by Homer.
downloadString.Post("http://www.gutenberg.org/cache/epub/16452/pg16452.txt");

// Mark the head of the pipeline as complete.
downloadString.Complete();

// Wait for the last block in the pipeline to process all messages.
printReversedWords.Completion.Wait();

Debugger.Break();

//Events - numbers and "calculations"
CancellationTokenSource cts = new CancellationTokenSource();
var ct = cts.Token;

var numbers1 = new BufferBlock<int>();
var numbers2 = new BufferBlock<int>();

var batchBlock1 = new BatchBlock<int>(10);
var batchBlock2 = new BatchBlock<int>(10);


//OR: BatchedJoinBlock<int, int> batchedJoinBlock = new BatchedJoinBlock<int, int>(10);

var joinBlock = new JoinBlock<int[], int[]>();

var multiplyBlock = new TransformBlock<Tuple<int[], int[]>, int>(data =>
{
    Console.WriteLine("Multiplying...");
    return data.Item1.Sum() * data.Item2.Sum();
});

var printBlock = new ActionBlock<int>(result =>
{
    Console.WriteLine("Result: {0}", result);
});

var lo = new DataflowLinkOptions { PropagateCompletion = true /*, MaxMessages, Append */ };

numbers1.LinkTo(batchBlock1, lo);
numbers2.LinkTo(batchBlock2, lo);

batchBlock1.LinkTo(joinBlock.Target1, lo);
batchBlock2.LinkTo(joinBlock.Target2, lo);

joinBlock.LinkTo(multiplyBlock, lo );

multiplyBlock.LinkTo(printBlock, lo );

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
Task.Run(
    () => {
        while(!ct.IsCancellationRequested)
        {
            numbers1.Post(Random.Shared.Next(1000));
        }
        numbers1.Complete();
    });
Task.Run(
    () => {
        while (!ct.IsCancellationRequested)
        {
            numbers2.Post(2000 + Random.Shared.Next(1000));
        }
        numbers2.Complete();
    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
await Task.Delay(12000);
cts.Cancel();

Debugger.Break();