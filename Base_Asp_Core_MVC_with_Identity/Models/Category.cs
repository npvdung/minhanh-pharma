

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class Category
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        [Display(Name = "Mã loại thuốc")]
        public string CategoryCode { get; set; }
        [Required]
        [Display(Name = "Tên loại thuốc")]
        public string CategoryName { get; set; }
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
    }
}
