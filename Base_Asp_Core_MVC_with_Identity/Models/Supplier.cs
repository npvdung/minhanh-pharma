using MessagePack;
using System.ComponentModel.DataAnnotations.Schema;


namespace Base_Asp_Core_MVC_with_Identity.Models
{
     
    public class Supplier
    {
        [Key]
        public Guid ID { get; set; }
         
        

        [Display(Name = "Mã nhà cung cấp")]
        [Required(ErrorMessage = "Mã nhà cung cấp là bắt buộc.")]
        public string SupplierCode { get; set; }

        [Display(Name = "Tên nhà cung cấp")]
        [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc.")]
        public string SupplierName { get; set; }
        [Display(Name = "Địa chỉ nhà cung cấp")]
        public string? Address { get; set; }
        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        public int? PhoneNumber { get; set; }
        [Display(Name = "Mô tả")]
        public string? Description { get; set;}
        [Display(Name = "Email nhà cung cấp")]
        public string? Email { get; set; }
    }
}
