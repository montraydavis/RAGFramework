using Microsoft.Extensions.Logging;

namespace RAGFramework;

public class SearchService
{
    private readonly VectorStore _vectorStore;
    private readonly ILogger<SearchService> _logger;

    public SearchService(VectorStore vectorStore, ILogger<SearchService> logger)
    {
        _vectorStore = vectorStore;
        _logger = logger;
    }

    public async Task<PredictionResult> SearchAsync(string query, SearchOptions options = null)
    {
        options ??= new SearchOptions();

        try
        {
            var result = await _vectorStore.PredictConceptsAsync(query);

            _logger.LogInformation(
                "Search completed for query: {Query}. Found {Count} matches above threshold",
                query,
                result.Predictions.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search for query: {Query}", query);
            throw;
        }
    }
}