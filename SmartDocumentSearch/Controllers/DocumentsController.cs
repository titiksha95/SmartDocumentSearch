using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartDocumentSearch.Data;
using SmartDocumentSearch.Interfaces;
using SmartDocumentSearch.Models;
using SmartDocumentSearch.Services;


namespace SmartDocumentSearch.Controllers
{
    
    public class DocumentsController : Controller
    {
        private readonly ITextExtractionService _textExtractionService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DocumentsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ITextExtractionService textExtractionService)
        {
            _context = context;
            _environment = environment;
            _textExtractionService = textExtractionService;
        }

        // Display all uploaded documents
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Document> documents = await _context.Documents
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return View(documents);
        }

        [HttpGet]
        public async Task<IActionResult> ViewText(int id)
        {
            Document? document = await _context.Documents
                .Include(d => d.Contents)
                .FirstOrDefaultAsync(d => d.DocumentId == id);

            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // Display upload form
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        // Receive and save uploaded file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(
                    "file",
                    "Please select a file.");

                return View();
            }

            long maximumFileSize = 10 * 1024 * 1024;

            if (file.Length > maximumFileSize)
            {
                ModelState.AddModelError(
                    "file",
                    "The file cannot be larger than 10 MB.");

                return View();
            }

            string extension =
                Path.GetExtension(file.FileName).ToLowerInvariant();

            string[] allowedExtensions =
            {
        ".pdf",
        ".docx",
        ".txt"
    };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "file",
                    "Only PDF, DOCX and TXT files are allowed.");

                return View();
            }

            string uploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

            Directory.CreateDirectory(uploadsFolder);

            string storedFileName =
                $"{Guid.NewGuid()}{extension}";

            string completeFilePath = Path.Combine(
                uploadsFolder,
                storedFileName);

            try
            {
                // Save the physical file
                await using (FileStream stream = new FileStream(
                    completeFilePath,
                    FileMode.CreateNew))
                {
                    await file.CopyToAsync(stream);
                }

                // Extract text according to file type
                List<ExtractedPage> extractedPages =
                    await _textExtractionService.ExtractTextAsync(
                        completeFilePath,
                        extension);

                if (extractedPages.Count == 0)
                {
                    if (System.IO.File.Exists(completeFilePath))
                    {
                        System.IO.File.Delete(completeFilePath);
                    }

                    ModelState.AddModelError(
                        "",
                        "No readable text was found in the document.");

                    return View();
                }

                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                Document document = new Document
                {
                    OriginalFileName =
                        Path.GetFileName(file.FileName),

                    StoredFileName = storedFileName,

                    FilePath = completeFilePath,

                    FileType = extension,

                    FileSize = file.Length,

                    UploadedAt = DateTime.UtcNow
                };

                _context.Documents.Add(document);

                // First save creates DocumentId
                await _context.SaveChangesAsync();

                List<DocumentContent> contents =
                    extractedPages.Select(page =>
                        new DocumentContent
                        {
                            DocumentId = document.DocumentId,
                            PageNumber = page.PageNumber,
                            ExtractedText = page.Text
                        })
                        .ToList();

                _context.DocumentContents.AddRange(contents);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] =
                    $"Document uploaded successfully. " +
                    $"{extractedPages.Count} section(s) extracted.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                if (System.IO.File.Exists(completeFilePath))
                {
                    System.IO.File.Delete(completeFilePath);
                }

                ModelState.AddModelError(
                    "",
                    "The document could not be processed. " +
                    "Make sure it is a valid, readable document.");

                return View();
            }
        }
    }
}