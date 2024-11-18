# VectorStore Documentation 📚

The VectorStore is a core component responsible for converting documents into vector representations and performing similarity searches. It combines TF-IDF vectorization with fuzzy matching for robust document retrieval.

## Key Features 🎯

- TF-IDF based document vectorization
- Parallel processing of concepts
- Fuzzy matching integration
- Cached vocabulary management
- Cosine similarity search

## Class Overview 🔍

```csharp
public class VectorStore
{
    private readonly TfIdfVectorizer _vectorizer;
    private readonly ConcurrentDictionary<string, float[]> _conceptVectors;
    private readonly ConceptStore _conceptStore;
    private readonly FuzzyMatchService _fuzzyMatch;
    private readonly ILogger<VectorStore> _logger;
    private HashSet<string> _vocabulary;
}
```

## Core Methods

### Building the Vector Store
```csharp
public async Task BuildVectorStoreAsync()
```
Processes all concepts and their documents to build the vector representations.

**Key Steps:**
1. Builds vocabulary from all documents
2. Trains the TF-IDF vectorizer
3. Processes concepts in parallel
4. Creates averaged concept vectors

**Example:**
```csharp
var vectorStore = new VectorStore(conceptStore, fuzzyMatch, logger);
await vectorStore.BuildVectorStoreAsync();
```

### Searching Similar Concepts
```csharp
public List<(string ConceptId, double Score, Concept Concept)> SearchSimilarConcepts(
    string query,
    double minimumScore = 0.1,
    int topK = 5)
```
Finds concepts similar to a given query using cosine similarity.

**Parameters:**
- `query`: The search text
- `minimumScore`: Minimum similarity threshold (0-1)
- `topK`: Maximum number of results to return

**Example:**
```csharp
var similarConcepts = vectorStore.SearchSimilarConcepts(
    "machine learning algorithms",
    minimumScore: 0.2,
    topK: 3
);
```

### Predicting Concepts
```csharp
public async Task<PredictionResult> PredictConceptsAsync(string query)
```
Combines fuzzy matching and vector similarity to find relevant concepts.

**Key Features:**
- Query term expansion
- Fuzzy matching integration
- Relevance scoring
- Detailed result metadata

**Example:**
```csharp
var prediction = await vectorStore.PredictConceptsAsync(
    "neural neworks for deep lerning"  // Typos handled!
);
```

## Internal Processing 🔄

### Vector Creation Process
1. Document tokenization
2. Vocabulary building
3. TF-IDF transformation
4. Vector averaging per concept

```csharp
// Internal vector creation flow
var conceptDocs = concept.Documents
    .Select(d => _vectorizer.Transform(d.Content))
    .ToList();

// Average vectors for concept representation
var conceptVector = new float[vectorLength];
foreach (var docVector in conceptDocs)
{
    for (int i = 0; i < vectorLength; i++)
    {
        conceptVector[i] += docVector[i];
    }
}
```

### Similarity Calculation
Uses cosine similarity for comparing vectors:
```csharp
var queryVector = _vectorizer.Transform(query);
var similarity = _vectorizer.CalculateCosineSimilarity(queryVector, conceptVector);
```

## Best Practices 💡

1. **Building the Store**
    - Call `BuildVectorStoreAsync()` after adding/updating concepts
    - Consider rebuilding periodically for dynamic content

2. **Search Optimization**
    - Adjust `minimumScore` based on desired precision
    - Use `topK` to limit result set size
    - Monitor and log search performance

3. **Memory Management**
    - The vector store keeps vectors in memory
    - Consider the size of your document collection

## Example Usage Scenarios

### Basic Search
```csharp
// Build the store
await vectorStore.BuildVectorStoreAsync();

// Perform a search
var results = await vectorStore.PredictConceptsAsync("machine learning");

foreach (var prediction in results.Predictions)
{
    Console.WriteLine($"Concept: {prediction.Concept.Name}");
    Console.WriteLine($"Score: {prediction.Score:P2}");
}
```

### Advanced Search with Fuzzy Matching
```csharp
// Search with typos and variations
var results = await vectorStore.PredictConceptsAsync(
    "artifical intellignce and neural netwrks"
);

// Expanded query will correct typos
Console.WriteLine($"Expanded Query: {results.ExpandedQuery}");
```

## Logging and Diagnostics 📊

The VectorStore provides detailed logging:
- Build process progress
- Search operations
- Error conditions
- Performance metrics

```csharp
_logger.LogDebug(
    "Expanded query from '{OriginalQuery}' to '{ExpandedQuery}'",
    query,
    expandedQueryText
);
```