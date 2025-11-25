using MessagePack;
using System.ComponentModel.DataAnnotations;

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class Product
    {
        [Key]
        [Display(Name = "Mã định danh")]
        public Guid ID { get; set; }

        [Display(Name = "Mã thuốc / sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập mã thuốc.")]
        public string ProductCode { get; set; }

        [Display(Name = "Tên thuốc / sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập tên thuốc.")]
        public string ProductName { get; set; }

        [Display(Name = "Loại thuốc")]
        public string? CategoryId { get; set; }

        [Display(Name = "Nhà cung cấp")]
        public string? SupplierId { get; set; }

        [Display(Name = "Đơn vị bán lẻ nhỏ nhất")]
        public string? Unit { get; set; }

        [Display(Name = "Giá bán")]
        public double? Price { get; set; }

        [Display(Name = "Thành phần")]
        public string? Ingredient { get; set; }

        [Display(Name = "Cách đóng gói")]
        public string? Content { get; set; }

        [Display(Name = "Công dụng")]
        public string? Uses { get; set; }

        [Display(Name = "Hướng dẫn sử dụng")]
        public string? UserManual { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }
    }
}
