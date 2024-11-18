# Step 1: Creating Concepts and Documents 📚

## Overview
In this step, we'll create structured concepts and documents for each page of the Hedge Platform. We'll organize the content hierarchically and add metadata to improve search relevance.

## Detailed Steps

### 1.1 Set Up Initial Project Structure 🏗

First, let's create our project structure and initialize our services:

```csharp
using Microsoft.Extensions.DependencyInjection;
using RAGFramework;

public class HedgePlatformSearch
{
    private readonly ConceptStore _conceptStore;
    private readonly VectorStore _vectorStore;
    private readonly SearchService _searchService;

    public HedgePlatformSearch()
    {
        var services = new ServiceCollection()
            .AddRAGFramework()
            .AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            })
            .BuildServiceProvider();

        _conceptStore = services.GetRequiredService<ConceptStore>();
        _vectorStore = services.GetRequiredService<VectorStore>();
        _searchService = services.GetRequiredService<SearchService>();
    }
}
```

### 1.2 Define Concept Creation Method 🔨

Let's create a method to handle concept creation:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    private async Task CreateConcepts()
    {
        // Client Selection Concept
        var clientSelectConcept = new Concept
        {
            Id = "client-select",
            Name = "Client Selection Page",
            Description = "Documentation for client selection and management",
            Metadata = new Dictionary<string, string>
            {
                { "page_type", "selection" },
                { "user_role", "trader" },
                { "workflow_stage", "initial" }
            }
        };

        // Trade Management Concept
        var tradeManagementConcept = new Concept
        {
            Id = "trade-manage",
            Name = "Trade Management Page",
            Description = "Documentation for trade management operations",
            Metadata = new Dictionary<string, string>
            {
                { "page_type", "management" },
                { "user_role", "trader" },
                { "workflow_stage", "execution" }
            }
        };

        // Bid Request Concept
        var bidRequestConcept = new Concept
        {
            Id = "bid-request",
            Name = "Bid Request Page",
            Description = "Documentation for bid request processing",
            Metadata = new Dictionary<string, string>
            {
                { "page_type", "transaction" },
                { "user_role", "trader" },
                { "workflow_stage", "execution" }
            }
        };

        // Add concepts to store
        _conceptStore.AddConcept(clientSelectConcept);
        _conceptStore.AddConcept(tradeManagementConcept);
        _conceptStore.AddConcept(bidRequestConcept);
    }
}
```

### 1.3 Add Detailed Documents 📄

Now let's add detailed documentation for each concept:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    private async Task AddDocuments()
    {
        // Client Selection Documents
        var clientSelectDocs = new[]
        {
            new Document
            {
                Id = "client-select-overview",
                Content = """
                    The Client Selection page is the starting point for hedge operations.
                    Users can search for clients using various criteria including:
                    - Client name or ID lookup
                    - Portfolio type filtering
                    - Risk profile categorization
                    - Account status verification
                    
                    The page provides a comprehensive view of client information and
                    enables quick access to client portfolio details.
                    """,
                Metadata = new Dictionary<string, string>
                {
                    { "doc_type", "overview" },
                    { "importance", "high" }
                }
            },
            new Document
            {
                Id = "client-select-usage",
                Content = """
                    To select a client:
                    1. Enter client name or ID in the search bar
                    2. Use filters to narrow down results:
                       - Client Type (Individual/Institution)
                       - Portfolio Size
                       - Risk Level
                    3. Click on client row to view detailed information
                    4. Press 'Select Client' to proceed with hedge operations
                    
                    Common search tips:
                    - Use partial name matching
                    - Filter by account status
                    - Sort by portfolio size
                    """,
                Metadata = new Dictionary<string, string>
                {
                    { "doc_type", "guide" },
                    { "importance", "medium" }
                }
            }
        };

        // Trade Management Documents
        var tradeManagementDocs = new[]
        {
            new Document
            {
                Id = "trade-manage-overview",
                Content = """
                    The Trade Management page provides comprehensive tools for:
                    - Viewing active trades
                    - Monitoring trade status
                    - Executing trade operations
                    - Risk assessment
                    
                    Key features include real-time updates, position tracking,
                    and performance analytics.
                    """,
                Metadata = new Dictionary<string, string>
                {
                    { "doc_type", "overview" },
                    { "importance", "high" }
                }
            },
            new Document
            {
                Id = "trade-manage-operations",
                Content = """
                    Trade operations available:
                    1. View Position Details
                       - Current value
                       - P&L tracking
                       - Risk metrics
                    2. Execute Trade Actions
                       - Modify positions
                       - Close trades
                       - Roll positions
                    3. Generate Reports
                       - Performance summary
                       - Risk analysis
                       - Audit trails
                    """,
                Metadata = new Dictionary<string, string>
                {
                    { "doc_type", "guide" },
                    { "importance", "high" }
                }
            }
        };

        // Bid Request Documents
        var bidRequestDocs = new[]
        {
            new Document
            {
                Id = "bid-request-overview",
                Content = """
                    The Bid Request page enables traders to:
                    - Submit new bid requests
                    - Track bid status
                    - Manage bid responses
                    - Process acceptances/rejections
                    
                    The system provides real-time updates on bid status
                    and automated notification features.
                    """,
                Metadata = new Dictionary<string, string>
                {
                    { "doc_type", "overview" },
                    { "importance", "high" }
                }
            },
            new Document
            {
                Id = "bid-request-process",
                Content = """
                    Bid request process steps:
                    1. Create New Bid
                       - Select instruments
                       - Specify quantities
                       - Set price parameters
                    2. Submit Request
                       - Choose counterparties
                       - Set response deadlines
                       - Add special instructions
                    3. Manage Responses
                       - View incoming bids
                       - Compare offers
                       - Accept/reject bids
                    4. Finalize Transaction
                       - Confirm details
                       - Process documentation
                       - Update records
                    """,
                Metadata = new Dictionary<string, string>
                {
                    { "doc_type", "process" },
                    { "importance", "high" }
                }
            }
        };

        // Add documents to concepts
        foreach (var doc in clientSelectDocs)
        {
            _conceptStore.AddDocument("client-select", doc);
        }
        
        foreach (var doc in tradeManagementDocs)
        {
            _conceptStore.AddDocument("trade-manage", doc);
        }
        
        foreach (var doc in bidRequestDocs)
        {
            _conceptStore.AddDocument("bid-request", doc);
        }
    }
}
```

