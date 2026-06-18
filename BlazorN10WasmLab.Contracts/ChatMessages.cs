using ProtoBuf;

namespace BlazorN10WasmLab.Contracts;

/// <summary>串流事件種類（取代來源 ChatStreamEvent 的繼承階層，扁平化以利 protobuf-net 序列化）。</summary>
public enum ChatEventKind
{
    /// <summary>一段新到達的回答文字（打字機效果）。</summary>
    TextDelta = 0,

    /// <summary>模型發起一次工具呼叫（尚未執行），供 UI 顯示卡片。</summary>
    ToolCallStarted = 1,

    /// <summary>本回合最終 response id（取代來源 onFinalResponseId 回呼），供下一回合鏈接。</summary>
    ResponseId = 2,
}

/// <summary>串流對話請求：使用者輸入 + 上一回合 server 端對話狀態（首回合為 null）。</summary>
[ProtoContract]
public class ChatStreamRequest
{
    [ProtoMember(1)]
    public string UserInput { get; set; } = string.Empty;

    [ProtoMember(2)]
    public string? PreviousResponseId { get; set; }
}

/// <summary>串流對話回覆（扁平）：依 Kind 決定哪些欄位有值。</summary>
[ProtoContract]
public class ChatStreamReply
{
    [ProtoMember(1)]
    public ChatEventKind Kind { get; set; }

    /// <summary>Kind=TextDelta：新到達的回答文字片段。</summary>
    [ProtoMember(2)]
    public string? TextDelta { get; set; }

    /// <summary>Kind=ToolCallStarted：工具名稱（LoadDocuments / Search）。</summary>
    [ProtoMember(3)]
    public string? ToolName { get; set; }

    /// <summary>Kind=ToolCallStarted 且為 Search：搜尋關鍵詞。</summary>
    [ProtoMember(4)]
    public string? SearchPhrase { get; set; }

    /// <summary>Kind=ToolCallStarted 且為 Search：選用的檔名過濾。</summary>
    [ProtoMember(5)]
    public string? FilenameFilter { get; set; }

    /// <summary>Kind=ResponseId：本回合最終 response id。</summary>
    [ProtoMember(6)]
    public string? ResponseId { get; set; }
}

/// <summary>追問建議請求：已縮減的對話脈絡（前端整形後傳入）。</summary>
[ProtoContract]
public class SuggestRequest
{
    [ProtoMember(1)]
    public List<string> ContextLines { get; set; } = [];
}

/// <summary>追問建議回覆：≤3 條建議。</summary>
[ProtoContract]
public class SuggestionsReply
{
    [ProtoMember(1)]
    public List<string> Suggestions { get; set; } = [];
}
