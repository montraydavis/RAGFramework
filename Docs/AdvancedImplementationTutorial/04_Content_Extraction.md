# Step 4: Content Extraction and Processing 📑

## Overview
In this step, we'll implement sophisticated content extraction and processing methods to make our search results more useful and actionable. We'll focus on contextual relevance, snippet generation, and content organization.

## Detailed Steps

### 4.1 Create Content Extraction Classes 🎯

First, let's define our content extraction structures:

```csharp
public class HedgePlatformSearch
{
    // ... previous code from Steps 1-3 ...

    public class ExtractedContent
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public List<string> Snippets { get; set; } = new();
        public List<string> Keywords { get; set; } = new();
        public Dictionary<string, string> RelatedConcepts { get; set; } = new();
        public List<string> Actions { get; set; } = new();
        public ContentMetadata Metadata { get; set; } = new();
    }

    public class ContentMetadata
    {
        public string Source { get; set; }
        public string Type { get; set; }
        public DateTime LastUpdated { get; set; }
        public double Relevance { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class SnippetOptions
    {
        public int MaxLength { get; set; } = 200;
        public int Context { get; set; } = 50;
        public bool HighlightMatches { get; set; } = true;
        public int MaxSnippets { get; set; } = 3;
    }
}
```

### 4.2 Implement Content Extraction Methods 🔨

Add methods to extract and process content from search results:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    private readonly List<string> _actionVerbs = new()
    {
        "select", "choose", "click", "enter", "view",
        "submit", "create", "modify", "update", "delete"
    };

    public async Task<ExtractedContent> ExtractContent(
        SearchHit hit,
        SnippetOptions options = null)
    {
        options ??= new SnippetOptions();

        var content = new ExtractedContent
        {
            Title = hit.ConceptName,
            Summary = GenerateSummary(hit.Content),
            Snippets = GenerateSnippets(
                hit.Content,
                hit.MatchedTerms,
                options),
            Keywords = ExtractKeywords(hit.Content),
            RelatedConcepts = FindRelatedConcepts(hit),
            Actions = ExtractActions(hit.Content),
            Metadata = new ContentMetadata
            {
                Source = hit.DocumentId,
                Type = hit.Metadata.GetValueOrDefault("doc_type", "general"),
                LastUpdated = DateTime.Parse(
                    hit.Metadata.GetValueOrDefault("last_updated", 
                    DateTime.UtcNow.ToString())),
                Relevance = hit.WeightedScore,
                Category = hit.ConceptName,
                Tags = hit.Metadata
                    .Where(m => m.Key.StartsWith("tag_"))
                    .Select(m => m.Value)
                    .ToList()
            }
        };

        return content;
    }

    private string GenerateSummary(string content)
    {
        // Get first paragraph or up to 200 characters
        var firstParagraph = content.Split("\n")
            .First(p => !string.IsNullOrWhiteSpace(p));

        return firstParagraph.Length > 200
            ? firstParagraph[..197] + "..."
            : firstParagraph;
    }

    private List<string> GenerateSnippets(
        string content,
        List<string> matchedTerms,
        SnippetOptions options)
    {
        var snippets = new List<string>();
        var sentences = content.Split(
            new[] { '.', '!', '?' },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var term in matchedTerms)
        {
            var relevantSentences = sentences
                .Where(s => s.Contains(term, 
                    StringComparison.OrdinalIgnoreCase))
                .Take(options.MaxSnippets);

            foreach (var sentence in relevantSentences)
            {
                var snippet = sentence.Trim();
                
                if (options.HighlightMatches)
                {
                    snippet = HighlightTerm(snippet, term);
                }

                if (snippet.Length > options.MaxLength)
                {
                    snippet = snippet[..(options.MaxLength - 3)] + "...";
                }

                snippets.Add(snippet);
            }
        }

        return snippets.Distinct().ToList();
    }

    private string HighlightTerm(string text, string term)
    {
        return text.Replace(
            term,
            $"**{term}**",
            StringComparison.OrdinalIgnoreCase);
    }

    private List<string> ExtractKeywords(string content)
    {
        // Simple keyword extraction based on frequency
        return content.Split(' ')
            .Where(word => word.Length > 3)
            .GroupBy(word => word.ToLower())
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();
    }

    private Dictionary<string, string> FindRelatedConcepts(SearchHit hit)
    {
        var related = new Dictionary<string, string>();
        var allConcepts = _conceptStore.GetAllConcepts();

        foreach (var concept in allConcepts)
        {
            if (concept.Id != hit.ConceptId)
            {
                // Check for content similarity
                var similarity = CalculateConceptSimilarity(
                    hit.Content,
                    concept.Documents.First().Content);

                if (similarity > 0.3)  // Threshold for relation
                {
                    related[concept.Id] = concept.Name;
                }
            }
        }

        return related;
    }

    private double CalculateConceptSimilarity(string content1, string content2)
    {
        // Simple word overlap similarity
        var words1 = new HashSet<string>(
            content1.ToLower().Split(' '));
        var words2 = new HashSet<string>(
            content2.ToLower().Split(' '));

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return (double)intersection / union;
    }

    private List<string> ExtractActions(string content)
    {
        var actions = new List<string>();
        var sentences = content.Split('.');

        foreach (var sentence in sentences)
        {
            foreach (var verb in _actionVerbs)
            {
                if (sentence.Contains(verb, 
                    StringComparison.OrdinalIgnoreCase))
                {
                    actions.Add(sentence.Trim());
                    break;
                }
            }
        }

        return actions.Distinct().ToList();
    }
}
```

### 4.3 Add Content Processing Methods 🔄

Implement methods to process and organize extracted content:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public class ProcessedContent
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public List<ContentSection> Sections { get; set; } = new();
        public List<string> ActionItems { get; set; } = new();
        public Dictionary<string, string> RelatedTopics { get; set; } = new();
    }

    public class ContentSection
    {
        public string Heading { get; set; }
        public string Content { get; set; }
        public double Relevance { get; set; }
    }

    public ProcessedContent ProcessExtractedContent(
        ExtractedContent content,
        ProcessingOptions options = null)
    {
        options ??= new ProcessingOptions();

        var processed = new ProcessedContent
        {
            Title = content.Title,
            Summary = content.Summary,
            Sections = OrganizeSections(content, options),
            ActionItems = OrganizeActions(content.Actions),
            RelatedTopics = OrganizeRelatedTopics(content.RelatedConcepts)
        };

        return processed;
    }

    private List<ContentSection> OrganizeSections(
        ExtractedContent content,
        ProcessingOptions options)
    {
        var sections = new List<ContentSection>();

        // Add snippet section
        if (content.Snippets.Any())
        {
            sections.Add(new ContentSection
            {
                Heading = "Relevant Excerpts",
                Content = string.Join("\n\n", content.Snippets),
                Relevance = 1.0
            });
        }

        // Add keyword section
        if (content.Keywords.Any())
        {
            sections.Add(new ContentSection
            {
                Heading = "Key Terms",
                Content = string.Join(", ", content.Keywords),
                Relevance = 0.8
            });
        }

        return sections;
    }

    private List<string> OrganizeActions(List<string> actions)
    {
        return actions
            .Select(action => action.Trim())
            .Where(action => !string.IsNullOrEmpty(action))
            .Distinct()
            .OrderBy(action => action)
            .ToList();
    }

    private Dictionary<string, string> OrganizeRelatedTopics(
        Dictionary<string, string> relatedConcepts)
    {
        return relatedConcepts
            .OrderBy(rc => rc.Value)
            .ToDictionary(
                rc => rc.Key,
                rc => rc.Value);
    }
}
```

