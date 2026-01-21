namespace SortTiming;


public static class DataGenerator
{
    public static int[] GenerateRandomArray(int size)
    {
        Random random = new Random();
        int[] array = new int[size];

        for (int i = 0; i < size; i++)
        {
            array[i] = random.Next(0, size * 10);
        }

        return array;
    }

    public static int[] GenerateSortedArray(int size)
    {
        int[] array = new int[size];

        for (int i = 0; i < size; i++)
        {
            array[i] = i;
        }

        return array;
    }

    public static int[] GenerateReverseSortedArray(int size)
    {
        int[] array = new int[size];

        for (int i = 0; i < size; i++)
        {
            array[i] = size - 1 - i;
        }

        return array;
    }
}