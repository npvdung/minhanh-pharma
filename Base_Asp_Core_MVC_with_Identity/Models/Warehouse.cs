using System.ComponentModel.DataAnnotations;

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class Warehouse
    {
        [Key]
        [Display(Name = "Mã định danh")]
        public Guid ID { get; set; }

        [Display(Name = "Sản phẩm")]
        public string ProductId { get; set; }
        [Display(Name = "Mã lô nhập")]
        public string? BatchCode { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Display(Name = "Số lượng tồn kho")]
        public int? QuantityInStock { get; set; }

        [Required(ErrorMessage = "Ngày nhập là bắt buộc.")]
        [Display(Name = "Ngày nhập hàng")]
        public DateTime? ProductionBatch { get; set; }

        [Required(ErrorMessage = "Ngày sản xuất là bắt buộc.")]
        [Display(Name = "Ngày sản xuất")]
        public DateTime? ManufacturingDate { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn là bắt buộc.")]
        [Display(Name = "Ngày hết hạn")]
        public DateTime? ExpirationData { get; set; }

        [Display(Name = "Đơn vị tồn kho")]
        public string? UnitStock { get; set; }

        [Display(Name = "Đơn vị bán nhỏ nhất")]
        public string? UnitSaleMin { get; set; }

        [Display(Name = "Tỷ lệ quy đổi")]
        public double? Conversion_rate { get; set; }

        [Display(Name = "Vị trí lưu kho")]
        public string? Location { get; set; }

        [Display(Name = "Cập nhật cuối")]
        public DateTime? LastUpdated { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Display(Name = "Giá trị nhập")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? TotalValueImport { get; set; }
    }

}
