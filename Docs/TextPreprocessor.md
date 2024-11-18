# Text Preprocessor Documentation 📚

The TextPreprocessor provides essential text normalization and tokenization capabilities for the RAG.NET framework. It ensures consistent text processing across all components of the system.

## Class Overview 🔍

```csharp
public class TextPreprocessor
{
    public string PreprocessText(string text)
    public IEnumerable<string> TokenizeText(string text)
}
```

## Key Features 🎯

- Text normalization
- Whitespace handling
- Punctuation removal
- Case normalization
- Token extraction
- Empty text handling

## Core Methods

### Text Preprocessing
```csharp
public string PreprocessText(string text)
{
    if (string.IsNullOrEmpty(text))
        return string.Empty;

    // Basic preprocessing steps
    return text.ToLower()
              .Replace("\n", " ")
              .Replace("\r", " ")
              .Trim();
}
```

### Text Tokenization
```csharp
public IEnumerable<string> TokenizeText(string text)
{
    // Basic word tokenization
    return text.Split(
        new[] { ' ', '\t', ',', '.', '!', '?' }, 
        StringSplitOptions.RemoveEmptyEntries
    );
}
```

## Usage Examples 💡

### Basic Text Processing
```csharp
var preprocessor = new TextPreprocessor();

// Normalize text
var text = "Machine Learning\nis AMAZING!";
var normalized = preprocessor.PreprocessText(text);
// Result: "machine learning is amazing!"

// Tokenize text
var tokens = preprocessor.TokenizeText(normalized);
// Result: ["machine", "learning", "is", "amazing"]
```

### Integration with Document Processing
```csharp
public class DocumentProcessor
{
    private readonly TextPreprocessor _preprocessor;

    public Document ProcessDocument(string content)
    {
        var normalized = _preprocessor.PreprocessText(content);
        var tokens = _preprocessor.TokenizeText(normalized);

        return new Document
        {
            Content = normalized,
            Metadata = new Dictionary<string, string>
            {
                { "token_count", tokens.Count().ToString() },
                { "processed_at", DateTime.UtcNow.ToString("O") }
            }
        };
    }
}
```

### Batch Processing
```csharp
public class BatchProcessor
{
    private readonly TextPreprocessor _preprocessor;

    public IEnumerable<string> ProcessBatch(IEnumerable<string> texts)
    {
        return texts
            .AsParallel()
            .Select(text => _preprocessor.PreprocessText(text))
            .Where(text => !string.IsNullOrEmpty(text));
    }
}
```

## Advanced Usage 🔄

### Custom Token Filters
```csharp
public class EnhancedPreprocessor : TextPreprocessor
{
    private readonly HashSet<string> _stopWords;

    public EnhancedPreprocessor(IEnumerable<string> stopWords)
    {
        _stopWords = new HashSet<string>(
            stopWords.Select(w => w.ToLower())
        );
    }

    public IEnumerable<string> TokenizeWithFilters(string text)
    {
        return TokenizeText(PreprocessText(text))
            .Where(token => !_stopWords.Contains(token))
            .Where(token => token.Length >= 2);
    }
}
```

### Language-Specific Processing
```csharp
public class LanguageAwarePreprocessor : TextPreprocessor
{
    private readonly string _language;

    public LanguageAwarePreprocessor(string language)
    {
        _language = language;
    }

    public override string PreprocessText(string text)
    {
        var normalized = base.PreprocessText(text);
        
        // Apply language-specific rules
        switch (_language.ToLower())
        {
            case "en":
                return HandleEnglish(normalized);
            case "es":
                return HandleSpanish(normalized);
            default:
                return normalized;
        }
    }
}
```

## Best Practices 💡

1. **Input Validation**
```csharp
public string SafePreprocess(string text)
{
    if (string.IsNullOrEmpty(text))
        return string.Empty;

    if (text.Length > MaxTextLength)
        throw new ArgumentException(
            $"Text exceeds maximum length of {MaxTextLength}");

    return PreprocessText(text);
}
```

2. **Performance Optimization**
```csharp
public class CachingPreprocessor : TextPreprocessor
{
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public override string PreprocessText(string text)
    {
        return _cache.GetOrAdd(
            text, 
            key => base.PreprocessText(key)
        );
    }
}
```

3. **Error Handling**
```csharp
public IEnumerable<string> SafeTokenize(string text)
{
    try
    {
        return TokenizeText(PreprocessText(text));
    }
    catch (Exception ex)
    {
        // Log error
        return Enumerable.Empty<string>();
    }
}
```

## Integration Patterns 🔌

### With Vector Store
```csharp
public class VectorProcessor
{
    private readonly TextPreprocessor _preprocessor;
    
    public async Task ProcessForVectorization(string text)
    {
        // Preprocess text
        var normalized = _preprocessor.PreprocessText(text);
        var tokens = _preprocessor.TokenizeText(normalized);
        
        // Ready for vectorization
        await VectorizeTokens(tokens);
    }
}
```

### With Search Service
```csharp
public class SearchQueryProcessor
{
    private readonly TextPreprocessor _preprocessor;
    
    public string PrepareSearchQuery(string query)
    {
        var normalized = _preprocessor.PreprocessText(query);
        var tokens = _preprocessor.TokenizeText(normalized);
        
        return string.Join(" ", tokens);
    }
}
```

## Performance Tips 📊

1. **Batch Processing**
```csharp
public IEnumerable<string> ProcessBatch(
    IEnumerable<string> texts,
    int batchSize = 1000)
{
    return texts
        .AsParallel()
        .Select(text => PreprocessText(text))
        .Where(text => !string.IsNullOrEmpty(text));
}
```

2. **Memory Optimization**
```csharp
public IEnumerable<string> TokenizeStream(
    IEnumerable<string> textStream)
{
    foreach (var text in textStream)
    {
        foreach (var token in TokenizeText(PreprocessText(text)))
        {
            yield return token;
        }
    }
}
```

3. **Thread Safety**
```csharp
public class ThreadSafePreprocessor
{
    private readonly SemaphoreSlim _semaphore = 
        new SemaphoreSlim(1, 1);
    private readonly TextPreprocessor _preprocessor = new();

    public async Task<string> PreprocessAsync(string text)
    {
        await _semaphore.WaitAsync();
        try
        {
            return _preprocessor.PreprocessText(text);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```