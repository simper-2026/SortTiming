namespace SortTiming;


public class NotSort : ISortingAlgorithm
{
    public string Name => "No Sorting (NotSort)";
    public string Runtime => "N/A";

    public void Sort(int[] array)
    {
        // Intentionally does nothing
    }
}