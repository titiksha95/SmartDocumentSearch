using System.ComponentModel.DataAnnotations;

namespace SmartDocumentSearch.Models
{
    public class SearchHistory
    {
        [Key]
        public int SearchHistoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string SearchTerm { get; set; } = string.Empty;

        public int ResultCount { get; set; }

        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    }
}