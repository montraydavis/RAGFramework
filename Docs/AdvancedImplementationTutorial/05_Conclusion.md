# RAG.NET Implementation Summary and Usage Example 🚀

## Tutorial Summary

1. **Concept Creation** ✅
    - Structured documentation into concepts
    - Added metadata and relationships
    - Organized documents within concepts

2. **Search Configuration** ✅
    - Configured thresholds and weights
    - Set up fuzzy matching
    - Implemented query preprocessing

3. **Search Implementation** ✅
    - Created search service wrapper
    - Added specialized search methods
    - Implemented result formatting

4. **Content Extraction** ✅
    - Implemented snippet generation
    - Added keyword extraction
    - Created content processing

## Complete Usage Example 🔥

Here's a comprehensive example showing how to use all the features together:

```csharp
public class HedgePlatformExample
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Initializing Hedge Platform Search System\n");
        
        var hedgeSearch = new HedgePlatformSearch();
        await hedgeSearch.Initialize();

        try
        {
            // Example 1: Quick Search for Client Selection
            Console.WriteLine("📋 Example 1: Quick Client Search\n");
            var quickResult = await hedgeSearch.QuickSearch("select new client");
            
            foreach (var hit in quickResult.Hits)
            {
                var content = await hedgeSearch.ExtractContent(hit, new SnippetOptions
                {
                    MaxLength = 150,
                    HighlightMatches = true,
                    MaxSnippets = 2
                });

                var processed = hedgeSearch.ProcessExtractedContent(content);
                Console.WriteLine(hedgeSearch.FormatProcessedContent(processed));
            }

            // Example 2: Detailed Trade Operation Search
            Console.WriteLine("\n📊 Example 2: Trade Operation Search\n");
            var tradeResult = await hedgeSearch.SearchTradeOperations(
                "modify existing position");

            // Process first hit in detail
            if (tradeResult.Hits.Any())
            {
                var mainHit = tradeResult.Hits.First();
                var content = await hedgeSearch.ExtractContent(mainHit);
                var processed = hedgeSearch.ProcessExtractedContent(content);

                Console.WriteLine("Most Relevant Result:");
                Console.WriteLine(hedgeSearch.FormatProcessedContent(processed));

                // Show related topics
                if (processed.RelatedTopics.Any())
                {
                    Console.WriteLine("\nYou might also be interested in:");
                    foreach (var topic in processed.RelatedTopics)
                    {
                        Console.WriteLine($"- {topic.Value}");
                    }
                }
            }

            // Example 3: Multi-step Operation
            Console.WriteLine("\n🔄 Example 3: Multi-step Operation\n");
            var multiStepSearch = await hedgeSearch.Search(
                "submit bid request for client portfolio",
                SearchPriority.HighPrecision);

            var stepByStepGuide = new StringBuilder();
            stepByStepGuide.AppendLine("Complete Process Guide:\n");

            foreach (var hit in multiStepSearch.Hits)
            {
                var content = await hedgeSearch.ExtractContent(hit);
                var processed = hedgeSearch.ProcessExtractedContent(content);

                stepByStepGuide.AppendLine($"Step: {processed.Title}");
                if (processed.ActionItems.Any())
                {
                    foreach (var action in processed.ActionItems)
                    {
                        stepByStepGuide.AppendLine($"- {action}");
                    }
                }
                stepByStepGuide.AppendLine();
            }

            Console.WriteLine(stepByStepGuide.ToString());

            // Example 4: Interactive Search Session
            Console.WriteLine("\n💡 Example 4: Interactive Search Session\n");
            await RunInteractiveSession(hedgeSearch);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    private static async Task RunInteractiveSession(HedgePlatformSearch hedgeSearch)
    {
        var queries = new[]
        {
            "how do I start with a new client?",
            "what are the steps to submit a bid?",
            "where can I view client portfolio?"
        };

        foreach (var query in queries)
        {
            Console.WriteLine($"\nQuery: {query}");
            Console.WriteLine("Processing...\n");

            var result = await hedgeSearch.Search(
                query, 
                SearchPriority.HighRecall);

            foreach (var hit in result.Hits.Take(1))  // Show top result
            {
                var content = await hedgeSearch.ExtractContent(hit);
                var processed = hedgeSearch.ProcessExtractedContent(content);

                Console.WriteLine("Top Result:");
                Console.WriteLine(hedgeSearch.FormatProcessedContent(processed));

                // Show quick actions if available
                if (processed.ActionItems.Any())
                {
                    Console.WriteLine("\nQuick Actions:");
                    foreach (var action in processed.ActionItems.Take(3))
                    {
                        Console.WriteLine($"→ {action}");
                    }
                }
            }

            Console.WriteLine("\nPress any key for next query...");
            Console.ReadKey();
        }
    }
}
```

## Example Usage Output 📝

When you run this example, you'll see output like:

```
🚀 Initializing Hedge Platform Search System

📋 Example 1: Quick Client Search
# Client Selection Page

Learn how to select and manage clients in the platform.

## Relevant Excerpts
- Enter client name or ID in the **search** bar
- Use filters to narrow down results by client type
- Click on client row to view detailed information

## Action Items
- Enter client name or ID
- Select client type filter
- Click 'Select Client' button
...

[Additional examples and interactive session output]
```

This example demonstrates:
- Different search types and priorities
- Content extraction and processing
- Result formatting and presentation
- Interactive search capabilities
- Action item extraction
- Related topic discovery
