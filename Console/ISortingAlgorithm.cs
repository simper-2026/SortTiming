namespace SortTiming;

/// <summary>
/// Interface for implementing sorting algorithms
/// </summary>
public interface ISortingAlgorithm
{
    /// <summary>
    /// Gets the name of the sorting algorithm
    /// </summary>
    string Name { get; }

    string Runtime { get; }
    
    /// <summary>
    /// Sorts the array in ascending order
    /// </summary>
    /// <param name="array">The array to sort</param>
    void Sort(int[] array);
}
