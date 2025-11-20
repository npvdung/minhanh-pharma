namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class Sales
    {
        [Key]
        [Display(Name = "Mã hóa đơn")]
        public Guid ID { get; set; }

        [Required]
        [Display(Name = "Số hóa đơn")]
        public string InvoiceCode { get; set; }

        [Display(Name = "Người bán")]
        public string? UserId { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Description { get; set; }

        [Display(Name = "Ngày lập hóa đơn")]
        public DateTime? InvoiceDate { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal? TotalAmount { get; set; }

        [Display(Name = "Khách hàng")]
        public string? CustomerId { get; set; }
    }

}
