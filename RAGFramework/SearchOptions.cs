namespace RAGFramework;

public class SearchOptions
{
    public double MinimumScore { get; set; } = 0.1;
    public int MaxResults { get; set; } = 5;
    public bool IncludeMetadata { get; set; } = true;
}