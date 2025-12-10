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
            var year = DateTime.Now.Year;

            // Tổng doanh thu toàn bộ (đã trừ trả hàng) – giữ nguyên
            var totalAmount = _context.Invoices
                                .Sum(x => x.TotalAmount ?? 0m);
            var totalReSale = _context.reSales
                                .Sum(x => x.TotalAmount ?? 0m);

            // Lấy dữ liệu theo NĂM hiện tại cho biểu đồ
            var invoices = _context.Invoices
                                .Where(i => i.InvoiceDate.HasValue &&
                                            i.InvoiceDate.Value.Year == year)
                                .ToList();

            var reSales = _context.reSales
                                .Where(r => r.InvoiceDate.HasValue &&
                                            r.InvoiceDate.Value.Year == year)
                                .ToList();

            var months = Enumerable.Range(1, 12)
                .Select(m => new
                {
                    Month = m,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)
                })
                .ToList();

            // === 3. Doanh thu theo tháng: HĐ bán - HĐ trả ===
            List<decimal?> revenueChartData = months.Select(m =>
            {
                decimal invoiceSum = invoices
                    .Where(i => i.InvoiceDate.Value.Month == m.Month)
                    .Sum(i => i.TotalAmount ?? 0m);

                decimal reSaleSum = reSales
                    .Where(r => r.InvoiceDate.Value.Month == m.Month)
                    .Sum(r => r.TotalAmount ?? 0m);

                // cast sang decimal? để khớp với DashboardViewModel
                return (decimal?)(invoiceSum - reSaleSum);
            }).ToList();

            // === 4. Khách hàng theo tháng (dựa trên CustomerId trong hóa đơn) ===
            // -> số KH có mua hàng trong tháng đó
            var userChartData = months.Select(m =>
                invoices
                    .Where(i => i.InvoiceDate.Value.Month == m.Month)
                    .Select(i => i.CustomerId)
                    .Distinct()
                    .Count()
            ).ToList();

            // ====== phần lọc sản phẩm sắp hết hạn giữ nguyên ======
            var today = DateTime.Today;

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
                                join tempProduct in _context.Products
                                    on tempdata.ProductId equals tempProduct.ID.ToString() into tempTable1
                                from tb1 in tempTable1.DefaultIfEmpty()
                                join tempSupplier in _context.suppliers
                                    on tb1.SupplierId equals tempSupplier.ID.ToString() into tempTable2
                                from tb2 in tempTable2.DefaultIfEmpty()
                                where tempdata.ExpirationData.HasValue
                                        && tempdata.ExpirationData.Value.Date >= today
                                        && tempdata.ExpirationData.Value.Date <= endDate
                                        && tempdata.QuantityInStock > 0
                                orderby tempdata.ExpirationData ascending
                                select new ProductDetailViewModel
                                {
                                    ProductId      = tempdata.ID.ToString(),
                                    ProductName    = tb1.ProductName,
                                    QuantitySold   = tempdata.QuantityInStock,
                                    ExpirationDate = tempdata.ExpirationData,
                                    BatchCode      = tempdata.BatchCode
                                })
                                .ToList();

            var viewModel = new DashboardViewModel
            {
                // Thống kê thẻ trên
                TotalCustomers = _context.Customers.Count(),           // tổng KH
                Revenue        = totalAmount - totalReSale,            // tổng doanh thu đã trừ trả hàng
                ProductsSold   = _context.Invoice_Details.Select(x => x.ProductId).Count(),
                OrdersCompleted = _context.Invoices.Count().ToString(),

                // Dữ liệu biểu đồ
                RevenueChartData = revenueChartData,
                UserChartData    = userChartData,
                ChartLabels      = months.Select(m => m.MonthName).ToList(),

                // Bảng sắp hết hạn
                ProductDetails   = productDetails,
                ExpiryRangeDays  = expiryRangeDays
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