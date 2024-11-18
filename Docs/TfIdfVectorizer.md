# TF-IDF Vectorizer Documentation 📚

The TF-IDF (Term Frequency-Inverse Document Frequency) Vectorizer converts text documents into numerical vectors, enabling semantic comparison between documents. This component is built on top of ML.NET's text processing pipeline.

## Key Features 🎯

- Text normalization and tokenization
- TF-IDF vector generation
- Cosine similarity calculation
- Vocabulary management
- ML.NET integration

## Class Overview 🔍

```csharp
public class TfIdfVectorizer
{
    private readonly MLContext _mlContext;
    private ITransformer _transformer;
    private int _vocabSize;
}
```

## Core Methods

### Text Tokenization
```csharp
public IEnumerable<string> GetTokens(string text)
```
Breaks text into individual tokens for processing.

**Features:**
- Case normalization
- Punctuation handling
- Empty token filtering

**Example:**
```csharp
var vectorizer = new TfIdfVectorizer();
var tokens = vectorizer.GetTokens("Machine Learning is amazing!");
// Returns: ["machine", "learning", "is", "amazing"]
```

### Training the Vectorizer
```csharp
public void Fit(IEnumerable<string> documents)
```
Trains the TF-IDF model on a collection of documents.

**Pipeline Steps:**
1. Text normalization
2. Word tokenization
3. Stop word removal
4. TF-IDF calculation

**Example:**
```csharp
var documents = new[]
{
    "Machine learning is a subset of AI",
    "Deep learning uses neural networks",
    "AI powers modern applications"
};

vectorizer.Fit(documents);
```

### Vector Transformation
```csharp
public float[] Transform(string text)
```
Converts text into its TF-IDF vector representation.

**Example:**
```csharp
var vector = vectorizer.Transform("Machine learning algorithms");
// Returns: float array representing TF-IDF weights
```

### Similarity Calculation
```csharp
public double CalculateCosineSimilarity(float[] vector1, float[] vector2)
```
Calculates the cosine similarity between two vectors.

**Example:**
```csharp
var similarity = vectorizer.CalculateCosineSimilarity(
    vectorizer.Transform("AI and ML"),
    vectorizer.Transform("Artificial Intelligence")
);
```

## ML.NET Pipeline Details 🔄

### Pipeline Configuration
```csharp
var pipeline = _mlContext.Transforms.Text
    .NormalizeText("NormalizedText", "Text")
    .Append(_mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "NormalizedText"))
    .Append(_mlContext.Transforms.Text.RemoveStopWords("Tokens"))
    .Append(_mlContext.Transforms.Text.ProduceWordBags(
        "Features", 
        "Tokens",
        weighting: NgramExtractingEstimator.WeightingCriteria.TfIdf));
```

### Data Structures
```csharp
private class TextData
{
    public string Text { get; set; }
}

private class TransformedTextData
{
    public float[] Features { get; set; }
}
```

## Best Practices 💡

1. **Training Data Quality**
    - Use a representative sample of documents
    - Ensure sufficient vocabulary coverage
    - Consider domain-specific terminology

2. **Performance Optimization**
    - Reuse the trained vectorizer
    - Cache frequently used vectors
    - Consider batch processing for large documents

3. **Vector Comparisons**
    - Use cosine similarity for semantic comparison
    - Consider dimensionality of vectors
    - Handle edge cases (empty texts, single words)

## Example Usage Scenarios

### Document Comparison
```csharp
var vectorizer = new TfIdfVectorizer();

// Train on corpus
vectorizer.Fit(trainingDocuments);

// Compare documents
var doc1Vector = vectorizer.Transform("AI and machine learning");
var doc2Vector = vectorizer.Transform("Artificial intelligence systems");

var similarity = vectorizer.CalculateCosineSimilarity(doc1Vector, doc2Vector);
Console.WriteLine($"Similarity: {similarity:P2}");
```

### Batch Processing
```csharp
// Process multiple documents efficiently
var documents = new[]
{
    "First document about AI",
    "Second document about ML",
    "Third document about neural networks"
};

// Train once
vectorizer.Fit(documents);

// Transform all
var vectors = documents.Select(doc => vectorizer.Transform(doc)).ToList();
```

### Integration with Vector Store
```csharp
// Example of how TF-IDF vectorizer integrates with vector store
public class DocumentProcessor
{
    private readonly TfIdfVectorizer _vectorizer;
    
    public async Task ProcessDocument(string content)
    {
        // Get vector representation
        var vector = _vectorizer.Transform(content);
        
        // Store or compare as needed
        await StoreVector(vector);
    }
}
```

## Mathematical Background 📐

### TF-IDF Calculation
Term Frequency (TF):
- Measures how frequently a term occurs in a document
- `TF(t,d) = (Number of times term t appears in document d) / (Total number of terms in document d)`

Inverse Document Frequency (IDF):
- Measures how important a term is across all documents
- `IDF(t) = log(Total number of documents / Number of documents containing term t)`

Final TF-IDF:
- `TF-IDF(t,d) = TF(t,d) * IDF(t)`

### Cosine Similarity
```csharp
double dotProduct = 0;
double norm1 = 0;
double norm2 = 0;

for (int i = 0; i < vector1.Length; i++)
{
    dotProduct += vector1[i] * vector2[i];
    norm1 += vector1[i] * vector1[i];
    norm2 += vector2[i] * vector2[i];
}

return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
```