### 1.4 Create Initialization Method 🚀

Finally, let's create a method to tie everything together:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public async Task Initialize()
    {
        try
        {
            await CreateConcepts();
            await AddDocuments();
            await _vectorStore.BuildVectorStoreAsync();
            
            Console.WriteLine("✅ Successfully initialized Hedge Platform Search");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error initializing search: {ex.Message}");
            throw;
        }
    }

    // Main entry point
    public static async Task Main(string[] args)
    {
        var hedgeSearch = new HedgePlatformSearch();
        await hedgeSearch.Initialize();
    }
}
```

## Testing Our Setup 🧪

Let's add a simple test method to verify our setup:

```csharp
public class HedgePlatformSearch
{
    // ... previous code ...

    public async Task ValidateSetup()
    {
        var concepts = _conceptStore.GetAllConcepts().ToList();
        
        Console.WriteLine($"\nConcepts created: {concepts.Count}");
        foreach (var concept in concepts)
        {
            Console.WriteLine($"\nConcept: {concept.Name}");
            Console.WriteLine($"Documents: {concept.Documents.Count}");
            Console.WriteLine("Document Types:");
            foreach (var doc in concept.Documents)
            {
                Console.WriteLine($"- {doc.Id}: {doc.Metadata["doc_type"]}");
            }
        }
    }
}
```

## Running the Code 🏃‍♂️

```csharp
public static async Task Main(string[] args)
{
    var hedgeSearch = new HedgePlatformSearch();
    await hedgeSearch.Initialize();
    await hedgeSearch.ValidateSetup();
}
```

This completes Step 1 of our tutorial. We now have:
- ✅ Initialized our search infrastructure
- ✅ Created concepts for each page
- ✅ Added detailed documentation
- ✅ Included relevant metadata
- ✅ Built our vector store
