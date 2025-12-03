using Base_Asp_Core_MVC_with_Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using System.Globalization;


namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReSalseApiController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private readonly UserManager<UserSystemIdentity> _uid;

        public ReSalseApiController(
            Base_Asp_Core_MVC_with_IdentityContext context,
            UserManager<UserSystemIdentity> uid)
        {
            _uid = uid;
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                var draw = Request.Query["draw"].FirstOrDefault();
                var start = Request.Query["start"].FirstOrDefault();
                var length = Request.Query["length"].FirstOrDefault();
                var sortColumn = Request.Query["columns[" + Request.Query["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Query["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Query["search[value]"].FirstOrDefault();
                // ==== Lấy thêm tham số lọc ngày trả ====
                var fromDateStr = Request.Query["fromDate"].FirstOrDefault();
                var toDateStr   = Request.Query["toDate"].FirstOrDefault();
                DateTime? fromDate = null;
                DateTime? toDate   = null;

                if (!string.IsNullOrWhiteSpace(fromDateStr) &&
                    DateTime.TryParse(fromDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var f))
                {
                    fromDate = f.Date;
                }

                if (!string.IsNullOrWhiteSpace(toDateStr) &&
                    DateTime.TryParse(toDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
                {
                    toDate = t.Date;
                }


                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // ====== LẤY MASTER RETURN + TÊN KHÁCH HÀNG ======
                var customerData = from r in _context.reSales
                                   join c in _context.Customers
                                       on r.CustomerId equals c.ID.ToString() into tempTable1
                                   from tb1 in tempTable1.DefaultIfEmpty()
                                   select new
                                   {
                                       r.ID,
                                       r.Sales,
                                       r.InvoiceDate,
                                       r.Description,         // đang lưu ID hóa đơn gốc
                                       r.Reason,              // LÍ DO TRẢ HÀNG (text)
                                       customName = tb1.FullName,
                                       r.TotalAmount
                                   };
// ==== Áp dụng filter theo khoảng Ngày trả (InvoiceDate) ====
                if (fromDate.HasValue)
                {
                    var fDate = fromDate.Value;
                    customerData = customerData.Where(m => m.InvoiceDate >= fDate);
                }

                if (toDate.HasValue)
                {
                    // Lấy đến hết ngày (23:59:59)
                    var tDate = toDate.Value.AddDays(1).AddTicks(-1);
                    customerData = customerData.Where(m => m.InvoiceDate <= tDate);
                }

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    customerData = customerData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    customerData = customerData.Where(m =>
                        m.Sales.Contains(searchValue) ||
                        m.customName.Contains(searchValue) ||
                        m.Reason.Contains(searchValue));
                }

                recordsTotal = customerData.Count();
                var pageData = customerData.Skip(skip).Take(pageSize).ToList();

                // Lấy danh sách ID phiếu trả trên trang hiện tại
                var masterIds = pageData.Select(p => p.ID.ToString()).ToList();

                // Tạo dictionary: SaleId -> danh sách mã lô
                var batchDict = _context.reSalesDetail
                    .Where(d => masterIds.Contains(d.SaleId))
                    .Join(_context.stocks,
                        d => d.ImportId,          // ID lô lưu trong chi tiết trả
                        s => s.ID.ToString(),     // ID của Warehouse (stocks)
                        (d, s) => new
                        {
                            d.SaleId,
                            s.BatchCode
                        })
                    .AsEnumerable()
                    .GroupBy(x => x.SaleId)
                    .ToDictionary(
                        g => g.Key,
                        g => string.Join(", ", g.Select(x => x.BatchCode).Distinct())
                    );

                // Build dữ liệu trả về cho DataTables
                var resultData = pageData.Select(item => new
                {
                    id = item.ID,
                    sales = item.Sales,
                    invoiceDate = item.InvoiceDate,
                    description = item.Description, // ID hóa đơn gốc (nếu cần dùng sau này)
                    reason = item.Reason,           // LÍ DO TRẢ HÀNG
                    customName = item.customName,
                    totalAmount = item.TotalAmount,
                    batchCode = batchDict.ContainsKey(item.ID.ToString())
                        ? batchDict[item.ID.ToString()]
                        : string.Empty
                }).ToList();

                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal = recordsTotal,
                    data = resultData
                };

                return Ok(jsonData);
            }
            catch
            {
                throw;
            }
        }

        // GET: api/ReSalseApi/GetInvoiceForReturn/{invoiceId}
        [HttpGet("GetInvoiceForReturn/{invoiceId}")]
        public IActionResult GetInvoiceForReturn(Guid invoiceId)
        {
            // Lấy hóa đơn bán
            var invoice = _context.Invoices.FirstOrDefault(x => x.ID == invoiceId);
            if (invoice == null)
            {
                return NotFound();
            }

            // Lấy chi tiết hóa đơn bán
            var details = _context.Invoice_Details
                .Where(d => d.SaleId == invoice.ID.ToString())
                .Select(d => new
                {
                    productStockId = d.ImportId,      // ID lô trong Warehouse (stocks)
                    unitProductId = d.UnitProductId, // Đơn vị đã bán
                    quantity = d.Quantity ?? 0,
                    price = d.Price ?? 0,            // Giá bán (để dự phòng)
                    totalAmount = d.TotalAmount ?? 0
                })
                .ToList();

            var result = new
            {
                customerId = invoice.CustomerId,
                userId = invoice.UserId,
                invoiceDate = invoice.InvoiceDate,
                totalAmount = invoice.TotalAmount ?? 0,
                details = details
            };

            return Ok(result);
        }

    }
}
