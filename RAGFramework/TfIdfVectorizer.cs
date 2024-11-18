using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;

namespace RAGFramework
{
    public class TextData
    {
        public string Text { get; set; }
    }

    public class TransformedTextData
    {
        public float[] Features { get; set; }
    }

    public class TfIdfVectorizer
    {
        private readonly MLContext _mlContext;
        private ITransformer _transformer;
        private int _vocabSize;

        public TfIdfVectorizer()
        {
            _mlContext = new MLContext(seed: 42);
        }
        
        public IEnumerable<string> GetTokens(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Enumerable.Empty<string>();

            // Basic tokenization - you might want to make this more sophisticated
            return text.ToLower()
                .Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?' }, 
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(token => !string.IsNullOrWhiteSpace(token));
        }

        public void Fit(IEnumerable<string> documents)
        {
            // Convert documents to TextData format
            var textData = documents.Select(doc => new TextData { Text = doc });
            var trainingData = _mlContext.Data.LoadFromEnumerable(textData);

            // Create the transformation pipeline
            // Create the transformation pipeline
            var pipeline = _mlContext.Transforms.Text.NormalizeText("NormalizedText", "Text")
                .Append(_mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "NormalizedText"))
                .Append(_mlContext.Transforms.Text.RemoveStopWords("Tokens"))
                .Append(_mlContext.Transforms.Text.ProduceWordBags(
                    "Features", 
                    "Tokens",
                    weighting: Microsoft.ML.Transforms.Text.NgramExtractingEstimator.WeightingCriteria.TfIdf));

            // Fit the pipeline
            _transformer = pipeline.Fit(trainingData);

            // Extract vocabulary information
            var transformedData = _transformer.Transform(trainingData);
            var featuresColumn = transformedData.GetColumn<float[]>("Features").First();
            _vocabSize = featuresColumn.Length;
        }

        public float[] Transform(string text)
        {
            var inputData = new List<TextData> { new TextData { Text = text } };
            var data = _mlContext.Data.LoadFromEnumerable(inputData);
            var transformedData = _transformer.Transform(data);
            return transformedData.GetColumn<float[]>("Features").First();
        }

        public double CalculateCosineSimilarity(float[] vector1, float[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same length");

            double dotProduct = 0;
            double norm1 = 0;
            double norm2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                norm1 += vector1[i] * vector1[i];
                norm2 += vector2[i] * vector2[i];
            }

            if (norm1 == 0 || norm2 == 0)
                return 0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
    }
}