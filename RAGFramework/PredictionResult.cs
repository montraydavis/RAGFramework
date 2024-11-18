namespace RAGFramework;

public class PredictionResult
{
    public string Query { get; set; }
    public string ExpandedQuery { get; set; }  // Added this property
    public List<ConceptPrediction> Predictions { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}