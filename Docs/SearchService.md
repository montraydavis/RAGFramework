# Search Service Documentation 📚

The SearchService provides a high-level interface for performing intelligent searches across your document collection. It orchestrates the interaction between the vector store, fuzzy matching, and concept retrieval systems.

## Class Overview 🔍

```csharp
public class SearchService
{
    private readonly VectorStore _vectorStore;
    private readonly ILogger<SearchService> _logger;
}
```

## Key Features 🎯

- Asynchronous search operations
- Configurable search parameters
- Comprehensive result tracking
- Detailed logging
- Error handling

## Core Methods

### Primary Search Method
```csharp
public async Task<PredictionResult> SearchAsync(
    string query, 
    SearchOptions options = null)
```

**Parameters:**
- `query`: The search text
- `options`: Optional search configuration (uses defaults if not provided)

**Returns:**
- `PredictionResult` containing matches and metadata

**Example:**
```csharp
var searchService = serviceProvider.GetRequiredService<SearchService>();

var results = await searchService.SearchAsync(
    "machine lerning",  // Typo handled automatically
    new SearchOptions 
    { 
        MinimumScore = 0.2,
        MaxResults = 5,
        IncludeMetadata = true
    }
);
```

## Configuration

### Search Options
```csharp
public class SearchOptions
{
    public double MinimumScore { get; set; } = 0.1;
    public int MaxResults { get; set; } = 5;
    public bool IncludeMetadata { get; set; } = true;
}
```

### Default Configuration
```csharp
services.Configure<SearchOptions>(options =>
{
    options.MinimumScore = 0.1;
    options.MaxResults = 5;
    options.IncludeMetadata = true;
});
```

## Usage Examples 💡

### Basic Search
```csharp
// Simple search with default options
var results = await searchService.SearchAsync("neural networks");

foreach (var prediction in results.Predictions)
{
    Console.WriteLine($"Found: {prediction.Concept.Name}");
    Console.WriteLine($"Score: {prediction.Score:P2}");
}
```

### Advanced Search
```csharp
// Search with custom options
var options = new SearchOptions
{
    MinimumScore = 0.3,    // Higher threshold for better precision
    MaxResults = 3,        // Limit results
    IncludeMetadata = true // Include additional information
};

var results = await searchService.SearchAsync(
    "deep learning neural networks",
    options
);

// Access expanded query
Console.WriteLine($"Expanded Query: {results.ExpandedQuery}");

// Process results
foreach (var prediction in results.Predictions)
{
    Console.WriteLine($"Concept: {prediction.Concept.Name}");
    Console.WriteLine($"Score: {prediction.Score:P2}");
    
    if (prediction.Concept.Metadata.Any())
    {
        Console.WriteLine("Metadata:");
        foreach (var meta in prediction.Concept.Metadata)
        {
            Console.WriteLine($"  {meta.Key}: {meta.Value}");
        }
    }
}
```

### Error Handling
```csharp
try
{
    var results = await searchService.SearchAsync(query);
    // Process results
}
catch (Exception ex)
{
    logger.LogError(
        ex,
        "Search failed for query: {Query}",
        query);
    // Handle error appropriately
}
```

## Integration Patterns 🔄

### With LLM Systems
```csharp
public async Task<string> GetAIResponse(string userQuery)
{
    // Get relevant documents
    var searchResults = await _searchService.SearchAsync(
        userQuery,
        new SearchOptions { MinimumScore = 0.6 }
    );

    // Prepare context from search results
    var context = searchResults.Predictions
        .OrderByDescending(p => p.Score)
        .SelectMany(p => p.Concept.Documents)
        .Select(d => d.Content);

    // Use with LLM
    return await PrepareLLMResponse(userQuery, context);
}
```

### With Real-time Updates
```csharp
public async Task<PredictionResult> SearchWithRealtimeContent(
    string query,
    IEnumerable<Document> realtimeDocuments)
{
    // Perform main search
    var results = await _searchService.SearchAsync(query);

    // Enhance with realtime content
    // Implementation depends on your specific needs
    return EnhanceWithRealtimeContent(results, realtimeDocuments);
}
```

## Logging and Monitoring 📊

### Search Activity Logging
```csharp
_logger.LogInformation(
    "Search completed for query: {Query}. Found {Count} matches above threshold",
    query,
    result.Predictions.Count);
```

### Performance Tracking
```csharp
// Inside SearchAsync
var sw = Stopwatch.StartNew();
var result = await _vectorStore.PredictConceptsAsync(query);
sw.Stop();

_logger.LogInformation(
    "Search completed in {ElapsedMs}ms",
    sw.ElapsedMilliseconds);
```

## Best Practices 💡

1. **Query Optimization**
    - Keep queries focused and specific
    - Use appropriate minimum score thresholds
    - Consider result limit based on use case

2. **Error Handling**
    - Always wrap searches in try-catch blocks
    - Log errors with context
    - Provide meaningful error messages

3. **Performance**
    - Use appropriate batch sizes
    - Monitor search times
    - Cache frequently used results if appropriate

4. **Result Processing**
    - Sort by score for relevance
    - Consider score thresholds for quality
    - Process metadata when needed

## Common Use Cases 🎯

### Document Search
```csharp
var docResults = await searchService.SearchAsync(
    "technical documentation",
    new SearchOptions { MinimumScore = 0.4 }
);
```

### Knowledge Base Query
```csharp
var kbResults = await searchService.SearchAsync(
    "troubleshooting steps",
    new SearchOptions 
    { 
        MinimumScore = 0.3,
        MaxResults = 10
    }
);
```

### Concept Discovery
```csharp
var concepts = await searchService.SearchAsync(
    "machine learning applications",
    new SearchOptions { IncludeMetadata = true }
);
```