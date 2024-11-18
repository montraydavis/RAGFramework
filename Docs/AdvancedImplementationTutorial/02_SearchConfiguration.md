# Step 2: Configuring Search Parameters 🔧

## Overview
In this step, we'll configure our search system with appropriate thresholds, weights, and settings specifically tuned for hedge platform terminology and user queries.

## Detailed Steps

### 2.1 Create Search Configuration Class 🎯

First, let's create a dedicated configuration class to manage our search settings:

```csharp
public class HedgePlatformSearch
{
    // ... previous code from Step 1 ...

    private class SearchConfiguration
    {
        // Search thresholds
        public const double HIGH_CONFIDENCE_THRESHOLD = 0.8;
        public const double MEDIUM_CONFIDENCE_THRESHOLD = 0.6;
        public const double LOW_CONFIDENCE_THRESHOLD = 0.4;

        // Result limits
        public const int MAX_RESULTS_DETAILED = 5;
        public const int MAX_RESULTS_SUMMARY = 3;

        // Fuzzy matching settings
        public const double FUZZY_MATCH_THRESHOLD = 0.85;
        public const int MAX_FUZZY_EXPANSIONS = 3;

        // Importance weights
        public const double OVERVIEW_DOC_WEIGHT = 1.2;
        public const double GUIDE_DOC_WEIGHT = 1.1;
        public const double PROCESS_DOC_WEIGHT = 1.0;
    }
}
```

### 2.2 Configure Search Options 🛠

Let's create methods to configure our search options based on different use cases:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    private SearchOptions CreateSearchOptions(SearchPriority priority = SearchPriority.Balanced)
    {
        return priority switch
        {
            SearchPriority.HighPrecision => new SearchOptions
            {
                MinimumScore = SearchConfiguration.HIGH_CONFIDENCE_THRESHOLD,
                MaxResults = SearchConfiguration.MAX_RESULTS_DETAILED,
                IncludeMetadata = true
            },
            SearchPriority.HighRecall => new SearchOptions
            {
                MinimumScore = SearchConfiguration.LOW_CONFIDENCE_THRESHOLD,
                MaxResults = SearchConfiguration.MAX_RESULTS_DETAILED,
                IncludeMetadata = true
            },
            _ => new SearchOptions
            {
                MinimumScore = SearchConfiguration.MEDIUM_CONFIDENCE_THRESHOLD,
                MaxResults = SearchConfiguration.MAX_RESULTS_SUMMARY,
                IncludeMetadata = true
            }
        };
    }

    public enum SearchPriority
    {
        Balanced,
        HighPrecision,
        HighRecall
    }
}
```

### 2.3 Configure Fuzzy Matching 🔍

Add fuzzy matching configuration for handling variations in hedge platform terminology:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    private void ConfigureFuzzyMatching(IServiceCollection services)
    {
        services.Configure<FuzzyMatchOptions>(options =>
        {
            options.SimilarityThreshold = SearchConfiguration.FUZZY_MATCH_THRESHOLD;
            options.MaxExpansionTerms = SearchConfiguration.MAX_FUZZY_EXPANSIONS;
            options.EnableCache = true;
        });

        // Common hedge platform terms and their variations
        _hedgeTermVariations = new Dictionary<string, HashSet<string>>
        {
            {
                "client", new HashSet<string>
                {
                    "customer", "account", "counterparty"
                }
            },
            {
                "trade", new HashSet<string>
                {
                    "transaction", "deal", "position"
                }
            },
            {
                "bid", new HashSet<string>
                {
                    "offer", "quote", "proposal"
                }
            }
        };
    }

    private readonly Dictionary<string, HashSet<string>> _hedgeTermVariations;
}
```

### 2.4 Implement Query Preprocessing 📝

