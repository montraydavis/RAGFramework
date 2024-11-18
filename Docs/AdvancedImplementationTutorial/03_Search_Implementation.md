# Step 3: Implementing Search Functionality 🔍

## Overview
We'll implement a robust search system that utilizes our configurations from Step 2 to provide relevant results for hedge platform queries.

## Detailed Steps

### 3.1 Create Search Service Wrapper 🎯

First, let's create a dedicated search service wrapper to handle our specific use cases:

```csharp
public class HedgePlatformSearch
{
    // ... previous code from Steps 1 and 2 ...

    public class SearchResult
    {
        public string OriginalQuery { get; set; }
        public string ExpandedQuery { get; set; }
        public List<SearchHit> Hits { get; set; } = new();
        public SearchPriority Priority { get; set; }
        public double ExecutionTime { get; set; }
    }

    public class SearchHit
    {
        public string ConceptId { get; set; }
        public string ConceptName { get; set; }
        public string DocumentId { get; set; }
        public string Content { get; set; }
        public double Score { get; set; }
        public double WeightedScore { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public List<string> MatchedTerms { get; set; }
    }

    public async Task<SearchResult> Search(
        string query,
        SearchPriority priority = SearchPriority.Balanced)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalQuery = query;
        var processedQuery = PreprocessQuery(query);
        var expandedQuery = ExpandQueryTerms(processedQuery);
        var options = CreateSearchOptions(priority);

        try
        {
            var searchResults = await _searchService.SearchAsync(
                expandedQuery,
                options);

            var hits = new List<SearchHit>();

            foreach (var prediction in searchResults.Predictions)
            {
                var weightedScore = CalculateResultWeight(prediction);
                var matchedTerms = ExtractMatchedTerms(
                    expandedQuery,
                    prediction.Concept);

                foreach (var doc in prediction.Concept.Documents)
                {
                    hits.Add(new SearchHit
                    {
                        ConceptId = prediction.ConceptId,
                        ConceptName = prediction.Concept.Name,
                        DocumentId = doc.Id,
                        Content = doc.Content,
                        Score = prediction.Score,
                        WeightedScore = weightedScore,
                        Metadata = doc.Metadata,
                        MatchedTerms = matchedTerms
                    });
                }
            }

            stopwatch.Stop();

            return new SearchResult
            {
                OriginalQuery = originalQuery,
                ExpandedQuery = expandedQuery,
                Hits = hits.OrderByDescending(h => h.WeightedScore).ToList(),
                Priority = priority,
                ExecutionTime = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            throw new SearchException(
                $"Error searching for query: {query}", ex);
        }
    }

    private List<string> ExtractMatchedTerms(
        string expandedQuery,
        Concept concept)
    {
        var queryTerms = expandedQuery.Split(' ')
            .Select(t => t.ToLower())
            .ToHashSet();

        var matchedTerms = new HashSet<string>();

        foreach (var doc in concept.Documents)
        {
            var docTerms = doc.Content.Split(' ')
                .Select(t => t.ToLower());

            foreach (var term in docTerms)
            {
                if (queryTerms.Contains(term))
                {
                    matchedTerms.Add(term);
                }
            }
        }

        return matchedTerms.ToList();
    }
}
```

### 3.2 Add Specialized Search Methods 🔨

Let's add methods for common hedge platform search scenarios:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public async Task<SearchResult> SearchClientOperations(string query)
    {
        // Enhance query with client-specific context
        var enhancedQuery = $"client {query} selection account";
        return await Search(enhancedQuery, SearchPriority.HighPrecision);
    }

    public async Task<SearchResult> SearchTradeOperations(string query)
    {
        // Enhance query with trade-specific context
        var enhancedQuery = $"trade {query} management position";
        return await Search(enhancedQuery, SearchPriority.HighPrecision);
    }

    public async Task<SearchResult> SearchBidOperations(string query)
    {
        // Enhance query with bid-specific context
        var enhancedQuery = $"bid {query} request quote";
        return await Search(enhancedQuery, SearchPriority.HighPrecision);
    }

    public async Task<SearchResult> QuickSearch(string query)
    {
        // Quick search with balanced priority
        return await Search(query, SearchPriority.Balanced);
    }
}
```

### 3.3 Implement Result Formatting 📝

Add methods to format search results for different use cases:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public class FormattedSearchResult
    {
        public string Query { get; set; }
        public int TotalHits { get; set; }
        public double ExecutionTime { get; set; }
        public List<string> Suggestions { get; set; }
        public Dictionary<string, List<string>> CategoryResults { get; set; }
    }

    public FormattedSearchResult FormatSearchResults(SearchResult result)
    {
        var formatted = new FormattedSearchResult
        {
            Query = result.OriginalQuery,
            TotalHits = result.Hits.Count,
            ExecutionTime = result.ExecutionTime,
            Suggestions = GenerateSuggestions(result),
            CategoryResults = new Dictionary<string, List<string>>()
        };

        // Group results by concept
        var groupedResults = result.Hits
            .GroupBy(h => h.ConceptName);

        foreach (var group in groupedResults)
        {
            formatted.CategoryResults[group.Key] = group
                .Select(hit => FormatHit(hit))
                .ToList();
        }

        return formatted;
    }

    private string FormatHit(SearchHit hit)
    {
        var docType = hit.Metadata.GetValueOrDefault("doc_type", "general");
        var importance = hit.Metadata.GetValueOrDefault("importance", "normal");

        return $"""
            📄 {docType.ToUpper()} - {importance}
            Score: {hit.WeightedScore:P2}
            Matched Terms: {string.Join(", ", hit.MatchedTerms)}
            
            {hit.Content}
            """;
    }

    private List<string> GenerateSuggestions(SearchResult result)
    {
        var suggestions = new List<string>();

        // Generate suggestions based on matched terms
        var allMatchedTerms = result.Hits
            .SelectMany(h => h.MatchedTerms)
            .Distinct()
            .ToList();

        // Add related operation suggestions
        if (allMatchedTerms.Contains("client"))
        {
            suggestions.Add("Try searching for specific client operations");
        }
        if (allMatchedTerms.Contains("trade"))
        {
            suggestions.Add("Try searching for trade management features");
        }
        if (allMatchedTerms.Contains("bid"))
        {
            suggestions.Add("Try searching for bid request processes");
        }

        return suggestions;
    }
}
```

