using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Base_Asp_Core_MVC_with_Identity.Models
{
    public class DisposalRecords
    {
        [Key]
        [Display(Name = "Mã phiếu hủy")]
        public Guid ID { get; set; }

        [Required]
        [Display(Name = "Lô nhập (Import ID)")]
        public string ImportId { get; set; }

        [Required]
        [Display(Name = "Ngày xuất hủy")]
        public DateTime? ExportDate { get; set; }

        [Display(Name = "Người thực hiện")]
        public string? UserId { get; set; }

        [Display(Name = "Nhà cung cấp")]
        public string? SupplierId { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Tổng tiền hủy")]
        public decimal? TotalAmount { get; set; }

        [Display(Name = "Ngày trả hàng / hoàn hàng")]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Lý do hủy")]
        public string? Reason { get; set; }
    }
}
