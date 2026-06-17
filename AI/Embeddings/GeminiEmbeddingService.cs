using Google.GenAI.Types;
using Pgvector;

namespace CardGeneratorBackend.AI.Embeddings
{
    public class GeminiEmbeddingService(Google.GenAI.Client geminiClient) : IEmbeddingService
    {
        private const string MODEL_NAME = "gemini-embedding-001";

        private readonly Google.GenAI.Client mGeminiClient = geminiClient;

        private Vector GetEmbeddingFromResponseObject(ContentEmbedding? responseObj)
        {
            return new Vector(responseObj?.Values?.Select(x => (float)x).ToArray());
        }

        public async Task<Vector> GetEmbedding(string text)
        {
            var response = await mGeminiClient.Models.EmbedContentAsync(MODEL_NAME, text, new EmbedContentConfig {
                OutputDimensionality = 768
            });

            return GetEmbeddingFromResponseObject(response.Embeddings?.First()) 
                ?? throw new EmbeddingException("Unable to generate an embedding for the given string");
        }

        public async Task<IList<Vector>> GetAllEmbeddings(IList<string> items)
        {
            var reqContents = items.Select(item => new Content
            {
                Parts = [ new Part { Text = item } ]
            })
            .ToList();

            var response = await mGeminiClient.Models.EmbedContentAsync(MODEL_NAME, reqContents, new EmbedContentConfig
            {
                OutputDimensionality = 768
            });

            if(response.Embeddings is null)
            {
                throw new EmbeddingException("Unable to generate embeddings");
            }

            return [.. response.Embeddings.Select((embedding, i) => GetEmbeddingFromResponseObject(embedding) 
                ?? throw new EmbeddingException($"Unable to generate an embedding for entry with index ${i}"))];
        }
    }
}