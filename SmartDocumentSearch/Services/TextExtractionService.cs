using DocumentFormat.OpenXml.Packaging;
using SmartDocumentSearch.Interfaces;
using SmartDocumentSearch.Models;
using UglyToad.PdfPig;

namespace SmartDocumentSearch.Services
{
    public class TextExtractionService : ITextExtractionService
    {
        public async Task<List<ExtractedPage>> ExtractTextAsync(
            string filePath,
            string extension)
        {
            extension = extension.ToLowerInvariant();

            return extension switch
            {
                ".pdf" => ExtractFromPdf(filePath),
                ".docx" => ExtractFromWord(filePath),
                ".txt" => await ExtractFromTextFileAsync(filePath),

                _ => throw new NotSupportedException(
                    "This file type is not supported.")
            };
        }

        private List<ExtractedPage> ExtractFromPdf(
            string filePath)
        {
            List<ExtractedPage> extractedPages = new();

            using PdfDocument pdfDocument =
                PdfDocument.Open(filePath);

            foreach (var page in pdfDocument.GetPages())
            {
                if (!string.IsNullOrWhiteSpace(page.Text))
                {
                    extractedPages.Add(new ExtractedPage
                    {
                        PageNumber = page.Number,
                        Text = page.Text
                    });
                }
            }

            return extractedPages;
        }

        private List<ExtractedPage> ExtractFromWord(
            string filePath)
        {
            List<ExtractedPage> extractedPages = new();

            using WordprocessingDocument wordDocument =
                WordprocessingDocument.Open(
                    filePath,
                    false);

            string extractedText =
                wordDocument.MainDocumentPart?
                    .Document?
                    .Body?
                    .InnerText ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(extractedText))
            {
                extractedPages.Add(new ExtractedPage
                {
                    PageNumber = null,
                    Text = extractedText
                });
            }

            return extractedPages;
        }

        private async Task<List<ExtractedPage>>
            ExtractFromTextFileAsync(string filePath)
        {
            string extractedText =
                await File.ReadAllTextAsync(filePath);

            List<ExtractedPage> extractedPages = new();

            if (!string.IsNullOrWhiteSpace(extractedText))
            {
                extractedPages.Add(new ExtractedPage
                {
                    PageNumber = null,
                    Text = extractedText
                });
            }

            return extractedPages;
        }
    }
}