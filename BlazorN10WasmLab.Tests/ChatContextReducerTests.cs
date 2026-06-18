using BlazorN10WasmLab.Contracts;

namespace BlazorN10WasmLab.Tests;

/// <summary>
/// ChatContextReducer 的單元測試：把對話訊息縮減成建議服務的脈絡輸入。
/// 純函式（不依賴 UI 模型）：取最後數則「非空」訊息，組成 "Role: Text"。搬自來源 ReduceMessages。
/// </summary>
public class ChatContextReducerTests
{
    [Fact]
    public void Reduce_FormatsRoleAndText()
    {
        var lines = ChatContextReducer.Reduce(
        [
            ("User", "hello"),
            ("Assistant", "hi there"),
        ], take: 5);

        Assert.Equal(["User: hello", "Assistant: hi there"], lines);
    }

    [Fact]
    public void Reduce_SkipsEmptyText()
    {
        var lines = ChatContextReducer.Reduce(
        [
            ("User", "keep"),
            ("Assistant", ""),
            ("Assistant", "   "),
            ("User", "also keep"),
        ], take: 5);

        Assert.Equal(["User: keep", "User: also keep"], lines);
    }

    [Fact]
    public void Reduce_TakesLastNOnly()
    {
        var msgs = new (string, string)[]
        {
            ("User", "m1"), ("Assistant", "m2"), ("User", "m3"),
            ("Assistant", "m4"), ("User", "m5"), ("Assistant", "m6"),
        };

        var lines = ChatContextReducer.Reduce(msgs, take: 5);

        // 取最後 5 則（m2..m6），m1 被丟。
        Assert.Equal(5, lines.Count);
        Assert.Equal("Assistant: m2", lines[0]);
        Assert.Equal("Assistant: m6", lines[^1]);
    }

    [Fact]
    public void Reduce_EmptyInput_ReturnsEmpty()
    {
        var lines = ChatContextReducer.Reduce([], take: 5);
        Assert.Empty(lines);
    }
}
