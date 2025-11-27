using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class DisposalProducts
    {
        [Key]
        [Display(Name = "Mã định danh")]
        public Guid ID { get; set; }

        [Required]
        [Display(Name = "Phiếu hủy")]
        public string DisposalRecordsId { get; set; }

        [Required]
        [Display(Name = "Sản phẩm")]
        public string? ProductId { get; set; }

        [Display(Name = "Ghi chú / Tỉ lệ")]
        public string? Description { get; set; }

        [Display(Name = "Ngày hủy")]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Đơn vị")]
        public string? Unit { get; set; }

        [Display(Name = "Số lượng hủy")]
        public int? Quantity { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Đơn giá")]
        public decimal? Price { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Giá nhập (nếu dùng)")]
        public decimal? ImportPrice { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Thành tiền")]
        public decimal? TotalAmount { get; set; }

        [Display(Name = "Đơn vị quy đổi")]
        public string? UnitProductId { get; set; }

        [Display(Name = "Lô nhập (ID lô trong kho)")]
        public string? ImportId { get; set; }
    }
}
