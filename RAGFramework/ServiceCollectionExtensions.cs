using Microsoft.Extensions.DependencyInjection;
using RAGFramework.Algos;

namespace RAGFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRAGFramework(this IServiceCollection services)
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
        services.AddSingleton<IFuzzyMatcher, LevenshteinMatcher>();
        services.AddSingleton<FuzzyMatchService>();

        // Register core services
        services.AddSingleton<ConceptStore>();
        services.AddSingleton<VectorStore>();
        services.AddScoped<SearchService>();

        return services;
    }
}
