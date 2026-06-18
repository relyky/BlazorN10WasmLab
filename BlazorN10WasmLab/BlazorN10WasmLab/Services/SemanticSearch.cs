using OpenAI.Embeddings;
using BlazorN10WasmLab.Services.Ingestion;
using BlazorN10WasmLab.Services.VectorStore;

namespace BlazorN10WasmLab.Services;

/// <summary>
/// 對 UI 的檢索門面。自管 sqlite-vec 後:查詢時用原生 EmbeddingClient 算 query 向量,
/// 再交給 SqliteVecStore 做 KNN(含 documentId filter)。
/// LoadDocumentsAsync 維持一次性 lazy 觸發(_ingestionTask 快取)。
/// </summary>
public class SemanticSearch(
    EmbeddingClient embeddingClient,
    SqliteVecStore vectorStore,
    [FromKeyedServices("ingestion_directory")] DirectoryInfo ingestionDirectory,
    DataIngestor dataIngestor)
{
    private Task? _ingestionTask;

    public async Task LoadDocumentsAsync() => await (_ingestionTask ??= dataIngestor.IngestDataAsync(ingestionDirectory, searchPattern: "*.*"));

    public async Task<IReadOnlyList<IngestedChunk>> SearchAsync(string text, string? documentIdFilter, int maxResults)
    {
        // Ensure documents have been loaded before searching
        await LoadDocumentsAsync();

        var queryEmbedding = (await embeddingClient.GenerateEmbeddingAsync(text)).Value.ToFloats();
        return await vectorStore.SearchAsync(queryEmbedding, documentIdFilter, maxResults);
    }
}
