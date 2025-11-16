using MessagePack;

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class Product
    {
        [Key]
        public Guid ID { get; set; }
        //[Display(Name = "Mã sản phẩm")]
        //[Required(ErrorMessage = "Mã sản phẩm là bắt buộc.")]
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string? CategoryId { get; set; }
        public string? SupplierId { get; set; }
        public string? Unit { get; set; }
        public double? Price { get; set; }
        public string? Ingredient { get; set; }
        public string? Content { get; set; }
        public string? Uses { get; set; }
        public string? UserManual { get; set; }
        public string? Note { get; set; }

    }
}
