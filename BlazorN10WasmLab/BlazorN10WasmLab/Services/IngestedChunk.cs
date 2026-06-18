namespace BlazorN10WasmLab.Services;

/// <summary>
/// 向量庫中一個可檢索區塊。自管 sqlite-vec 後降為純 POCO(不再帶任何向量庫框架 attribute)。
/// </summary>
public sealed class IngestedChunk
{
    public const int VectorDimensions = 1536; // text-embedding-3-small 的預設維度
    public const string CollectionName = "data-openaichatlab-chunks";

    /// <summary>唯一鍵(對應 sqlite-vec rowid 之外的穩定識別)。</summary>
    public required string Key { get; set; }

    /// <summary>來源文件識別(相對於 Data 目錄的檔名),供 citation 與 documentId filter 使用。</summary>
    public required string DocumentId { get; set; }

    /// <summary>區塊文字內容(embedding 來源,也是 citation quote 的比對基礎)。</summary>
    public required string Text { get; set; }
}
