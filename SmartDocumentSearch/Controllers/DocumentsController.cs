using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartDocumentSearch.Data;
using SmartDocumentSearch.Models;

namespace SmartDocumentSearch.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DocumentsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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

            // Maximum allowed size: 10 MB
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

            // Path: ProjectFolder/Uploads
            string uploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

            Directory.CreateDirectory(uploadsFolder);

            // Unique name prevents files from overwriting each other
            string storedFileName =
                $"{Guid.NewGuid()}{extension}";

            string completeFilePath = Path.Combine(
                uploadsFolder,
                storedFileName);

            try
            {
                // Save physical file
                await using (FileStream stream =
                    new FileStream(
                        completeFilePath,
                        FileMode.CreateNew))
                {
                    await file.CopyToAsync(stream);
                }

                // Save information in the Documents table
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
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Document uploaded successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                // Remove the physical file if database saving fails
                if (System.IO.File.Exists(completeFilePath))
                {
                    System.IO.File.Delete(completeFilePath);
                }

                ModelState.AddModelError(
                    "",
                    "The document could not be uploaded.");

                return View();
            }
        }
    }
}