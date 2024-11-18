using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RAGFramework
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);  // Set to Debug to see query expansions
            });

            services.AddRAGFramework();

            var serviceProvider = services.BuildServiceProvider();

            var searchService = serviceProvider.GetRequiredService<SearchService>();
            var conceptStore = serviceProvider.GetRequiredService<ConceptStore>();
            var vectorStore = serviceProvider.GetRequiredService<VectorStore>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Add test concepts with intentional variations
                AddTestConcepts(conceptStore);
                logger.LogInformation("Added test concepts");

                await vectorStore.BuildVectorStoreAsync();
                logger.LogInformation("Built vector store");

                // Test cases for fuzzy matching
                var testQueries = new Dictionary<string, string>
                {
                    // Common typos
                    { "machne learnin", "Machine Learning with typos" },
                    { "artifcial inteligence", "AI with multiple typos" },
                    
                    // Missing characters
                    { "neural nework", "Neural Network missing 't'" },
                    { "deep learing", "Deep Learning missing 'n'" },
                    
                    // Extra characters
                    { "databasee management", "Database with extra 'e'" },
                    { "programmingg language", "Programming with extra 'g'" },
                    
                    // Mixed cases
                    { "macHIne LeARNing", "Mixed case input" },
                    { "DeEp LEarning", "Mixed case with typo" },
                    
                    // Multiple variations in one query
                    { "artficial nueral netwrks for deepp learnin", "Multiple variations" },
                };

                foreach (var test in testQueries)
                {
                    Console.WriteLine($"\nTest Case: {test.Value}");
                    Console.WriteLine($"Query: {test.Key}");
                    Console.WriteLine("Results:");
                    
                    var result = await searchService.SearchAsync(test.Key);
                    
                    if (result.Predictions.Any())
                    {
                        Console.WriteLine($"Expanded Query: {result.ExpandedQuery}");
                        foreach (var prediction in result.Predictions)
                        {
                            Console.WriteLine($"- {prediction.Concept.Name} (Score: {prediction.Score:P2})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No matches found");
                    }
                    
                    Console.WriteLine(new string('-', 50));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred in the test program");
            }
        }

        static void AddTestConcepts(ConceptStore conceptStore)
        {
            // AI/ML Concept with variations
            var mlConcept = new Concept
            {
                Id = "ml",
                Name = "Machine Learning",
                Description = "Artificial Intelligence and Machine Learning concepts"
            };
            
            mlConcept.Documents.Add(new Document
            {
                Id = "ml1",
                Content = @"Machine learning and artificial intelligence are transforming technology. 
                           Neural networks and deep learning enable computers to learn from data.
                           AI systems can recognize patterns and make decisions."
            });
            
            mlConcept.Documents.Add(new Document
            {
                Id = "ml2",
                Content = @"Deep learning is a subset of machine learning using artificial neural networks. 
                           These networks process data through multiple layers for pattern recognition.
                           Machine learning algorithms improve through experience."
            });

            // Programming Concept with variations
            var progConcept = new Concept
            {
                Id = "prog",
                Name = "Programming",
                Description = "Programming and Software Development"
            };
            
            progConcept.Documents.Add(new Document
            {
                Id = "prog1",
                Content = @"Programming languages enable software development.
                           Different languages serve different purposes.
                           Software engineering involves systematic coding practices."
            });
            
            progConcept.Documents.Add(new Document
            {
                Id = "prog2",
                Content = @"Database management systems store and retrieve data.
                           Programming interfaces connect different systems.
                           Software development requires careful planning."
            });

            // Add concepts
            conceptStore.AddConcept(mlConcept);
            conceptStore.AddConcept(progConcept);
        }
    }
}