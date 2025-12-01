using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class Import
    {
        [Key]
        public Guid ID { get; set; }

        [Required]
        [Display(Name = "Mã phiếu nhập")]
        public string ImportCode { get; set; }

        [Display(Name = "Tên thuốc nhập")]
        public string? ImportName { get; set; }

        [Display(Name = "Ngày nhập hàng")]
        public DateTime? ImportDate { get; set; }

        [Display(Name = "Người nhập")]
        public string? AccountId { get; set; }

        [Display(Name = "Nhà cung cấp")]
        public string? SupplierId { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Tổng tiền")]
        public decimal? TotalAmount { get; set; }

        [Display(Name = "Trạng thái")]
        public int Status { get; set; }
    }
}
