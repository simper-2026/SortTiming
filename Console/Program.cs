using System.Diagnostics;

namespace SortTiming;

class Program
{
    // Run each test multiple times for statistical accuracy
    // - Warmup runs allow JIT compilation and cache warming (results discarded)
    // - Multiple measured runs smooth out system variance (CPU scheduling, background tasks)
    // - Statistical measures (mean, min, max, std dev) provide reliability information
    private const int WarmupRuns = 1;
    private const int MeasuredRuns = 5;
    
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
            Console.WriteLine("** No sorting algorithms added yet. **");
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
            reverseResult.ArrayType = "Reversed";
            results.Add(reverseResult);
        }
        
        Console.WriteLine("\n" + new string('=', 130));
        Console.WriteLine("SUMMARY");
        Console.WriteLine(new string('=', 130));
        Console.WriteLine($"{"Algorithm",-20} {"Array Type",-12} {"Mean (ms)",12} {"Min (ms)",12} {"Max (ms)",12} {"StdDev (ms)",14} {"Status",10}");
        Console.WriteLine(new string('-', 130));
        
        foreach (var algorithmGroup in results.GroupBy(r => r.Algorithm))
        {
            var algorithmResults = algorithmGroup.ToList();
            bool firstRow = true;
            
            foreach (var result in algorithmResults.OrderBy(r => r.ArrayType))
            {
                string algorithmName = firstRow ? algorithmGroup.Key : "";
                firstRow = false;
                
                string statusDisplay = result.Status == "PASSED" ? "✓" : 
                                     result.Status == "FAILED" ? "✗ FAILED" :
                                     result.Status;
                
                if (result.Status == "TIMEOUT" || result.Status == "ERROR")
                {
                    Console.WriteLine($"{algorithmName,-20} {result.ArrayType,-12} {"-",12} {"-",12} {"-",12} {"-",14} {statusDisplay,10}");
                }
                else
                {
                    Console.WriteLine($"{algorithmName,-20} {result.ArrayType,-12} {result.MeanMilliseconds,12} {result.MinMilliseconds,12} {result.MaxMilliseconds,12} {result.StdDevMilliseconds,14:F2} {statusDisplay,10}");
                }
            }
            
            if (algorithmResults.Count > 0)
            {
                Console.WriteLine(new string('-', 130));
            }
        }
        
        Console.WriteLine(new string('=', 130));
    }
    
    static SortResult TimeSort(ISortingAlgorithm algorithm, int[] array, int timeoutSeconds)
    {
        Console.WriteLine($"Running {WarmupRuns} warmup + {MeasuredRuns} measured iterations...");
        
        var measuredTimes = new List<long>();
        bool isSorted = true;
        
        int totalRuns = WarmupRuns + MeasuredRuns;
        
        for (int run = 0; run < totalRuns; run++)
        {
            // Create a fresh copy for each run
            int[] arrayCopy = (int[])array.Clone();
            
            bool isWarmup = run < WarmupRuns;
            Console.Write($"  {(isWarmup ? "Warmup" : "Run")} {(isWarmup ? run + 1 : run - WarmupRuns + 1)}: ");
            
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool timedOut = false;
            Exception? exception = null;
            
            // The stack had to be increased to handle deep recursion
            Thread sortThread = new Thread(() =>
            {
                try
                {
                    algorithm.Sort(arrayCopy);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }, 16 * 1024 * 1024); // 16 MB stack (default is 1 MB)
            
            sortThread.Start();
            
            if (!sortThread.Join(TimeSpan.FromSeconds(timeoutSeconds)))
            {
                timedOut = true;
                sortThread.Interrupt();
                // Give thread a moment to stop, but don't wait forever
                sortThread.Join(100);
            }
            
            stopwatch.Stop();
            long elapsedMs = stopwatch.ElapsedMilliseconds;
            
            if (exception != null)
            {
                Console.WriteLine($"EXCEPTION after {elapsedMs} ms");
                Console.WriteLine($"Error: {exception.GetType().Name}");
                if (exception.Message.Length < 200)
                {
                    Console.WriteLine($"Message: {exception.Message}");
                }
                else
                {
                    Console.WriteLine($"Message: {exception.Message.Substring(0, 197)}...");
                }
                Console.WriteLine($"Status: ERROR ✗\n");
                
                // Record the error time for this run but continue to avoid crash
                if (!isWarmup)
                {
                    measuredTimes.Add(elapsedMs);
                }
                
                // If this is an early failure, still return error result
                if (run < 2)
                {
                    return new SortResult(algorithm.Name, "", "ERROR");
                }
                // Otherwise break out of the loop to report partial results
                break;
            }
            
            if (timedOut)
            {
                Console.WriteLine($"TIMED OUT after {elapsedMs} ms ({stopwatch.Elapsed.TotalSeconds:F3} seconds)");
                Console.WriteLine($"Status: TIMEOUT ⏱\n");
                return new SortResult(algorithm.Name, "", "TIMEOUT");
            }
            
            Console.WriteLine($"{elapsedMs} ms");
            
            // Verify correctness (only need to check once, but check last measured run)
            if (run == totalRuns - 1)
            {
                isSorted = VerifySorted(arrayCopy);
            }
            
            // Only record times for measured runs (skip warmup)
            if (!isWarmup)
            {
                measuredTimes.Add(elapsedMs);
            }
        }
        
        // Handle case where we had errors but got some measurements
        if (measuredTimes.Count == 0)
        {
            return new SortResult(algorithm.Name, "", "ERROR");
        }
        
        var retval = new SortResult(algorithm.Name, "", "");
        retval.MinMilliseconds = measuredTimes.Min();
        retval.MaxMilliseconds = measuredTimes.Max();   
        retval.MeanMilliseconds = (long)measuredTimes.Average();
        retval.StdDevMilliseconds = CalculateStdDev(measuredTimes);
        
        string status = isSorted ? "PASSED" : "FAILED";
        Console.WriteLine($"\nStatistics: Mean={retval.MeanMilliseconds}ms, Min={retval.MinMilliseconds}ms, Max={retval.MaxMilliseconds}ms, StdDev={retval.StdDevMilliseconds:F2}ms");
        Console.WriteLine($"Verification: {(isSorted ? "PASSED ✓" : "FAILED ✗")}\n");
        
        return retval;
    }
    
    static double CalculateStdDev(List<long> values)
    {
        if (values.Count <= 1) return 0;
        
        double mean = values.Average();
        double sumSquaredDiffs = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumSquaredDiffs / values.Count);
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
