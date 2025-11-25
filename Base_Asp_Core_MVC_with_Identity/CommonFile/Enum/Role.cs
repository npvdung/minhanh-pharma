namespace Base_Asp_Core_MVC_with_Identity.CommonFile.Enum
{
    public enum Roles
    {
        Admin,
        ManagerCategory,
        ManagerImport,
        ManagerReturn,
        ManagerStock,
        ViewReport,
        ManagerSales,
        Employee,
        Viewer
    }
    public enum Status
    {
        [Display(Name = "Chờ")]
        Process,
        [Display(Name = "Từ chối")]
        Rejected,
        [Display(Name = "Xác nhận")]
        Approved,
        [Display(Name = "Huỷ")]
        Cancelled
    }
    public enum EnumApprodImport
    {
        [Display(Name = "Đang chờ")]
        Wait,
        [Display(Name = "Đã duyệt")]
        approved
    }
}
