namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class ReSales
    {
        [Key]
        
        public Guid ID { get; set; }

        [Required]
        [Display(Name = "Mã phiếu trả")]
        
        public string Sales { get; set; }

        [Display(Name = "Người xử lý")]
        public string? UserId { get; set; }

        [Display(Name = "Hóa đơn gốc")]
        public string? Description { get; set; }

        [Display(Name = "Lý do trả hàng")]
        public string? Reason { get; set; }

        [Display(Name = "Ngày trả hàng")]
        public DateTime? InvoiceDate { get; set; }

        [Display(Name = "Tổng tiền trả lại")]
        public decimal? TotalAmount { get; set; }

        [Display(Name = "Khách hàng")]
        public string? CustomerId { get; set; }
    }

}
