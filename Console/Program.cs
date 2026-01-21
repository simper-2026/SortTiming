using System.Diagnostics;

namespace SortTiming;

class Program
{
    private const int DefaultTimeoutSeconds = 30;
    
    static void Main(string[] args)
    {
        Console.WriteLine("Sorting Algorithm Performance Tester");
        Console.WriteLine("====================================");
        
        Console.Write("Enter array size: ");
        if (!int.TryParse(Console.ReadLine(), out int size) || size <= 0)
        {
            Console.WriteLine("Invalid size. Using default size of 10000.");
            size = 10000;
        }
        
        Console.Write($"Enter timeout in seconds (default {DefaultTimeoutSeconds}): ");
        if (!int.TryParse(Console.ReadLine(), out int timeoutSeconds) || timeoutSeconds <= 0)
        {
            timeoutSeconds = DefaultTimeoutSeconds;
        }
        
        var randomArray = DataGenerator.GenerateRandomArray(size);
        var sortedArray = DataGenerator.GenerateSortedArray(size);
        var reverseArray = DataGenerator.GenerateReverseSortedArray(size);
        
        Console.WriteLine($"\nGenerated test arrays of {size} elements");
        Console.WriteLine($"Timeout: {timeoutSeconds} seconds\n");
        
        // Add your sorting algorithm implementations here
        List<ISortingAlgorithm> algorithms = new List<ISortingAlgorithm>
        {
            // new BubbleSort(),
            // new MergeSort(),
            // new QuickSort(),
        };
        
        if (algorithms.Count == 0)
        {
            Console.WriteLine("No sorting algorithms added yet.");
            Console.WriteLine("Create classes that implement ISortingAlgorithm and add them to the algorithms list.");
            return;
        }
        
        var results = new List<SortResult>();
        
        foreach (var algorithm in algorithms)
        {
            Console.WriteLine($"\n{'='*60}");
            Console.WriteLine($"Algorithm: {algorithm.Name}");
            Console.WriteLine($"{'='*60}\n");
            
            Console.WriteLine("[RANDOM ARRAY]");
            var randomResult = TimeSort(algorithm, randomArray, timeoutSeconds);
            randomResult.ArrayType = "Random";
            results.Add(randomResult);
            
            Console.WriteLine("[SORTED ARRAY]");
            var sortedResult = TimeSort(algorithm, sortedArray, timeoutSeconds);
            sortedResult.ArrayType = "Sorted";
            results.Add(sortedResult);
            
            Console.WriteLine("[REVERSE SORTED ARRAY]");
            var reverseResult = TimeSort(algorithm, reverseArray, timeoutSeconds);
            reverseResult.ArrayType = "Reverse";
            results.Add(reverseResult);
        }
        
        Console.WriteLine("\n" + new string('=', 85));
        Console.WriteLine("SUMMARY");
        Console.WriteLine(new string('=', 85));
        Console.WriteLine($"{"Algorithm",-30} {"Random",15} {"Sorted",15} {"Reverse",15}");
        Console.WriteLine(new string('-', 85));
        
        foreach (var algorithmGroup in results.GroupBy(r => r.Algorithm))
        {
            var algorithmResults = algorithmGroup.ToList();
            var randomResult = algorithmResults.FirstOrDefault(r => r.ArrayType == "Random");
            var sortedResult = algorithmResults.FirstOrDefault(r => r.ArrayType == "Sorted");
            var reverseResult = algorithmResults.FirstOrDefault(r => r.ArrayType == "Reverse");
            
            string randomDisplay = FormatResult(randomResult.Milliseconds, randomResult.Status);
            string sortedDisplay = FormatResult(sortedResult.Milliseconds, sortedResult.Status);
            string reverseDisplay = FormatResult(reverseResult.Milliseconds, reverseResult.Status);
            
            Console.WriteLine($"{algorithmGroup.Key,-30} {randomDisplay,15} {sortedDisplay,15} {reverseDisplay,15}");
        }
        
        Console.WriteLine(new string('=', 85));
    }
    
    static string FormatResult(long milliseconds, string status)
    {
        if (status == "TIMEOUT")
        {
            return "TIMEOUT";
        }
        else if (status == "ERROR")
        {
            return "ERROR";
        }
        else if (status == "FAILED")
        {
            return $"{milliseconds} ms (!)";
        }
        else
        {
            return $"{milliseconds} ms";
        }
    }
    
    static SortResult TimeSort(ISortingAlgorithm algorithm, int[] array, int timeoutSeconds)
    {
        // Create a copy to preserve original array
        int[] arrayCopy = (int[])array.Clone();
        
        Console.Write("Sorting... ");
        
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
        
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool completed = false;
        bool timedOut = false;
        Exception? exception = null;
        
        var sortTask = Task.Run(() =>
        {
            try
            {
                algorithm.Sort(arrayCopy);
                completed = true;
            }
            catch (OperationCanceledException)
            {
                timedOut = true;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }, cts.Token);
        
        try
        {
            sortTask.Wait(cts.Token);
        }
        catch (AggregateException ae)
        {
            foreach (var ex in ae.InnerExceptions)
            {
                if (ex is not OperationCanceledException)
                {
                    exception = ex;
                }
                else
                {
                    timedOut = true;
                }
            }
        }
        catch (OperationCanceledException)
        {
            timedOut = true;
        }
        
        stopwatch.Stop();
        long elapsedMs = stopwatch.ElapsedMilliseconds;
        
        if (exception != null)
        {
            Console.WriteLine($"EXCEPTION after {elapsedMs} ms");
            Console.WriteLine($"Error: {exception.GetType().Name}");
            if (exception.Message.Length < 100)
            {
                Console.WriteLine($"Message: {exception.Message}");
            }
            Console.WriteLine($"Status: ERROR ✗\n");
            return new SortResult(algorithm.Name, "", elapsedMs, "ERROR");
        }
        
        if (timedOut)
        {
            Console.WriteLine($"TIMED OUT after {elapsedMs} ms ({stopwatch.Elapsed.TotalSeconds:F3} seconds)");
            Console.WriteLine($"Status: TIMEOUT ⏱\n");
            return new SortResult(algorithm.Name, "", elapsedMs, "TIMEOUT");
        }
        
        Console.WriteLine($"completed in {elapsedMs} ms ({stopwatch.Elapsed.TotalSeconds:F3} seconds)");
        
        bool isSorted = VerifySorted(arrayCopy);
        string status = isSorted ? "PASSED" : "FAILED";
        Console.WriteLine($"Verification: {(isSorted ? "PASSED ✓" : "FAILED ✗")}\n");
        
        return new SortResult(algorithm.Name, "", elapsedMs, status);
    }
    
    static bool VerifySorted(int[] array)
    {
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] < array[i - 1])
            {
                return false;
            }
        }
        return true;
    }
}
