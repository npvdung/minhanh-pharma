using MessagePack;
using System.ComponentModel.DataAnnotations;

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class ProductUnit
    {
        [Key]
        [Display(Name = "Mã định danh")]
        public Guid ID { get; set; }

        [Display(Name = "Thuốc / Sản phẩm")]
        public string? ProductId { get; set; }

        [Display(Name = "Tên đơn vị (VD: Hộp, Vỉ, Viên...)")]
        public string UnitName { get; set; }

        [Display(Name = "Tỷ lệ quy đổi (so với đơn vị nhỏ nhất)")]
        public int? Rate { get; set; }

        [Display(Name = "Giá nhập")]
        public decimal? PriceBuy { get; set; }

        [Display(Name = "Quy cách đóng gói")]
        public string? Contain { get; set; }

        [Display(Name = "Giá bán")]
        public decimal? PriceSell { get; set; }
    }
}
