using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections.Generic;

namespace RAGFramework
{
    public class Concept
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Document> Documents { get; set; } = new();
        
        // Metadata for the concept
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class Document
    {
        public string Id { get; set; }
        public string ConceptId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    // Training data structure
    public class ConceptTrainingData
    {
        [VectorType(1)]
        public float[] Features { get; set; }
        
        [VectorType(1)]
        public float[] Labels { get; set; }
    }

    // Prediction input structure
    public class ConceptPredictionInput
    {
        public string Text { get; set; }
    }

    // Prediction output structure
    public class ConceptPredictionOutput
    {
        public float[] Scores { get; set; }
    }

    public class TextPreprocessor
    {
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

        public IEnumerable<string> TokenizeText(string text)
        {
            // Basic word tokenization
            return text.Split(new[] { ' ', '\t', ',', '.', '!', '?' }, 
                            StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public class ConceptStore
    {
        private readonly Dictionary<string, Concept> _concepts = new();
        private readonly TextPreprocessor _preprocessor = new();

        public void AddConcept(Concept concept)
        {
            if (string.IsNullOrEmpty(concept.Id))
                throw new ArgumentException("Concept must have an ID");

            _concepts[concept.Id] = concept;
        }

        public void AddDocument(string conceptId, Document document)
        {
            if (!_concepts.ContainsKey(conceptId))
                throw new KeyNotFoundException($"Concept {conceptId} not found");

            document.ConceptId = conceptId;
            _concepts[conceptId].Documents.Add(document);
        }

        public IEnumerable<Concept> GetAllConcepts()
        {
            return _concepts.Values;
        }

        public Concept GetConcept(string conceptId)
        {
            return _concepts[conceptId];
        }
    }
}