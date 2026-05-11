using System.ComponentModel.DataAnnotations;

namespace OutsourcingApplication.Models
{
    public class DictTag
    {
        [Key]
        public int TagID { get; set; }
        public string TagName { get; set; }
    }
}
