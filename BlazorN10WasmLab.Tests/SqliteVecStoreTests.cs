using BlazorN10WasmLab.Services;
using BlazorN10WasmLab.Services.VectorStore;
using Microsoft.Data.Sqlite;

namespace BlazorN10WasmLab.Tests;

/// <summary>
/// SqliteVecStore 的整合測試。驗證兩件外部行為:
/// (1) sqlite-vec 原生擴充能載入(否則 OpenConnection 即拋例外);
/// (2) rebuild → KNN search 的 round-trip 正確(最近向量回傳對應 chunk)。
///
/// 用臨時 db 檔,每個測試獨立;不碰任何實作細節(直接走公開 RebuildAsync / SearchAsync)。
/// </summary>
public sealed class SqliteVecStoreTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"vec-test-{Guid.NewGuid():N}.db");

    private SqliteVecStore NewStore() => new($"Data Source={_dbPath}");

    // 造一個指定維度的單位向量:第 hotIndex 維為 1,其餘為 0。便於用已知最近鄰驗 KNN。
    private static ReadOnlyMemory<float> OneHot(int hotIndex)
    {
        var v = new float[IngestedChunk.VectorDimensions];
        v[hotIndex] = 1f;
        return v;
    }

    private static IngestedChunk Chunk(string key, string documentId, string text) =>
        new() { Key = key, DocumentId = documentId, Text = text };

    [Fact]
    public async Task Search_AfterRebuild_ReturnsNearestChunk()
    {
        var store = NewStore();
        await store.RebuildAsync(
        [
            (Chunk("k0", "doc-a.md", "alpha"), OneHot(0)),
            (Chunk("k1", "doc-b.md", "bravo"), OneHot(1)),
            (Chunk("k2", "doc-c.md", "charlie"), OneHot(2)),
        ]);

        // 查詢向量與 k1（OneHot(1)）相同 → 最近鄰應為 k1。
        var results = await store.SearchAsync(OneHot(1), documentIdFilter: null, maxResults: 1);

        Assert.Single(results);
        Assert.Equal("k1", results[0].Key);
        Assert.Equal("bravo", results[0].Text);
    }

    [Fact]
    public async Task Search_WithDocumentIdFilter_OnlyReturnsThatDocument()
    {
        var store = NewStore();
        await store.RebuildAsync(
        [
            (Chunk("k0", "doc-a.md", "alpha"), OneHot(0)),
            (Chunk("k1", "doc-b.md", "bravo"), OneHot(1)),
        ]);

        // 查詢最接近 k0,但限定只搜 doc-b.md → 應回 k1（該文件唯一 chunk）。
        var results = await store.SearchAsync(OneHot(0), documentIdFilter: "doc-b.md", maxResults: 5);

        Assert.All(results, r => Assert.Equal("doc-b.md", r.DocumentId));
        Assert.Contains(results, r => r.Key == "k1");
    }

    [Fact]
    public async Task RebuildAsync_EmptyItems_ProducesQueryableEmptyStore()
    {
        var store = NewStore();
        await store.RebuildAsync([]);

        var results = await store.SearchAsync(OneHot(0), documentIdFilter: null, maxResults: 5);

        Assert.Empty(results);
    }

    public void Dispose()
    {
        // SqliteVecStore 每次操作開新連線並依賴連線池;測試結束須先清池釋放檔案 handle 才能刪檔。
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
