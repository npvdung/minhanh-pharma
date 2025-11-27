using Base_Asp_Core_MVC_with_Identity.Data;
using Base_Asp_Core_MVC_with_Identity.Models;
using Base_Asp_Core_MVC_with_Identity.Models.View;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;

        public HomeController(ILogger<HomeController> logger, Base_Asp_Core_MVC_with_IdentityContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(int range = 7)
        {

            var totalAmount = _context.Invoices
                                .Sum(x => x.TotalAmount ?? 0m);
            var totalReSale = _context.reSales
                                .Sum(x => x.TotalAmount ?? 0m);
            // 1. Lấy dữ liệu Invoice từ database
            var invoices = _context.Invoices.ToList();

            // 2. Khởi tạo danh sách mặc định cho 12 tháng
            var months = Enumerable.Range(1, 12).Select(m => new { Month = m, MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) }).ToList();

            // 3. Tính tổng doanh thu theo tháng
            var revenueChartData = months.Select(m =>
                invoices
                    .Where(i => i.InvoiceDate.Value.Month == m.Month && i.InvoiceDate.Value.Year == DateTime.Now.Year)
                    .Sum(i => i.TotalAmount)
            ).ToList();

            // 4. Tính số lượng người dùng theo tháng (dựa trên UserId)
            var userChartData = months.Select(m =>
                invoices
                    .Where(i => i.InvoiceDate.Value.Month == m.Month && i.InvoiceDate.Value.Year == DateTime.Now.Year)
                    .Select(i => i.UserId)
                    .Distinct()
                    .Count()
            ).ToList();
                // ====== LỌC SẢN PHẨM SẮP HẾT HẠN THEO KHOẢNG THỜI GIAN ======
            var today = DateTime.Today;

            // Chuẩn hóa khoảng thời gian (ngày) từ query
            // 7: 1 tuần, 30: 1 tháng, 90: 3 tháng, 180: 6 tháng
            int expiryRangeDays;
            DateTime endDate;

            switch (range)
            {
                case 30:
                    expiryRangeDays = 30;
                    endDate = today.AddMonths(1);
                    break;
                case 90:
                    expiryRangeDays = 90;
                    endDate = today.AddMonths(3);
                    break;
                case 180:
                    expiryRangeDays = 180;
                    endDate = today.AddMonths(6);
                    break;
                case 7:
                default:
                    expiryRangeDays = 7;
                    endDate = today.AddDays(7);
                    break;
            }

            var productDetails = (from tempdata in _context.stocks
                                join tempProduct in _context.Products on tempdata.ProductId equals tempProduct.ID.ToString() into tempTable1
                                from tb1 in tempTable1.DefaultIfEmpty()
                                join tempSupplier in _context.suppliers on tb1.SupplierId equals tempSupplier.ID.ToString() into tempTable2
                                from tb2 in tempTable2.DefaultIfEmpty()
                                where tempdata.ExpirationData.HasValue
                                        && tempdata.ExpirationData.Value.Date >= today
                                        && tempdata.ExpirationData.Value.Date <= endDate
                                        && tempdata.QuantityInStock > 0
                                orderby tempdata.ExpirationData ascending
                                select new ProductDetailViewModel
                                {
                                    ProductId = tempdata.ID.ToString(),
                                    ProductName = tb1.ProductName,
                                    QuantitySold = tempdata.QuantityInStock,
                                    ExpirationDate = tempdata.ExpirationData
                                })
                                .ToList();

                var viewModel = new DashboardViewModel
                {
                    // Thống kê
                    TotalCustomers = _context.Customers.Count(),
                    Revenue = totalAmount - totalReSale,
                    ProductsSold = _context.Invoice_Details.Select(x => x.ProductId).Count(),
                    OrdersCompleted = _context.Invoices.Count().ToString(),

                    RevenueChartData = revenueChartData,
                    UserChartData = userChartData,
                    ChartLabels = months.Select(m => m.MonthName).ToList(),

                    // Dữ liệu bảng
                    ProductDetails = productDetails,

                    // NEW: lưu khoảng thời gian đang chọn
                    ExpiryRangeDays = expiryRangeDays
                };

                return View(viewModel);

    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
}