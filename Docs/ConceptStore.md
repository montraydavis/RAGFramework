# Concept Store Documentation 📚

The ConceptStore acts as a central repository for managing concepts and their associated documents. It provides an in-memory storage solution with thread-safe operations for concept management.

## Class Overview 🔍

```csharp
public class ConceptStore
{
    private readonly Dictionary<string, Concept> _concepts = new();
    private readonly TextPreprocessor _preprocessor = new();
}
```

## Key Features 🎯

- Thread-safe concept storage
- Document management within concepts
- Text preprocessing capabilities
- Efficient concept retrieval
- Validation checks

## Core Methods

### Adding Concepts
```csharp
public void AddConcept(Concept concept)
{
    if (string.IsNullOrEmpty(concept.Id))
        throw new ArgumentException("Concept must have an ID");

    _concepts[concept.Id] = concept;
}
```

### Adding Documents
```csharp
public void AddDocument(string conceptId, Document document)
{
    if (!_concepts.ContainsKey(conceptId))
        throw new KeyNotFoundException($"Concept {conceptId} not found");

    document.ConceptId = conceptId;
    _concepts[conceptId].Documents.Add(document);
}
```

### Retrieving Concepts
```csharp
public IEnumerable<Concept> GetAllConcepts()
{
    return _concepts.Values;
}

public Concept GetConcept(string conceptId)
{
    return _concepts[conceptId];
}
```

## Usage Examples 💡

### Basic Concept Management
```csharp
var store = new ConceptStore();

// Create and add a concept
var mlConcept = new Concept
{
    Id = "ml-001",
    Name = "Machine Learning",
    Description = "Core ML concepts"
};

store.AddConcept(mlConcept);

// Add a document to the concept
var document = new Document
{
    Id = "doc-001",
    Content = "Machine learning is a subset of artificial intelligence...",
    CreatedAt = DateTime.UtcNow
};

store.AddDocument("ml-001", document);
```

### Working with Metadata
```csharp
// Create a concept with metadata
var deepLearningConcept = new Concept
{
    Id = "dl-001",
    Name = "Deep Learning",
    Description = "Neural network architectures",
    Metadata = new Dictionary<string, string>
    {
        { "field", "artificial-intelligence" },
        { "difficulty", "advanced" },
        { "prerequisites", "ml-001" }
    }
};

store.AddConcept(deepLearningConcept);

// Add document with metadata
var advancedDoc = new Document
{
    Id = "doc-002",
    Content = "Deep neural networks consist of multiple layers...",
    Metadata = new Dictionary<string, string>
    {
        { "author", "Dr. Smith" },
        { "last_updated", DateTime.UtcNow.ToString("O") },
        { "version", "1.0" }
    }
};

store.AddDocument("dl-001", advancedDoc);
```

### Bulk Operations
```csharp
public void BulkAddConcepts(IEnumerable<Concept> concepts)
{
    foreach (var concept in concepts)
    {
        if (string.IsNullOrEmpty(concept.Id))
            throw new ArgumentException(
                $"Concept {concept.Name} must have an ID");
            
        AddConcept(concept);
    }
}

// Usage
var concepts = new[]
{
    new Concept { Id = "c1", Name = "Concept 1" },
    new Concept { Id = "c2", Name = "Concept 2" },
    new Concept { Id = "c3", Name = "Concept 3" }
};

store.BulkAddConcepts(concepts);
```

## Integration Patterns 🔄

### With Vector Store
```csharp
public async Task BuildVectorStore(
    ConceptStore conceptStore,
    VectorStore vectorStore)
{
    // Get all concepts
    var concepts = conceptStore.GetAllConcepts();
    
    // Process documents for vectorization
    foreach (var concept in concepts)
    {
        foreach (var doc in concept.Documents)
        {
            // Preprocess and vectorize
            await ProcessDocument(doc);
        }
    }
}
```

### With Search Service
```csharp
public async Task<IEnumerable<Document>> SearchDocuments(
    string query,
    ConceptStore store,
    SearchService searchService)
{
    var results = await searchService.SearchAsync(query);
    
    return results.Predictions
        .SelectMany(p => store.GetConcept(p.ConceptId).Documents)
        .OrderByDescending(d => d.CreatedAt);
}
```

## Best Practices 💡

1. **Concept Organization**
   ```csharp
   // Group related concepts
   var parentConcept = new Concept
   {
       Id = "parent",
       Name = "Parent Concept"
   };

   var childConcept = new Concept
   {
       Id = "child",
       Name = "Child Concept",
       Metadata = new Dictionary<string, string>
       {
           { "parent_id", "parent" }
       }
   };
   ```

2. **Document Management**
   ```csharp
   // Version control for documents
   var document = new Document
   {
       Id = Guid.NewGuid().ToString(),
       Content = "Updated content...",
       Metadata = new Dictionary<string, string>
       {
           { "version", "2.0" },
           { "previous_version", "1.0" },
           { "updated_at", DateTime.UtcNow.ToString("O") }
       }
   };
   ```

3. **Error Handling**
   ```csharp
   public void SafeAddDocument(string conceptId, Document document)
   {
       try
       {
           if (!_concepts.ContainsKey(conceptId))
               throw new KeyNotFoundException(
                   $"Concept {conceptId} not found");

           document.ConceptId = conceptId;
           _concepts[conceptId].Documents.Add(document);
       }
       catch (Exception ex)
       {
           // Log error and handle appropriately
           throw new ConceptStoreException(
               $"Failed to add document to concept {conceptId}", 
               ex);
       }
   }
   ```

## Performance Considerations 📊

1. **Memory Usage**
    - Monitor the size of your concept store
    - Consider implementing document pagination
    - Use lazy loading for large document contents

2. **Concurrent Access**
   ```csharp
   private readonly ConcurrentDictionary<string, Concept> _concepts = 
       new ConcurrentDictionary<string, Concept>();
   ```

3. **Batch Operations**
   ```csharp
   public void BulkAddDocuments(
       string conceptId, 
       IEnumerable<Document> documents)
   {
       if (!_concepts.ContainsKey(conceptId))
           throw new KeyNotFoundException();

       var concept = _concepts[conceptId];
       foreach (var doc in documents)
       {
           doc.ConceptId = conceptId;
       }
       
       concept.Documents.AddRange(documents);
   }
   ```

## Extension Points 🔌

1. **Custom Validators**
   ```csharp
   public interface IConceptValidator
   {
       bool ValidateConcept(Concept concept);
       bool ValidateDocument(Document document);
   }
   ```

2. **Event Handlers**
   ```csharp
   public delegate void ConceptAddedEventHandler(
       object sender, 
       ConceptEventArgs e);
   
   public event ConceptAddedEventHandler ConceptAdded;
   ```

3. **Custom Storage Providers**
   ```csharp
   public interface IConceptStorage
   {
       void StoreConcept(Concept concept);
       Concept RetrieveConcept(string conceptId);
       void StoreDocument(string conceptId, Document document);
   }
   ```