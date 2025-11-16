namespace Base_Asp_Core_MVC_with_Identity.Models.View
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public decimal Revenue { get; set; }
        public int ProductsSold { get; set; }
        public string OrdersCompleted { get; set; }
        public List<decimal?> RevenueChartData { get; set; }
        public List<int> UserChartData { get; set; }
        public List<string> ChartLabels { get; set; }
        public List<ProductDetailViewModel> ProductDetails { get; set; }

    }

    public class ProductDetailViewModel
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int? QuantitySold { get; set; }
        public decimal? Price { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
