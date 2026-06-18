namespace BlazorN10WasmLab.Client.Pages.Chat;

/// <summary>
/// 訊息角色。取代 Microsoft.Extensions.AI 的 ChatRole(只保留本專案 UI 實際需要的兩種)。
/// </summary>
public enum ChatMessageRole
{
    User,
    Assistant,
}

/// <summary>
/// UI 用的訊息模型,取代 Microsoft.Extensions.AI 的 ChatMessage。
/// 一則 assistant 訊息可同時帶串流文字與若干 tool-call 標記(供 ChatMessageItem 渲染卡片)。
/// </summary>
public sealed class ChatMessageModel
{
    public required ChatMessageRole Role { get; init; }

    /// <summary>串流期間會持續累加的文字內容。</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 本回合模型發起的 tool call(LoadDocuments / Search),依出現順序排列,供卡片渲染。
    /// Slice 1(單輪、無工具)時恆為空;Slice 2 接上工具迴圈後才會填入。
    /// </summary>
    public List<ToolCallMarker> ToolCalls { get; } = new();

    public static ChatMessageModel User(string text) => new() { Role = ChatMessageRole.User, Text = text };

    public static ChatMessageModel Assistant(string text = "") => new() { Role = ChatMessageRole.Assistant, Text = text };
}

/// <summary>
/// 一次 tool call 的渲染所需資訊。Search 會帶 SearchPhrase / FilenameFilter;LoadDocuments 兩者皆為 null。
/// </summary>
public sealed class ToolCallMarker
{
    public required string ToolName { get; init; }
    public string? SearchPhrase { get; init; }
    public string? FilenameFilter { get; init; }
}
