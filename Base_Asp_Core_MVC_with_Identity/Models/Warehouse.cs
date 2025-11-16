using System.ComponentModel.DataAnnotations;

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class Warehouse
    {
        [Key]
        public Guid ID { get; set; }
        public string ProductId { get; set; }
        [Display(Name = "Số lượng")]
        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        public int? QuantityInStock { get; set;}
        [Display(Name = "Ngày nhập hàng")]
        [Required(ErrorMessage = "Ngày nhập hàng là bắt buộc.")]
        public DateTime? ProductionBatch { get; set;}
        [Display(Name = "Ngày sản xuất")]
        [Required(ErrorMessage = "Ngày sản xuất là bắt buộc.")]
        public DateTime? ManufacturingDate { get; set;}
        [Display(Name = "Ngày hết hạn")]
        [Required(ErrorMessage = "Ngày hết hạn là bắt buộc.")]
        public DateTime? ExpirationData { get;set;}
        public string? UnitStock { get; set; }
        public string? UnitSaleMin { get; set; }
        public double? Conversion_rate { get; set; }
        public string? Location { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? Note { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? TotalValueImport { get; set; }
    }
}
