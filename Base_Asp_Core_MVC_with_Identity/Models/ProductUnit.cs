using MessagePack;

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class ProductUnit
    {
        [Key]
        public Guid ID { get; set; }
        public string ProductId { get; set; }
        public string UnitName { get; set; }
        public int? Rate { get; set; }
        public decimal? PriceBuy { get; set; }
        public string? Contain { get; set; }
        public decimal? PriceSell { get; set; }
    }
}
