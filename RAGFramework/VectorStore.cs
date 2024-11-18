using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RAGFramework.Algos;

namespace RAGFramework;

public class VectorStore
{
    private readonly TfIdfVectorizer _vectorizer;
    private readonly ConcurrentDictionary<string, float[]> _conceptVectors;
    private readonly ConceptStore _conceptStore;
    private readonly FuzzyMatchService _fuzzyMatch;
    private readonly ILogger<VectorStore> _logger;
    private HashSet<string> _vocabulary;

    public VectorStore(
        ConceptStore conceptStore,
        FuzzyMatchService fuzzyMatch,
        ILogger<VectorStore> logger)
    {
        _vectorizer = new TfIdfVectorizer();
        _conceptVectors = new ConcurrentDictionary<string, float[]>();
        _conceptStore = conceptStore;
        _fuzzyMatch = fuzzyMatch;
        _logger = logger;
    }

    public async Task BuildVectorStoreAsync()
    {
        try
        {
            
            // Build vocabulary from all documents
            _vocabulary = new HashSet<string>(_conceptStore.GetAllConcepts()
                .SelectMany(c => c.Documents)
                .SelectMany(d => _vectorizer.GetTokens(d.Content)));
            
            // Collect all documents for training
            var allDocuments = _conceptStore.GetAllConcepts()
                .SelectMany(c => c.Documents)
                .Select(d => d.Content)
                .ToList();

            if (!allDocuments.Any())
            {
                _logger.LogWarning("No documents found to build vector store");
                return;
            }

            // Train the vectorizer
            _vectorizer.Fit(allDocuments);

            // Process each concept in parallel
            var concepts = _conceptStore.GetAllConcepts();
            await Parallel.ForEachAsync(concepts, async (concept, ct) =>
            {
                try
                {
                    var conceptDocs = concept.Documents
                        .Select(d => _vectorizer.Transform(d.Content))
                        .ToList();

                    if (conceptDocs.Any())
                    {
                        var vectorLength = conceptDocs[0].Length;
                        var conceptVector = new float[vectorLength];

                        // Average the document vectors
                        foreach (var docVector in conceptDocs)
                        {
                            for (int i = 0; i < vectorLength; i++)
                            {
                                conceptVector[i] += docVector[i];
                            }
                        }

                        for (int i = 0; i < vectorLength; i++)
                        {
                            conceptVector[i] /= conceptDocs.Count;
                        }

                        _conceptVectors.TryAdd(concept.Id, conceptVector);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing concept {ConceptId}", concept.Id);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building vector store");
            throw;
        }
    }

    public List<(string ConceptId, double Score, Concept Concept)> SearchSimilarConcepts(
        string query,
        double minimumScore = 0.1,
        int topK = 5)
    {
        try
        {
            var queryVector = _vectorizer.Transform(query);

            return _conceptVectors
                .AsParallel()
                .Select(kv => (
                    ConceptId: kv.Key,
                    Score: _vectorizer.CalculateCosineSimilarity(queryVector, kv.Value),
                    Concept: _conceptStore.GetConcept(kv.Key)
                ))
                .Where(x => x.Score >= minimumScore)
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching concepts for query: {Query}", query);
            throw;
        }
    }

    public async Task<PredictionResult> PredictConceptsAsync(string query)
    {
        try
        {
            // Split query into terms
            var queryTerms = _vectorizer.GetTokens(query);
            
            // Expand terms using fuzzy matching
            var expandedQuery = queryTerms
                .SelectMany(term => _fuzzyMatch.ExpandTerms(term, _vocabulary))
                .Distinct()
                .ToList();

            // Join expanded terms back into a query
            var expandedQueryText = string.Join(" ", expandedQuery);

            _logger.LogDebug(
                "Expanded query from '{OriginalQuery}' to '{ExpandedQuery}'",
                query,
                expandedQueryText);

            // Use expanded query for search
            var searchResults = SearchSimilarConcepts(expandedQueryText);

            return new PredictionResult
            {
                Query = query,
                ExpandedQuery = expandedQueryText,
                Predictions = searchResults
                    .Select(r => new ConceptPrediction
                    {
                        ConceptId = r.ConceptId,
                        Score = r.Score,
                        Concept = r.Concept
                    })
                    .ToList(),
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting concepts for query: {Query}", query);
            throw;
        }
    }
}