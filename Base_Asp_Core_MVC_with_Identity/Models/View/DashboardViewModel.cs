using System.ComponentModel.DataAnnotations.Schema;

namespace Base_Asp_Core_MVC_with_Identity.Models.View
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal Revenue { get; set; }
        public int ProductsSold { get; set; }
        public string OrdersCompleted { get; set; }
        public List<decimal?> RevenueChartData { get; set; }
        public List<int> UserChartData { get; set; }
        public List<string> ChartLabels { get; set; }
        public List<ProductDetailViewModel> ProductDetails { get; set; }
        public int ExpiryRangeDays { get; set; }

    }

    public class ProductDetailViewModel
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int? QuantitySold { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal? Price { get; set; }
        public string BatchCode { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
