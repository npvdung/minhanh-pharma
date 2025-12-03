using System.ComponentModel.DataAnnotations.Schema;

namespace Base_Asp_Core_MVC_with_Identity.Models
{
     
    public class ImportProducts
    {
        [Key]
        public Guid ID { get; set; }
         
                public string? ImportProductId { get; set; }
        [Required]
        [Display(Name = "Tên sản phẩm")]
        public string? ProduceId { get; set; }
        public string? Description { get; set; }
        public DateTime? ProductionBatch { get; set; }
        [Required]
        public DateTime? ManufacturingDate { get; set; }
        [Required]
        public DateTime? ExpirationData { get; set; }
        public string? Unit { get; set; }
        public double? ConvertRate { get; set; }
        [Required]
        public int? Quantity { get; set; }
        [NotMapped]
        [Column(TypeName = "decimal(18,0)")]
        public decimal? Price { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? ImportPrice { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalAmount { get; set; }
        public string? BatchCode { get; set; }
        public string? UnitProductId { get; set; }
    }
}
