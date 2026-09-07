using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDocumentSearch.Models
{
    public class DocumentContent
    {
        [Key]
        public int DocumentContentId { get; set; }

        [Required]
        public int DocumentId { get; set; }

        public int? PageNumber { get; set; }

        [Required]
        public string ExtractedText { get; set; } = string.Empty;

        [ForeignKey(nameof(DocumentId))]
        public Document? Document { get; set; }
    }
}