using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("download")]
        public IActionResult DownloadFile([FromQuery] string fileUrl)
        {
            // 获取物理路径
            string physicalPath = _fileService.GetPhysicalPath(fileUrl);
            if (physicalPath == null) return NotFound("文件不存在或已被删除");

            // 获取文件名
            string downloadName = _fileService.GetOriginalFileName(fileUrl);

            // 获取MIME类型
            // .NET 提供的 File 结果会自动处理 Content-Type
            var fileBytes = System.IO.File.ReadAllBytes(physicalPath);

            // 返回文件流
            return File(fileBytes, "application/octet-stream", downloadName);
        }
    }
}
