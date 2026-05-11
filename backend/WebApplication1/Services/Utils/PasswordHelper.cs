namespace OutsourcingApplication.Services.Utils
{
    public static class PasswordHelper
    {
        // 加密：将明文密码转换为不可逆的哈希字符串
        public static string HashPassword(string password)
        {
            // BCrypt 会自动生成盐（Salt）并混入哈希值中
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            return hashedPassword;
        }

        // 校验：检查明文密码与数据库里的哈希值是否匹配
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // BCrypt 内部会提取 hashedPassword 里的盐来处理明文，再进行比对
            bool isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            return isValid;
        }
    }
}
