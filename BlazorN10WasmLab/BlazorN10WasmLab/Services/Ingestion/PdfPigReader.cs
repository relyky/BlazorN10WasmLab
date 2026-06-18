using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace BlazorN10WasmLab.Services.Ingestion;

/// <summary>
/// PDF 讀檔:沿用 PdfPig 的抽字邏輯(逐頁 → Docstrum text block),把所有 text block 串成純文字。
/// </summary>
internal sealed class PdfPigReader : IFileTextExtractor
{
    public Task<string> ExtractAsync(FileInfo file, CancellationToken cancellationToken = default)
    {
        using var pdf = PdfDocument.Open(file.FullName);
        var sb = new StringBuilder();
        foreach (var page in pdf.GetPages())
        {
            var words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters);
            foreach (var textBlock in DocstrumBoundingBoxes.Instance.GetBlocks(words))
            {
                sb.AppendLine(textBlock.Text);
            }
        }
        return Task.FromResult(sb.ToString());
    }
}
