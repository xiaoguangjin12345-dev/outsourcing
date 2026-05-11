using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private const string RootFolder = "uploads";

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ----------------- 核心保存方法：返回存储后的相对路径（用于存入数据库）--------------------
        public string SaveFile(IFormFile file, string module, int id, string subDir = "")
        {
            if (file == null || file.Length == 0) return null;

            string relativeDir = Path.Combine(RootFolder, module, id.ToString(), subDir ?? "");
            string physicalDir = Path.Combine(_env.WebRootPath, relativeDir);

            if (!Directory.Exists(physicalDir)) Directory.CreateDirectory(physicalDir);

            string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{file.FileName}";
            string physicalPath = Path.Combine(physicalDir, fileName);

            // 用 file.CopyTo 代替 CopyToAsync
            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                file.CopyTo(stream); 
            }

            return "/" + Path.Combine(relativeDir, fileName).Replace("\\", "/");
        }

        // -------------------- 删除方法（可选）：用于文件更新时清理旧文件---------------------
        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            // 将数据库里的相对路径转回物理路径
            string physicalPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }

        // ----------------- 文件下载：返回物理路径 --------------------
        public string GetPhysicalPath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;

            string physicalPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));

            return File.Exists(physicalPath) ? physicalPath : null;
        }

        // ----------------- 获取文件原始名称 --------------------
        public string GetOriginalFileName(string storagePath)
        {
            if (string.IsNullOrEmpty(storagePath)) return "download";
            string fileName = Path.GetFileName(storagePath);

            if (fileName.Length > 15 && fileName[14] == '_')
            {
                return fileName.Substring(15);
            }
            return fileName;
        }
    }
}
