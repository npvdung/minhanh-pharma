
namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class Import
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public string ImportCode { get; set; }
        [Required]
        public string ImportName { get; set; }
        public DateTime? ImportDate { get; set; }
        public string? AccountId { get; set; }
        public string? SupplierId { get; set; }
        public string? Description { get; set; }
        public decimal? TotalAmount { get; set; }
        public int Status { get; set; }
    }
}
