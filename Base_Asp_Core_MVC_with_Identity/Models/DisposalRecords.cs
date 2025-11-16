namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class DisposalRecords
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public string ImportId { get; set; }
        [Required]
        public DateTime? ExportDate { get; set; }
        public string? UserId { get; set; }
        public string? SupplierId { get; set; }
        public string? Description { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Reason { get; set; }


    }
}
