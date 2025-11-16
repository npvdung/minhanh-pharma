

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class Category
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public string CategoryCode { get; set; }
        [Required]
        public string CategoryName { get; set; }

        public string? Description { get; set; }
    }
}
