using System.Diagnostics;

namespace SortTiming;

class Program
{
    // Default timeout in seconds (can be configured)
    private const int DefaultTimeoutSeconds = 30;
    
    static void Main(string[] args)
    {
        Console.WriteLine("Sorting Algorithm Performance Tester");
        Console.WriteLine("====================================");
        
        // Get array size from user
        Console.Write("Enter array size: ");
        if (!int.TryParse(Console.ReadLine(), out int size) || size <= 0)
        {
            Console.WriteLine("Invalid size. Using default size of 10000.");
            size = 10000;
        }
        
        // Get timeout from user
        Console.Write($"Enter timeout in seconds (default {DefaultTimeoutSeconds}): ");
        if (!int.TryParse(Console.ReadLine(), out int timeoutSeconds) || timeoutSeconds <= 0)
        {
            timeoutSeconds = DefaultTimeoutSeconds;
        }
        
        // Generate random array
        int[] array = GenerateRandomArray(size);
        Console.WriteLine($"\nGenerated random array of {size} elements");
        Console.WriteLine($"Timeout: {timeoutSeconds} seconds\n");
        
        // Add your sorting algorithm implementations here
        List<ISortingAlgorithm> algorithms = new List<ISortingAlgorithm>
        {
            //new NotSort(),
            // new RandomSort(),
            // new BubbleSort(),
            // new QuickSort(),
            // new MergeSort(),
            // Add more algorithms as needed
        };
        
        if (algorithms.Count == 0)
        {
            Console.WriteLine("No sorting algorithms added yet.");
            Console.WriteLine("Create classes that implement ISortingAlgorithm and add them to the algorithms list.");
            return;
        }
        
        // Test each algorithm on the same array
        var results = new List<(string Name, long Milliseconds, string Status)>();
        
        foreach (var algorithm in algorithms)
        {
            var result = TimeSort(algorithm, array, timeoutSeconds);
            results.Add(result);
        }
        
        // Display summary
        Console.WriteLine("\n===========================================");
        Console.WriteLine("SUMMARY");
        Console.WriteLine("===========================================");
        Console.WriteLine($"{"Algorithm",-30} {"Time (ms)",12} {"Status",12}");
        Console.WriteLine(new string('-', 57));
        
        foreach (var result in results.OrderBy(r => r.Milliseconds))
        {
            string timeDisplay = result.Milliseconds >= 0 ? result.Milliseconds.ToString() : "--";
            Console.WriteLine($"{result.Name,-30} {timeDisplay,12} {result.Status,12}");
        }
    }
    
    static int[] GenerateRandomArray(int size)
    {
        Random random = new Random();
        int[] array = new int[size];
        
        for (int i = 0; i < size; i++)
        {
            array[i] = random.Next(0, size * 10);
        }
        
        return array;
    }
    
    static (string Name, long Milliseconds, string Status) TimeSort(ISortingAlgorithm algorithm, int[] array, int timeoutSeconds)
    {
        // Create a copy to preserve original array
        int[] arrayCopy = (int[])array.Clone();
        
        Console.WriteLine($"Testing: {algorithm.Name}");
        Console.Write("Sorting... ");
        
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
        
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool completed = false;
        bool timedOut = false;
        
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
        }, cts.Token);
        
        try
        {
            sortTask.Wait(cts.Token);
        }
        catch (OperationCanceledException)
        {
            timedOut = true;
        }
        
        stopwatch.Stop();
        long elapsedMs = stopwatch.ElapsedMilliseconds;
        
        if (timedOut)
        {
            Console.WriteLine($"TIMED OUT after {elapsedMs} ms ({stopwatch.Elapsed.TotalSeconds:F3} seconds)");
            Console.WriteLine($"Status: TIMEOUT ⏱\n");
            return (algorithm.Name, elapsedMs, "TIMEOUT");
        }
        
        Console.WriteLine($"completed in {elapsedMs} ms ({stopwatch.Elapsed.TotalSeconds:F3} seconds)");
        
        // Verify array is sorted
        bool isSorted = VerifySorted(arrayCopy);
        string status = isSorted ? "PASSED" : "FAILED";
        Console.WriteLine($"Verification: {(isSorted ? "PASSED ✓" : "FAILED ✗")}\n");
        
        return (algorithm.Name, elapsedMs, status);
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
