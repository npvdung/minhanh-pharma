using Base_Asp_Core_MVC_with_Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceApiController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private readonly UserManager<UserSystemIdentity> _uid;

        public InvoiceApiController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> uid)
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

                // ==== Lấy thêm tham số lọc ngày tạo ====
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

                var customerData =
                    from tempcustomer in _context.Invoices
                    join tempCutomer in _context.Customers
                        on tempcustomer.CustomerId equals tempCutomer.ID.ToString() into tempTable1
                    from tb1 in tempTable1.DefaultIfEmpty()
                    select new
                    {
                        tempcustomer.ID,
                        tempcustomer.InvoiceCode,
                        tempcustomer.InvoiceDate,
                        customName = tb1.FullName,
                        tempcustomer.TotalAmount
                    };

                // ==== Áp dụng filter ngày tạo (nếu có) ====
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
                        m.InvoiceCode.Contains(searchValue) ||
                        m.customName.Contains(searchValue));
                }

                recordsTotal = customerData.Count();
                int sttCounter = skip + 1;
                var data = customerData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