### 3.4 Add Search Exception Handling 🔧

Implement proper exception handling for search operations:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public class SearchException : Exception
    {
        public string Query { get; }
        public SearchPriority Priority { get; }

        public SearchException(
            string message,
            Exception innerException = null,
            string query = "",
            SearchPriority priority = SearchPriority.Balanced)
            : base(message, innerException)
        {
            Query = query;
            Priority = priority;
        }
    }

    private async Task<SearchResult> SafeSearch(
        string query,
        SearchPriority priority)
    {
        try
        {
            return await Search(query, priority);
        }
        catch (Exception ex)
        {
            throw new SearchException(
                "Search operation failed",
                ex,
                query,
                priority);
        }
    }
}
```

### 3.5 Create Complete Search Example 🚀

Let's put it all together with a comprehensive example:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public async Task RunSearchExamples()
    {
        try
        {
            // Example 1: Quick search
            Console.WriteLine("\n📝 Quick Search Example");
            var quickResult = await QuickSearch("how to select client");
            var formattedQuick = FormatSearchResults(quickResult);
            PrintResults(formattedQuick);

            // Example 2: Client operations search
            Console.WriteLine("\n👥 Client Operations Search Example");
            var clientResult = await SearchClientOperations("view portfolio");
            var formattedClient = FormatSearchResults(clientResult);
            PrintResults(formattedClient);

            // Example 3: Trade operations search
            Console.WriteLine("\n💹 Trade Operations Search Example");
            var tradeResult = await SearchTradeOperations("modify position");
            var formattedTrade = FormatSearchResults(tradeResult);
            PrintResults(formattedTrade);

            // Example 4: Bid operations search
            Console.WriteLine("\n🔄 Bid Operations Search Example");
            var bidResult = await SearchBidOperations("submit new request");
            var formattedBid = FormatSearchResults(bidResult);
            PrintResults(formattedBid);
        }
        catch (SearchException ex)
        {
            Console.WriteLine($"❌ Search error: {ex.Message}");
            Console.WriteLine($"Query: {ex.Query}");
            Console.WriteLine($"Priority: {ex.Priority}");
        }
    }

    private void PrintResults(FormattedSearchResult results)
    {
        Console.WriteLine($"\nQuery: {results.Query}");
        Console.WriteLine($"Found {results.TotalHits} results " +
            $"in {results.ExecutionTime}ms");

        if (results.Suggestions.Any())
        {
            Console.WriteLine("\n💡 Suggestions:");
            foreach (var suggestion in results.Suggestions)
            {
                Console.WriteLine($"- {suggestion}");
            }
        }

        Console.WriteLine("\n📊 Results by Category:");
        foreach (var category in results.CategoryResults)
        {
            Console.WriteLine($"\n== {category.Key} ==");
            foreach (var result in category.Value)
            {
                Console.WriteLine(result);
                Console.WriteLine("---");
            }
        }
    }
}
```

### 3.6 Update Main Program 🏃‍♂️

Update the main program to run our examples:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public static async Task Main(string[] args)
    {
        var hedgeSearch = new HedgePlatformSearch();
        
        try
        {
            await hedgeSearch.Initialize();
            await hedgeSearch.RunSearchExamples();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
```

This completes Step 3 of our tutorial. We now have:
- ✅ Implemented comprehensive search functionality
- ✅ Added specialized search methods for different operations
- ✅ Created result formatting and display
- ✅ Implemented proper error handling
- ✅ Added working examples
