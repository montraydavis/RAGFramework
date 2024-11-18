# Dependency Injection Setup Documentation 📚

The ServiceCollectionExtensions class provides a clean, modular way to configure all RAG.NET services and their dependencies using Microsoft's dependency injection container.

## Core Extension Method 🔍

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRAGFramework(
        this IServiceCollection services)
    {
        // Register options directly
        services.Configure<SearchOptions>(options =>
        {
            options.MinimumScore = 0.1;
            options.MaxResults = 5;
            options.IncludeMetadata = true;
        });

        // Register fuzzy matching services
        services.Configure<FuzzyMatchOptions>(options =>
        {
            options.SimilarityThreshold = 0.8;
            options.MaxExpansionTerms = 3;
            options.EnableCache = true;
        });

        // Register core services
        services.AddSingleton<IFuzzyMatcher, LevenshteinMatcher>();
        services.AddSingleton<FuzzyMatchService>();
        services.AddSingleton<ConceptStore>();
        services.AddSingleton<VectorStore>();
        services.AddScoped<SearchService>();

        return services;
    }
}
```

## Usage Examples 💡

### Basic Setup
```csharp
var services = new ServiceCollection();

// Add framework with default configuration
services.AddRAGFramework();

// Build service provider
var serviceProvider = services.BuildServiceProvider();
```

### Custom Configuration
```csharp
services.AddRAGFramework()
        .Configure<SearchOptions>(options =>
        {
            options.MinimumScore = 0.2;
            options.MaxResults = 10;
        })
        .Configure<FuzzyMatchOptions>(options =>
        {
            options.SimilarityThreshold = 0.85;
            options.MaxExpansionTerms = 5;
        });
```

### With Logging
```csharp
services.AddRAGFramework()
        .AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
```

## Service Lifetimes 🔄

### Singleton Services
Long-lived services that maintain state:
```csharp
services.AddSingleton<ConceptStore>();      // Stores all concepts
services.AddSingleton<VectorStore>();       // Maintains vector database
services.AddSingleton<FuzzyMatchService>(); // Handles caching
```

### Scoped Services
Per-request services:
```csharp
services.AddScoped<SearchService>(); // New instance per scope
```

## Configuration Options 🎯

### Search Options
```csharp
services.Configure<SearchOptions>(options =>
{
    options.MinimumScore = 0.1;     // Minimum similarity threshold
    options.MaxResults = 5;          // Maximum results to return
    options.IncludeMetadata = true;  // Include metadata in results
});
```

### Fuzzy Match Options
```csharp
services.Configure<FuzzyMatchOptions>(options =>
{
    options.SimilarityThreshold = 0.8;  // Match threshold
    options.MaxExpansionTerms = 3;      // Max terms for expansion
    options.EnableCache = true;         // Enable result caching
});
```

## Advanced Configuration 🔧

### Custom Service Registration
```csharp
public static IServiceCollection AddRAGFrameworkWithCustomServices(
    this IServiceCollection services,
    Action<RAGOptions> configureOptions = null)
{
    // Add base services
    services.AddRAGFramework();

    // Configure custom options
    if (configureOptions != null)
    {
        services.Configure(configureOptions);
    }

    // Add custom services
    services.AddSingleton<ICustomService, CustomService>();
    services.AddScoped<ICustomProcessor, CustomProcessor>();

    return services;
}
```

### Environment-Based Configuration
```csharp
public static IServiceCollection AddRAGFrameworkForEnvironment(
    this IServiceCollection services,
    IHostEnvironment environment)
{
    services.AddRAGFramework();

    if (environment.IsDevelopment())
    {
        services.Configure<SearchOptions>(options =>
        {
            options.MinimumScore = 0.05;  // More lenient for development
            options.MaxResults = 10;       // More results for testing
        });
    }

    return services;
}
```

## Best Practices 💡

1. **Service Organization**
```csharp
// Group related services
public static IServiceCollection AddSearchServices(
    this IServiceCollection services)
{
    services.AddSingleton<VectorStore>();
    services.AddSingleton<FuzzyMatchService>();
    services.AddScoped<SearchService>();
    return services;
}

public static IServiceCollection AddStorageServices(
    this IServiceCollection services)
{
    services.AddSingleton<ConceptStore>();
    return services;
}
```

2. **Configuration Validation**
```csharp
public static IServiceCollection ValidateConfiguration(
    this IServiceCollection services)
{
    var sp = services.BuildServiceProvider();
    
    var searchOptions = sp.GetRequiredService<IOptions<SearchOptions>>();
    if (searchOptions.Value.MinimumScore < 0 || 
        searchOptions.Value.MinimumScore > 1)
    {
        throw new InvalidOperationException(
            "MinimumScore must be between 0 and 1");
    }

    return services;
}
```

3. **Dependency Management**
```csharp
// Ensure required services are registered
public static IServiceCollection ValidateServices(
    this IServiceCollection services)
{
    var requiredServices = new[]
    {
        typeof(IFuzzyMatcher),
        typeof(VectorStore),
        typeof(ConceptStore)
    };

    foreach (var serviceType in requiredServices)
    {
        if (!services.Any(d => d.ServiceType == serviceType))
        {
            throw new InvalidOperationException(
                $"Required service {serviceType.Name} is not registered");
        }
    }

    return services;
}
```

## Example Complete Setup 📋

```csharp
public static void ConfigureServices(
    IServiceCollection services, 
    IConfiguration configuration,
    IHostEnvironment environment)
{
    // Add framework with custom configuration
    services.AddRAGFramework()
            .Configure<SearchOptions>(
                configuration.GetSection("Search"))
            .Configure<FuzzyMatchOptions>(
                configuration.GetSection("FuzzyMatch"));

    // Add logging
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        
        if (environment.IsDevelopment())
        {
            builder.SetMinimumLevel(LogLevel.Debug);
        }
        else
        {
            builder.SetMinimumLevel(LogLevel.Information);
        }
    });

    // Validate configuration
    services.ValidateConfiguration()
            .ValidateServices();

    // Build provider
    var serviceProvider = services.BuildServiceProvider();

    // Initialize services
    var vectorStore = serviceProvider
        .GetRequiredService<VectorStore>();
    var conceptStore = serviceProvider
        .GetRequiredService<ConceptStore>();

    // Ready to use!
}
```