### 4.4 Implement Result Formatting 📝

Add methods to format processed content for display:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public string FormatProcessedContent(ProcessedContent content)
    {
        var sb = new StringBuilder();

        // Title and Summary
        sb.AppendLine($"# {content.Title}");
        sb.AppendLine();
        sb.AppendLine(content.Summary);
        sb.AppendLine();

        // Sections
        foreach (var section in content.Sections
            .OrderByDescending(s => s.Relevance))
        {
            sb.AppendLine($"## {section.Heading}");
            sb.AppendLine();
            sb.AppendLine(section.Content);
            sb.AppendLine();
        }

        // Action Items
        if (content.ActionItems.Any())
        {
            sb.AppendLine("## Action Items");
            sb.AppendLine();
            foreach (var action in content.ActionItems)
            {
                sb.AppendLine($"- {action}");
            }
            sb.AppendLine();
        }

        // Related Topics
        if (content.RelatedTopics.Any())
        {
            sb.AppendLine("## Related Topics");
            sb.AppendLine();
            foreach (var topic in content.RelatedTopics)
            {
                sb.AppendLine($"- {topic.Value}");
            }
        }

        return sb.ToString();
    }
}
```

### 4.5 Create Complete Example 🚀

Let's put it all together with a complete example:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public async Task RunContentExtractionExample()
    {
        try
        {
            // Perform search
            var searchResult = await Search(
                "how to select client and view portfolio",
                SearchPriority.HighPrecision);

            Console.WriteLine("\n🔍 Processing Search Results\n");

            // Process each hit
            foreach (var hit in searchResult.Hits)
            {
                Console.WriteLine($"Processing: {hit.ConceptName}\n");

                // Extract content
                var extracted = await ExtractContent(
                    hit,
                    new SnippetOptions
                    {
                        MaxLength = 150,
                        Context = 30,
                        HighlightMatches = true,
                        MaxSnippets = 2
                    });

                // Process content
                var processed = ProcessExtractedContent(extracted);

                // Format and display
                var formatted = FormatProcessedContent(processed);
                Console.WriteLine(formatted);
                Console.WriteLine(
                    new string('-', 80) + "\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ Error processing content: {ex.Message}");
        }
    }

    // Update Main method
    public static async Task Main(string[] args)
    {
        var hedgeSearch = new HedgePlatformSearch();
        
        try
        {
            await hedgeSearch.Initialize();
            await hedgeSearch.RunContentExtractionExample();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
```

This completes Step 4 of our tutorial. We now have:
- ✅ Implemented content extraction
- ✅ Added snippet generation
- ✅ Created keyword extraction
- ✅ Added action item identification
- ✅ Implemented related topic finding
- ✅ Created content formatting
- ✅ Added complete working examples

The system can now:
1. Extract relevant content from search results
2. Generate meaningful snippets
3. Identify key terms and actions
4. Find related topics
5. Format content for display
