using Microsoft.ML.Tokenizers;
using BlazorN10WasmLab.Services.Ingestion;

namespace BlazorN10WasmLab.Tests;

/// <summary>
/// TokenChunker 的單元測試。只用本地 Tiktoken tokenizer(無 network / IO / embedding)。
///
/// 斷言策略:Chunk 回傳解碼後文字,切塊邏輯在 token 層級;故以「把每塊重新 EncodeToIds
/// 取 token 數」來驗不變量。解碼→再 encode 的 token 數未必與原 slice 完全一致(tiktoken
/// 邊界效應),因此 token 數只斷言上界(≤ chunkSize),結構性斷言(塊數)用足夠長的可控輸入。
/// </summary>
public class TokenChunkerTests
{
    private static readonly Tokenizer Tokenizer = TiktokenTokenizer.CreateForModel("gpt-4o");

    private static int TokenCount(string text) => Tokenizer.EncodeToIds(text).Count;

    // 產生一段 token 數「至少」為 minTokens 的文字(用遞增數字確保 token 不被過度合併)。
    private static string MakeText(int minTokens)
    {
        var words = new List<string>();
        var i = 0;
        while (TokenCount(string.Join(" ", words)) < minTokens)
        {
            words.Add($"word{i++}");
        }
        return string.Join(" ", words);
    }

    [Fact]
    public void Chunk_EmptyString_ReturnsEmpty()
    {
        var chunker = new TokenChunker(chunkSize: 100, overlap: 10);
        Assert.Empty(chunker.Chunk(""));
    }

    [Fact]
    public void Chunk_Whitespace_ReturnsEmpty()
    {
        var chunker = new TokenChunker(chunkSize: 100, overlap: 10);
        Assert.Empty(chunker.Chunk("   \n\t  "));
    }

    [Fact]
    public void Chunk_ShorterThanOneChunk_ReturnsSingleChunk()
    {
        var chunker = new TokenChunker(chunkSize: 100, overlap: 10);
        var text = MakeText(20); // 遠少於 chunkSize
        Assert.True(TokenCount(text) < 100);

        var chunks = chunker.Chunk(text);

        Assert.Single(chunks);
    }

    [Fact]
    public void Chunk_LongText_ProducesMultipleChunks_EachWithinChunkSize()
    {
        const int chunkSize = 50;
        var chunker = new TokenChunker(chunkSize: chunkSize, overlap: 10);
        var text = MakeText(300); // 遠超過一塊

        var chunks = chunker.Chunk(text);

        Assert.True(chunks.Count > 1, "長文字應切成多塊");
        foreach (var chunk in chunks)
        {
            Assert.True(TokenCount(chunk) <= chunkSize, $"每塊 token 數應 ≤ chunkSize,實際 {TokenCount(chunk)}");
        }
    }

    [Fact]
    public void Chunk_WithOverlap_AdjacentChunksShareContent()
    {
        var chunker = new TokenChunker(chunkSize: 50, overlap: 20);
        var text = MakeText(200);

        var chunks = chunker.Chunk(text);

        Assert.True(chunks.Count >= 2);
        // 相鄰塊應有重疊:第一塊的尾段詞應出現在第二塊的開頭。
        var firstWords = chunks[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var secondChunk = chunks[1];
        var tail = firstWords[^1]; // 第一塊最後一個詞
        Assert.Contains(tail, secondChunk);
    }

    [Fact]
    public void Chunk_ZeroOverlap_NoSharedTailWord()
    {
        var chunker = new TokenChunker(chunkSize: 50, overlap: 0);
        var text = MakeText(200);

        var chunks = chunker.Chunk(text);

        Assert.True(chunks.Count >= 2);
        // overlap=0:相鄰塊不應重複(第一塊最後一詞不等於第二塊第一詞)。
        var firstWords = chunks[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var secondWords = chunks[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.NotEqual(firstWords[^1], secondWords[0]);
    }

    [Fact]
    public void Chunk_ZeroOverlap_CoversAllTokens()
    {
        const int chunkSize = 40;
        var chunker = new TokenChunker(chunkSize: chunkSize, overlap: 0);
        var text = MakeText(200);
        var totalTokens = TokenCount(text);

        var chunks = chunker.Chunk(text);

        // overlap=0 時,各塊 token 數總和應約等於原文(無重疊、無遺漏)。
        // 因解碼邊界效應,允許小容差。
        var sumTokens = chunks.Sum(TokenCount);
        Assert.InRange(sumTokens, totalTokens - chunks.Count, totalTokens + chunks.Count);
    }

    [Fact]
    public void Chunk_LongText_LastChunkNotDuplicatedTail()
    {
        // 驗證迴圈結尾的 break:不因 overlap 在最後多產生一塊重複尾段。
        var chunker = new TokenChunker(chunkSize: 50, overlap: 10);
        var text = MakeText(120);

        var chunks = chunker.Chunk(text);

        // 最後一塊不應與前一塊完全相同(否則代表多產生了重複尾塊)。
        if (chunks.Count >= 2)
        {
            Assert.NotEqual(chunks[^1], chunks[^2]);
        }
    }

    [Theory]
    [InlineData(0, 10)]    // chunkSize <= 0
    [InlineData(-5, 0)]    // chunkSize < 0
    [InlineData(50, 50)]   // overlap >= chunkSize
    [InlineData(50, 60)]   // overlap > chunkSize
    [InlineData(50, -1)]   // overlap < 0
    public void Constructor_InvalidArguments_Throws(int chunkSize, int overlap)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TokenChunker(chunkSize, overlap));
    }
}
