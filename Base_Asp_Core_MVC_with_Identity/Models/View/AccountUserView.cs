using System.Xml.Linq;

namespace Base_Asp_Core_MVC_with_Identity.Models.View
{
    public class AccountUserView
    {
        [Required]
        public Guid Id { get; set; }
        [Display(Name = "Tên Nhân viên")]
        [Required(ErrorMessage = "Tên Nhân viên là bắt buộc.")]
        public string FirstName { get; set; }
        [Display(Name = "Chức vụ")]
        [Required(ErrorMessage = "Chức vụ là bắt buộc.")]
        public string LastName { get; set; }
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail không hợp lệ")]
        public string Email { get; set; }
        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
