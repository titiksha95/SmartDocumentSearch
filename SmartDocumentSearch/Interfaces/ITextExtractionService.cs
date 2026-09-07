using SmartDocumentSearch.Models;

namespace SmartDocumentSearch.Interfaces
{
    public interface ITextExtractionService
    {
        Task<List<ExtractedPage>> ExtractTextAsync(
            string filePath,
            string extension);
    }
}