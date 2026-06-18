namespace BlazorN10WasmLab.Contracts;

/// <summary>
/// 把對話訊息縮減成追問建議服務的脈絡輸入。純函式、不依賴 UI 模型：
/// 取最後 take 則「非空」訊息，組成 "Role: Text"。搬自來源 ChatSuggestions.ReduceMessages。
/// </summary>
public static class ChatContextReducer
{
    public static IReadOnlyList<string> Reduce(IEnumerable<(string Role, string Text)> messages, int take)
        => messages
            .Where(m => !string.IsNullOrWhiteSpace(m.Text))
            .TakeLast(take)
            .Select(m => $"{m.Role}: {m.Text}")
            .ToList();
}
