# Data Structure Documentation 📚

## Concept
```csharp
public class Concept
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Document> Documents { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
```
Primary container for organizing related documents. Includes metadata support for flexible tagging and categorization.

## Document
```csharp
public class Document
{
    public string Id { get; set; }
    public string ConceptId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
```
Represents individual documents within a concept. Contains the actual content and supports metadata for document-level attributes.

## ConceptTrainingData
```csharp
public class ConceptTrainingData
{
    [VectorType(1)]
    public float[] Features { get; set; }
    
    [VectorType(1)]
    public float[] Labels { get; set; }
}
```
ML.NET training data structure for concept classification. Used internally by the TF-IDF vectorizer.

## ConceptPredictionInput
```csharp
public class ConceptPredictionInput
{
    public string Text { get; set; }
}
```
Input structure for prediction operations. Contains the text to be classified.

## ConceptPredictionOutput
```csharp
public class ConceptPredictionOutput
{
    public float[] Scores { get; set; }
}
```
Output structure for prediction operations. Contains classification scores.

## ConceptPrediction
```csharp
public class ConceptPrediction
{
    public string ConceptId { get; set; }
    public double Score { get; set; }
    public Concept Concept { get; set; }
}
```
Represents a prediction result with the matched concept and its confidence score.

## PredictionResult
```csharp
public class PredictionResult
{
    public string Query { get; set; }
    public string ExpandedQuery { get; set; }
    public List<ConceptPrediction> Predictions { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}
```
Contains the complete results of a search operation, including the original query, expanded query, and all predictions.

## SearchOptions
```csharp
public class SearchOptions
{
    public double MinimumScore { get; set; } = 0.1;
    public int MaxResults { get; set; } = 5;
    public bool IncludeMetadata { get; set; } = true;
}
```
Configuration options for search operations. Controls filtering and result limits.

## TextData
```csharp
public class TextData
{
    public string Text { get; set; }
}
```
Internal structure used by the TF-IDF vectorizer for text processing.

## TransformedTextData
```csharp
public class TransformedTextData
{
    public float[] Features { get; set; }
}
```
Internal structure used by the TF-IDF vectorizer to store transformed text features.

## FuzzyMatchOptions
```csharp
public class FuzzyMatchOptions
{
    public double SimilarityThreshold { get; set; } = 0.8;
    public int MaxExpansionTerms { get; set; } = 3;
    public bool EnableCache { get; set; } = true;
}
```
Configuration options for the fuzzy matching system. Controls matching sensitivity and caching behavior.