namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class SalesProducts
    {
        [Key]
        public Guid ID { get; set; }

        [Required]
        public string SaleId { get; set; }
        [Required]
        public string? ProductId { get; set; }
        [Required]
        public string? ImportId { get; set; }
        [Required]
        public string? UserId { get; set; }
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UnitProductId { get; set; }
    }
}
