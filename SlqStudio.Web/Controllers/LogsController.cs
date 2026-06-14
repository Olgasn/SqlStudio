using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlqStudio.Application.Services.LogFileServices;
using SlqStudio.ViewModel.Logs;

namespace SlqStudio.Controllers
{
    [Authorize(Roles = "editingteacher")]
    public class LogsController : Controller
    {
        private readonly ILogFileService _logFileService;
        private const int PageSize = 10;

        public LogsController(ILogFileService logFileService)
        {
            _logFileService = logFileService;
        }

        public IActionResult Index(int page = 1)
        {
            var dto = _logFileService.GetLogFiles(page, PageSize);
            
            var model = new LogFileListViewModel
            {
                Files = dto.Files.Select(f => new LogFileViewModel
                {
                    FileName = f.FileName,
                    FileSize = f.FileSize,
                    LastModified = f.LastModified
                }).ToList(),
                CurrentPage = dto.CurrentPage,
                TotalPages = dto.TotalPages
            };

            return View(model);
        }

        public IActionResult ViewFile(string fileName)
        {
            var content = _logFileService.GetFileContent(fileName);
            ViewBag.FileName = fileName;
            ViewBag.Content = content;

            return View();
        }

        public IActionResult Download(string fileName)
        {
            var fileBytes = _logFileService.GetFileBytes(fileName);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpPost]
        public IActionResult Delete(string fileName)
        {
            _logFileService.DeleteFile(fileName);
            return RedirectToAction(nameof(Index));
        }
    }
}