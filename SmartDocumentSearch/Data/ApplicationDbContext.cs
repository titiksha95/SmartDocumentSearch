using Microsoft.EntityFrameworkCore;
using SmartDocumentSearch.Models;

namespace SmartDocumentSearch.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }

        public DbSet<DocumentContent> DocumentContents { get; set; }

        public DbSet<SearchHistory> SearchHistories { get; set; }
    }
}