namespace SmartDocumentSearch.ViewModels
{
    public class DocumentSearchViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;

        public int TotalMatches { get; set; }

        public List<DocumentSearchResultViewModel> Results { get; set; }
            = new List<DocumentSearchResultViewModel>();
    }

    public class DocumentSearchResultViewModel
    {
        public int DocumentId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public int? PageNumber { get; set; }

        public string TextSnippet { get; set; } = string.Empty;

        public int MatchCount { get; set; }
    }
}