namespace OutsourcingApplication.DTOs.Common
{
    public class SelectOptionDto
    {
        // 选项实际的值（对应数据库里的ID或类别编码）
        public string Value { get; set; } = null!;

        // 选项显示的标签
        public string Label { get; set; } = null!;

        // 无参构造函数，用于反序列化
        public SelectOptionDto() { }

        // 全参构造函数，便于程序编写
        public SelectOptionDto(string value, string label)
        {
            Value = value;
            Label = label;
        }
    }
}
