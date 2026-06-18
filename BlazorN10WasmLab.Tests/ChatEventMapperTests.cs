using BlazorN10WasmLab.Contracts;
using BlazorN10WasmLab.Services;

namespace BlazorN10WasmLab.Tests;

/// <summary>
/// ChatEventMapper 的單元測試：把內部 ChatStreamEvent 映射成扁平 ChatStreamReply。
/// 純函式（不依賴 OpenAI / 串流），含 Search 工具參數解析（搬自來源 ToMarker）。
/// </summary>
public class ChatEventMapperTests
{
    [Fact]
    public void Map_TextDelta_SetsKindAndText()
    {
        var reply = ChatEventMapper.Map(new ChatStreamEvent.TextDelta("hello"));

        Assert.Equal(ChatEventKind.TextDelta, reply.Kind);
        Assert.Equal("hello", reply.TextDelta);
    }

    [Fact]
    public void Map_LoadDocumentsToolCall_SetsToolNameOnly()
    {
        var call = new ToolCall("c1", "LoadDocuments", "{}");
        var reply = ChatEventMapper.Map(new ChatStreamEvent.ToolCallStarted(call));

        Assert.Equal(ChatEventKind.ToolCallStarted, reply.Kind);
        Assert.Equal("LoadDocuments", reply.ToolName);
        Assert.Null(reply.SearchPhrase);
        Assert.Null(reply.FilenameFilter);
    }

    [Fact]
    public void Map_SearchToolCall_ParsesPhraseAndFilter()
    {
        var args = """{"searchPhrase":"battery life","filenameFilter":"Example_GPS_Watch.md"}""";
        var call = new ToolCall("c2", "Search", args);
        var reply = ChatEventMapper.Map(new ChatStreamEvent.ToolCallStarted(call));

        Assert.Equal(ChatEventKind.ToolCallStarted, reply.Kind);
        Assert.Equal("Search", reply.ToolName);
        Assert.Equal("battery life", reply.SearchPhrase);
        Assert.Equal("Example_GPS_Watch.md", reply.FilenameFilter);
    }

    [Fact]
    public void Map_SearchToolCall_NoFilter_LeavesFilterNull()
    {
        var args = """{"searchPhrase":"waterproof"}""";
        var call = new ToolCall("c3", "Search", args);
        var reply = ChatEventMapper.Map(new ChatStreamEvent.ToolCallStarted(call));

        Assert.Equal("waterproof", reply.SearchPhrase);
        Assert.Null(reply.FilenameFilter);
    }

    [Fact]
    public void Map_SearchToolCall_InvalidJson_LeavesParamsNull()
    {
        var call = new ToolCall("c4", "Search", "not-json");
        var reply = ChatEventMapper.Map(new ChatStreamEvent.ToolCallStarted(call));

        Assert.Equal("Search", reply.ToolName);
        Assert.Null(reply.SearchPhrase);
        Assert.Null(reply.FilenameFilter);
    }

    [Fact]
    public void ResponseId_BuildsResponseIdReply()
    {
        var reply = ChatEventMapper.ResponseId("resp_123");

        Assert.Equal(ChatEventKind.ResponseId, reply.Kind);
        Assert.Equal("resp_123", reply.ResponseId);
    }
}
