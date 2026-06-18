namespace BlazorN10WasmLab.Services.Ingestion;

/// <summary>
/// 一份讀進來的文件:識別(相對檔名)+ 純文字內容。
/// 取代 Microsoft.Extensions.DataIngestion 的 IngestionDocument 容器。
/// </summary>
public sealed record SourceDocument(string DocumentId, string Text);

/// <summary>把單一檔案讀成純文字。</summary>
public interface IFileTextExtractor
{
    Task<string> ExtractAsync(FileInfo file, CancellationToken cancellationToken = default);
}

/// <summary>
/// 依副檔名分流的讀檔器(取代繼承自 IngestionDocumentReader 的 DocumentReader）。
/// .md → 存原始 md 文字;.pdf → PdfPig 抽字。
/// </summary>
public sealed class DocumentReader(DirectoryInfo rootDirectory)
{
    private readonly MarkdownReader _markdownReader = new();
    private readonly PdfPigReader _pdfReader = new();

    /// <summary>讀取目錄下符合條件的檔案,逐一回傳純文字文件。不支援的副檔名略過。</summary>
    public async IAsyncEnumerable<SourceDocument> ReadAllAsync(string searchPattern = "*.*")
    {
        foreach (var file in rootDirectory.GetFiles(searchPattern, SearchOption.AllDirectories))
        {
            var extractor = ResolveExtractor(file);
            if (extractor is null)
                continue;

            var documentId = Path.GetRelativePath(rootDirectory.FullName, file.FullName);
            var text = await extractor.ExtractAsync(file);
            yield return new SourceDocument(documentId, text);
        }
    }

    private IFileTextExtractor? ResolveExtractor(FileInfo file)
        => file.Extension.ToLowerInvariant() switch
        {
            ".md" => _markdownReader,
            ".pdf" => _pdfReader,
            _ => null,
        };
}
