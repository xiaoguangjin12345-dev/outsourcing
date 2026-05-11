namespace OutsourcingApplication.Services.Interfaces
{
    public interface IFileService
    {
        // 保存方法：返回存储后的相对路径（用于存入数据库）
        string SaveFile(IFormFile file, string module, int id, string subDir = "");
        // 删除方法：用于文件更新时清理旧文件
        void DeleteFile(string relativePath);
        // 文件下载：返回物理路径
        string GetPhysicalPath(string relativePath);
        // 获取文件原始名称
        string GetOriginalFileName(string storagePath);
    }
}
