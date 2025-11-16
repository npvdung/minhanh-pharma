namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class Sales
    {
        [Key]
        public Guid ID { get; set; }

        [Required]
        public string InvoiceCode { get; set; }
        public string? UserId { get; set; }
        public string? Description { get; set; }
        public DateTime? InvoiceDate { get; set; }
        //public int Quantity { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? CustomerId { get; set; }
    }
}
