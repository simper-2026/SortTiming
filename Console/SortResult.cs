namespace SortTiming;

class SortResult
{
    public string Algorithm { get; set; }
    public string ArrayType { get; set; }
    public long Milliseconds { get; set; }
    public string Status { get; set; }

    public SortResult(string algorithm, string arrayType, long milliseconds, string status)
    {
        Algorithm = algorithm;
        ArrayType = arrayType;
        Milliseconds = milliseconds;
        Status = status;
    }
}
