using System.ComponentModel.DataAnnotations.Schema;

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class DisposalProducts
    {
        [Key]
        public Guid ID { get; set; }
        
        [Required]
        public string DisposalRecordsId { get; set; }
        [Required] 
        public string? ProductId { get; set; }
        public string? Description { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Unit { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? ImportPrice { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? UnitProductId { get; set; }
        [NotMapped]
        public string? ImportId { get; set; }

    }
}