Add methods to preprocess and enhance search queries:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    private string PreprocessQuery(string query)
    {
        return query.ToLower().Trim();
    }

    private string ExpandQueryTerms(string query)
    {
        var terms = query.Split(' ');
        var expandedTerms = new List<string>();

        foreach (var term in terms)
        {
            expandedTerms.Add(term);  // Add original term

            // Add known variations
            if (_hedgeTermVariations.TryGetValue(term.ToLower(), out var variations))
            {
                expandedTerms.AddRange(variations);
            }
        }

        return string.Join(" ", expandedTerms.Distinct());
    }
}
```

### 2.5 Add Result Weighting ⚖️

Implement result weighting based on document type and metadata:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    private double CalculateResultWeight(ConceptPrediction prediction)
    {
        var baseScore = prediction.Score;
        var weight = 1.0;

        // Apply weights based on document type
        foreach (var doc in prediction.Concept.Documents)
        {
            if (doc.Metadata.TryGetValue("doc_type", out var docType))
            {
                weight *= docType switch
                {
                    "overview" => SearchConfiguration.OVERVIEW_DOC_WEIGHT,
                    "guide" => SearchConfiguration.GUIDE_DOC_WEIGHT,
                    "process" => SearchConfiguration.PROCESS_DOC_WEIGHT,
                    _ => 1.0
                };
            }

            // Apply weights based on importance
            if (doc.Metadata.TryGetValue("importance", out var importance))
            {
                weight *= importance switch
                {
                    "high" => 1.2,
                    "medium" => 1.1,
                    "low" => 1.0,
                    _ => 1.0
                };
            }
        }

        return baseScore * weight;
    }
}
```

### 2.6 Update Initialization 🔄

Update our initialization method to include the new configurations:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public HedgePlatformSearch()
    {
        var services = new ServiceCollection();
        
        // Configure fuzzy matching
        ConfigureFuzzyMatching(services);

        // Add framework with configurations
        services.AddRAGFramework()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                });

        var serviceProvider = services.BuildServiceProvider();

        _conceptStore = serviceProvider.GetRequiredService<ConceptStore>();
        _vectorStore = serviceProvider.GetRequiredService<VectorStore>();
        _searchService = serviceProvider.GetRequiredService<SearchService>();
    }

    public async Task Initialize()
    {
        try
        {
            await CreateConcepts();
            await AddDocuments();
            await _vectorStore.BuildVectorStoreAsync();
            
            // Validate configurations
            await ValidateConfigurations();
            
            Console.WriteLine("✅ Successfully initialized Hedge Platform Search");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error initializing search: {ex.Message}");
            throw;
        }
    }

    private async Task ValidateConfigurations()
    {
        // Test different search priorities
        var testQuery = "client selection process";
        var priorities = Enum.GetValues<SearchPriority>();

        foreach (var priority in priorities)
        {
            var options = CreateSearchOptions(priority);
            Console.WriteLine($"\nTesting {priority} priority:");
            Console.WriteLine($"Minimum Score: {options.MinimumScore}");
            Console.WriteLine($"Max Results: {options.MaxResults}");

            var expandedQuery = ExpandQueryTerms(PreprocessQuery(testQuery));
            Console.WriteLine($"Expanded Query: {expandedQuery}");
        }
    }
}
```

### 2.7 Create a Configuration Testing Method 🧪

Let's add a method to test our configurations:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public async Task TestSearchConfigurations()
    {
        var testQueries = new[]
        {
            "client selection",
            "trade managment",  // Intentional typo
            "request for bid",
            "customer account selection"  // Using variation
        };

        foreach (var query in testQueries)
        {
            Console.WriteLine($"\nTesting query: {query}");
            
            // Test with different priorities
            foreach (var priority in Enum.GetValues<SearchPriority>())
            {
                var options = CreateSearchOptions(priority);
                var expandedQuery = ExpandQueryTerms(PreprocessQuery(query));
                
                Console.WriteLine($"\nPriority: {priority}");
                Console.WriteLine($"Expanded query: {expandedQuery}");
                Console.WriteLine($"Minimum score: {options.MinimumScore}");
                
                var results = await _searchService.SearchAsync(query, options);
                
                foreach (var prediction in results.Predictions)
                {
                    var weightedScore = CalculateResultWeight(prediction);
                    Console.WriteLine($"- {prediction.Concept.Name}");
                    Console.WriteLine($"  Base Score: {prediction.Score:P2}");
                    Console.WriteLine($"  Weighted Score: {weightedScore:P2}");
                }
            }
        }
    }
}
```

Now we can test our configuration:

```csharp
public static async Task Main(string[] args)
{
    var hedgeSearch = new HedgePlatformSearch();
    await hedgeSearch.Initialize();
    await hedgeSearch.TestSearchConfigurations();
}
```

This completes Step 2 of our tutorial. We now have:
- ✅ Configured search parameters
- ✅ Implemented fuzzy matching for hedge terminology
- ✅ Added query preprocessing
- ✅ Implemented result weighting
- ✅ Created testing methods
