using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;

namespace BlazorN10WasmLab.Services.VectorStore;

/// <summary>
/// 自管的 sqlite-vec 向量庫存取(取代 Semantic Kernel 的 VectorStoreCollection)。
/// 一張 vec0 虛擬表同時存 metadata(key / documentid / content)與 float[1536] 向量。
/// embedding 由呼叫端(DataIngestor / SemanticSearch)用原生 EmbeddingClient 顯式算好後傳入。
/// </summary>
public sealed class SqliteVecStore(string connectionString)
{
    private const string TableName = "vec_chunks";

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        connection.EnableExtensions();
        connection.LoadVector(); // 載入 vec0 extension(sqlite-vec 套件提供)
        return connection;
    }

    /// <summary>
    /// 整批重建:drop 舊表 → create → 全量寫入。對應現況 IncrementalIngestion=false 語意。
    /// </summary>
    public async Task RebuildAsync(IReadOnlyList<(IngestedChunk Chunk, ReadOnlyMemory<float> Embedding)> items, CancellationToken cancellationToken = default)
    {
        await using var connection = OpenConnection();
        await using (var drop = connection.CreateCommand())
        {
            drop.CommandText = $"DROP TABLE IF EXISTS {TableName};";
            await drop.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var create = connection.CreateCommand())
        {
            create.CommandText = $"""
                CREATE VIRTUAL TABLE {TableName} USING vec0(
                    chunk_key TEXT PRIMARY KEY,
                    documentid TEXT,
                    content TEXT,
                    embedding FLOAT[{IngestedChunk.VectorDimensions}] distance_metric=cosine
                );
                """;
            await create.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await using var insert = connection.CreateCommand();
        insert.CommandText = $"""
            INSERT INTO {TableName}(chunk_key, documentid, content, embedding)
            VALUES ($key, $documentid, $content, $embedding);
            """;
        var pKey = insert.CreateParameter(); pKey.ParameterName = "$key"; insert.Parameters.Add(pKey);
        var pDoc = insert.CreateParameter(); pDoc.ParameterName = "$documentid"; insert.Parameters.Add(pDoc);
        var pContent = insert.CreateParameter(); pContent.ParameterName = "$content"; insert.Parameters.Add(pContent);
        var pEmbedding = insert.CreateParameter(); pEmbedding.ParameterName = "$embedding"; insert.Parameters.Add(pEmbedding);

        foreach (var (chunk, embedding) in items)
        {
            pKey.Value = chunk.Key;
            pDoc.Value = chunk.DocumentId;
            pContent.Value = chunk.Text;
            pEmbedding.Value = ToBlob(embedding);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// KNN 查詢:給定 query 向量,回傳最近的 chunk。可選 documentId filter(只搜該檔)。
    /// </summary>
    public async Task<IReadOnlyList<IngestedChunk>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding,
        string? documentIdFilter,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        await using var connection = OpenConnection();
        await using var command = connection.CreateCommand();

        var filterClause = string.IsNullOrEmpty(documentIdFilter) ? "" : " AND documentid = $documentid";
        command.CommandText = $"""
            SELECT chunk_key, documentid, content
            FROM {TableName}
            WHERE embedding MATCH $query AND k = $k{filterClause}
            ORDER BY distance;
            """;

        var pQuery = command.CreateParameter(); pQuery.ParameterName = "$query"; pQuery.Value = ToBlob(queryEmbedding); command.Parameters.Add(pQuery);
        var pK = command.CreateParameter(); pK.ParameterName = "$k"; pK.Value = maxResults; command.Parameters.Add(pK);
        if (!string.IsNullOrEmpty(documentIdFilter))
        {
            var pDoc = command.CreateParameter(); pDoc.ParameterName = "$documentid"; pDoc.Value = documentIdFilter; command.Parameters.Add(pDoc);
        }

        var results = new List<IngestedChunk>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new IngestedChunk
            {
                Key = reader.GetString(0),
                DocumentId = reader.GetString(1),
                Text = reader.GetString(2),
            });
        }
        return results;
    }

    /// <summary>float 向量序列化成 sqlite-vec 要的 compact BLOB(float32 原始 bytes)。</summary>
    private static byte[] ToBlob(ReadOnlyMemory<float> vector)
        => MemoryMarshal.AsBytes(vector.Span).ToArray();
}
