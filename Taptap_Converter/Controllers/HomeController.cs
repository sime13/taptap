using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Taptap_Converter.Models;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using PdfiumViewer;
using System.IO.Compression;

namespace Taptap_Converter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ConvertPdftoJpg()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConvertPdftoJpgAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewData["Message"] = "Veuillez sélectionner un fichier valide.";
                return View("ConvertPdftoJpg");
            }

            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                using (var pdfDocument = PdfDocument.Load(filePath))
                {
                    int pageCount = pdfDocument.PageCount;
                    var fileBaseName = Path.GetFileNameWithoutExtension(file.FileName);

                    if (pageCount == 1)
                    {
                        // Conversion d'une seule page (pas besoin de ZIP)
                        using (var image = pdfDocument.Render(0, 300, 300, true))
                        {
                            var jpgPath = Path.Combine(uploadPath, $"{fileBaseName}.jpg");
                            image.Save(jpgPath, ImageFormat.Jpeg);
                            return File(System.IO.File.ReadAllBytes(jpgPath), "image/jpeg", $"{fileBaseName}.jpg");
                        }
                    }
                    else
                    {
                        // Conversion de plusieurs pages -> ZIP
                        var imagesPath = Path.Combine(uploadPath, fileBaseName);

                        if (!Directory.Exists(imagesPath))
                        {
                            Directory.CreateDirectory(imagesPath);
                        }

                        for (int i = 0; i < pageCount; i++)
                        {
                            using (var image = pdfDocument.Render(i, 300, 300, true))
                            {
                                var jpgPath = Path.Combine(imagesPath, $"page_{i + 1}.jpg");
                                image.Save(jpgPath, ImageFormat.Jpeg);
                            }
                        }

                        var zipFileName = Path.Combine(uploadPath, $"{fileBaseName}.zip");

                        if (System.IO.File.Exists(zipFileName))
                        {
                            System.IO.File.Delete(zipFileName);
                        }

                        ZipFile.CreateFromDirectory(imagesPath, zipFileName);
                        return File(System.IO.File.ReadAllBytes(zipFileName), "application/zip", $"{fileBaseName}.zip");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["Message"] = "Erreur lors de la conversion : " + ex.Message;
                return View("ConvertPdftoJpg");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
