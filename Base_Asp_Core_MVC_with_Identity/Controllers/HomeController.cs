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

        public IActionResult Index()
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
            var productDetails = (from tempdata in _context.stocks
                                  join tempProduct in _context.Products on tempdata.ProductId equals tempProduct.ID.ToString() into tempTable1
                                  from tb1 in tempTable1.DefaultIfEmpty()
                                  join tempSupplier in _context.suppliers on tb1.SupplierId equals tempSupplier.ID.ToString() into tempTable2
                                  from tb2 in tempTable2.DefaultIfEmpty()
                                  orderby tempdata.ExpirationData ascending // Sắp xếp gần nhất
                                  select new ProductDetailViewModel
                                  {
                                      ProductId = tempdata.ID.ToString(),
                                      ProductName = tb1.ProductName,
                                      QuantitySold = tempdata.QuantityInStock, // Nếu đây là trường cần hiển thị
                                      ExpirationDate = tempdata.ExpirationData, // Ngày hết hạn
                                  })
                     .Where(x => x.QuantitySold > 0)
                     .Take(10) // Lấy 10 bản ghi
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