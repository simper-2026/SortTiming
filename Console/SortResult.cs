namespace SortTiming;

class SortResult
{
    public string Algorithm { get; set; }
    public string ArrayType { get; set; }
    public long MeanMilliseconds { get; set; } 
    public long MinMilliseconds { get; set; }
    public long MaxMilliseconds { get; set; }
    public double StdDevMilliseconds { get; set; }
    public string Status { get; set; }

    public SortResult(string algorithm, string arrayType, string status)
    {
        Algorithm = algorithm;
        ArrayType = arrayType;
        MeanMilliseconds = 0;
        MinMilliseconds = 0;
        MaxMilliseconds = 0;
        StdDevMilliseconds = 0;
        Status = status;
    }
}
