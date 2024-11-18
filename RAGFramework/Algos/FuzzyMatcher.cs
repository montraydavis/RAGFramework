using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RAGFramework.Algos
{
    // Interface for different fuzzy matching algorithms
    public interface IFuzzyMatcher
    {
        double CalculateSimilarity(string source, string target);
        bool IsMatch(string source, string target, double threshold);
    }

    // Levenshtein distance implementation
    public class LevenshteinMatcher : IFuzzyMatcher
    {
        public double CalculateSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0;
            
            var distance = CalculateLevenshteinDistance(source, target);
            var maxLength = Math.Max(source.Length, target.Length);
            
            return 1 - ((double)distance / maxLength);
        }

        public bool IsMatch(string source, string target, double threshold)
        {
            return CalculateSimilarity(source, target) >= threshold;
        }

        private int CalculateLevenshteinDistance(string source, string target)
        {
            var n = source.Length;
            var m = target.Length;
            var d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            // Initialize first row and column
            for (var i = 0; i <= n; i++) d[i, 0] = i;
            for (var j = 0; j <= m; j++) d[0, j] = j;

            // Calculate minimum edit distance
            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            return d[n, m];
        }
    }

    // Configuration for fuzzy matching
    public class FuzzyMatchOptions
    {
        public double SimilarityThreshold { get; set; } = 0.8;
        public int MaxExpansionTerms { get; set; } = 3;
        public bool EnableCache { get; set; } = true;
    }

    // Service to handle fuzzy matching logic
    public class FuzzyMatchService
    {
        private readonly IFuzzyMatcher _matcher;
        private readonly FuzzyMatchOptions _options;
        private readonly ILogger<FuzzyMatchService> _logger;
        private readonly ConcurrentDictionary<string, HashSet<string>> _cache;

        public FuzzyMatchService(
            IFuzzyMatcher matcher,
            IOptions<FuzzyMatchOptions> options,
            ILogger<FuzzyMatchService> logger)
        {
            _matcher = matcher;
            _options = options.Value;
            _logger = logger;
            _cache = new ConcurrentDictionary<string, HashSet<string>>();
        }

        public IEnumerable<string> ExpandTerms(string term, IEnumerable<string> vocabulary)
        {
            if (string.IsNullOrWhiteSpace(term)) return Enumerable.Empty<string>();

            // Check cache if enabled
            if (_options.EnableCache && _cache.TryGetValue(term, out var cachedExpansions))
            {
                return cachedExpansions;
            }

            var expansions = new HashSet<string> { term };
            
            try
            {
                var matches = vocabulary
                    .Where(v => _matcher.IsMatch(term, v, _options.SimilarityThreshold))
                    .OrderByDescending(v => _matcher.CalculateSimilarity(term, v))
                    .Take(_options.MaxExpansionTerms);

                expansions.UnionWith(matches);

                // Cache results if enabled
                if (_options.EnableCache)
                {
                    _cache.TryAdd(term, expansions);
                }

                _logger.LogDebug(
                    "Expanded term '{Term}' to {Count} variations", 
                    term, 
                    expansions.Count);

                return expansions;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error expanding term '{Term}'",
                    term);
                return expansions;
            }
        }

        public void ClearCache()
        {
            _cache.Clear();
        }
    }

    // Extension method for service registration
    public static class FuzzyMatchingExtensions
    {
        public static IServiceCollection AddFuzzyMatching(
            this IServiceCollection services,
            Action<FuzzyMatchOptions> configureOptions = null)
        {
            // Register default options
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            else
            {
                services.Configure<FuzzyMatchOptions>(options => {});
            }

            // Register services
            services.AddSingleton<IFuzzyMatcher, LevenshteinMatcher>();
            services.AddSingleton<FuzzyMatchService>();

            return services;
        }
    }
}