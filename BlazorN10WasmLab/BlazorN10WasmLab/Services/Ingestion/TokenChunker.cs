using Microsoft.ML.Tokenizers;

namespace BlazorN10WasmLab.Services.Ingestion;

/// <summary>
/// 固定 token 大小切塊 + overlap(取代語意切塊 SemanticSimilarityChunker)。
/// 純函式:不呼叫 embedding、不碰 IO。用 Tiktoken 計算 token 邊界。
/// </summary>
public sealed class TokenChunker
{
    private readonly Tokenizer _tokenizer;
    private readonly int _chunkSize;
    private readonly int _overlap;

    public TokenChunker(int chunkSize = 512, int overlap = 64, Tokenizer? tokenizer = null)
    {
        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "chunkSize 必須為正數。");
        if (overlap < 0 || overlap >= chunkSize)
            throw new ArgumentOutOfRangeException(nameof(overlap), "overlap 必須 >= 0 且小於 chunkSize。");

        _tokenizer = tokenizer ?? TiktokenTokenizer.CreateForModel("gpt-4o");
        _chunkSize = chunkSize;
        _overlap = overlap;
    }

    /// <summary>
    /// 把文字切成固定 token 大小的區塊,相鄰區塊重疊 overlap 個 token。
    /// 回傳每塊的文字內容(已由 token 還原)。空白或無 token 的輸入回傳空集合。
    /// </summary>
    public IReadOnlyList<string> Chunk(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var ids = _tokenizer.EncodeToIds(text);
        if (ids.Count == 0)
            return [];

        var step = _chunkSize - _overlap; // 每次前進的 token 數
        var chunks = new List<string>();
        for (var start = 0; start < ids.Count; start += step)
        {
            var length = Math.Min(_chunkSize, ids.Count - start);
            var slice = ids.Skip(start).Take(length).ToArray();
            chunks.Add(_tokenizer.Decode(slice));

            if (start + length >= ids.Count)
                break; // 已到結尾,避免最後因 overlap 多產生一塊重複尾段
        }
        return chunks;
    }
}
