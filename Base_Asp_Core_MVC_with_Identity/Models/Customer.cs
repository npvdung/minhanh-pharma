namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class Customer
    {
        [Key]
        public Guid ID { get; set; }

        //[Display(Name = "Mã khách hàng")]
        //[Required(ErrorMessage = "Mã khách hàng là bắt buộc.")]
        public string CustomerCode { get; set; }

        //[Display(Name = "Tên khách hàng")]
        //[Required(ErrorMessage = "Tên khách hàng là bắt buộc.")]
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        //[Display(Name = "Số điện thoại")]
        //[Required(ErrorMessage = "Số điện thoại khách hàng là bắt buộc.")]
        public string? PhoneNumber { get; set; }
        //[Display(Name = "Địa chỉ")]
        //[Required(ErrorMessage = "Địa chỉ khách hàng là bắt buộc.")]
        public string? Address { get; set; }
        //[Display(Name = "Email")]
        //[Required(ErrorMessage = "Email khách hàng là bắt buộc.")]
        public string? Email { get; set; }
        //[Display(Name = "Mô tả")]
        public string? Desciption { get; set; }
    }
}
