using OpenAI.Embeddings;
using BlazorN10WasmLab.Services.VectorStore;

namespace BlazorN10WasmLab.Services.Ingestion;

/// <summary>
/// 自管 ingestion:讀檔 → 固定 token 切塊 → 原生 EmbeddingClient 批次算向量 → 整批重建寫入 sqlite-vec。
/// 取代原本以 Microsoft.Extensions.DataIngestion pipeline 為核心的實作。
/// </summary>
public sealed class DataIngestor(
    ILogger<DataIngestor> logger,
    EmbeddingClient embeddingClient,
    SqliteVecStore vectorStore)
{
    // embedding 請求的批次保護(那份 Azure 文件:單請求陣列 ≤2048 筆)。token 總和上限(300K)
    // 對本範例的小文件不會觸及,故僅以筆數分批;日後換大文件可再加 token 計量。
    private const int MaxEmbeddingBatchSize = 2048;

    private readonly TokenChunker _chunker = new(chunkSize: 512, overlap: 64);

    public async Task IngestDataAsync(DirectoryInfo directory, string searchPattern)
    {
        var reader = new DocumentReader(directory);

        // 1. 讀檔 + 切塊
        var chunks = new List<IngestedChunk>();
        await foreach (var doc in reader.ReadAllAsync(searchPattern))
        {
            foreach (var text in _chunker.Chunk(doc.Text))
            {
                chunks.Add(new IngestedChunk
                {
                    Key = Guid.NewGuid().ToString(),
                    DocumentId = doc.DocumentId,
                    Text = text,
                });
            }
            logger.LogInformation("Read and chunked '{DocumentId}'.", doc.DocumentId);
        }

        if (chunks.Count == 0)
        {
            logger.LogWarning("No chunks produced from '{Directory}'.", directory.FullName);
            await vectorStore.RebuildAsync([]);
            return;
        }

        // 2. 批次算 embedding
        var items = new List<(IngestedChunk, ReadOnlyMemory<float>)>(chunks.Count);
        for (var offset = 0; offset < chunks.Count; offset += MaxEmbeddingBatchSize)
        {
            var batch = chunks.Skip(offset).Take(MaxEmbeddingBatchSize).ToList();
            var embeddings = await embeddingClient.GenerateEmbeddingsAsync(batch.Select(c => c.Text).ToList());
            for (var i = 0; i < batch.Count; i++)
            {
                items.Add((batch[i], embeddings.Value[i].ToFloats()));
            }
        }

        // 3. 整批重建寫入向量庫
        await vectorStore.RebuildAsync(items);
        logger.LogInformation("Ingested {Count} chunks into vector store.", items.Count);
    }
}
