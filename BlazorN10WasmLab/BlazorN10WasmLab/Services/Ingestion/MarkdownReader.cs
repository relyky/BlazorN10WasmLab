namespace BlazorN10WasmLab.Services.Ingestion;

/// <summary>
/// Markdown 讀檔:直接存原始 md 文字(不轉純文字),使 citation quote 與 viewer 載入的原檔一致。
/// </summary>
internal sealed class MarkdownReader : IFileTextExtractor
{
    public async Task<string> ExtractAsync(FileInfo file, CancellationToken cancellationToken = default)
        => await File.ReadAllTextAsync(file.FullName, cancellationToken);
}
