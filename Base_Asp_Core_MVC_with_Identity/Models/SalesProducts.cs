namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class SalesProducts
    {
        [Key]
        [Display(Name = "Mã định danh")]
        public Guid ID { get; set; }

        [Required]
        [Display(Name = "Hóa đơn")]
        public string SaleId { get; set; }

        [Required]
        [Display(Name = "Thuốc / Sản phẩm")]
        public string? ProductId { get; set; }

        [Required]
        [Display(Name = "Lô nhập")]
        public string? ImportId { get; set; }

        [Required]
        [Display(Name = "Người thực hiện")]
        public string? UserId { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Description { get; set; }

        [Display(Name = "Đơn vị")]
        public string? Unit { get; set; }

        [Display(Name = "Số lượng")]
        public int? Quantity { get; set; }

        [Display(Name = "Đơn giá")]
        public decimal? Price { get; set; }

        [Display(Name = "Thành tiền")]
        public decimal? TotalAmount { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Đơn vị quy đổi")]
        public string? UnitProductId { get; set; }
    }

}
