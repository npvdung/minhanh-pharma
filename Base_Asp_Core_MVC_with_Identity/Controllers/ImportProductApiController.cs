using Base_Asp_Core_MVC_with_Identity.CommonFile.Enum;
using Base_Asp_Core_MVC_with_Identity.Data;
using Base_Asp_Core_MVC_with_Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportProductApiController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private readonly UserManager<UserSystemIdentity> _uid;

        public ImportProductApiController(Base_Asp_Core_MVC_with_IdentityContext context,
            UserManager<UserSystemIdentity> uid)
        {
            _context = context;
            _uid = uid;
        }

        [HttpGet]
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                // --------- DataTables Params ------------
                var draw = Request.Query["draw"].FirstOrDefault();
                var start = Request.Query["start"].FirstOrDefault();
                var length = Request.Query["length"].FirstOrDefault();
                var sortColumn = Request.Query["columns[" + Request.Query["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortDirection = Request.Query["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Query["search[value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 10;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // ************** QUERY CHÍNH - CÁCH 2 **************
                var query =
                    from imp in _context.ImportsProduct
                    join d in _context.ImportProductDetails
                        on imp.ID.ToString() equals d.ImportProductId into gd
                    from d in gd.DefaultIfEmpty()
                    join p in _context.Products
                        on d.ProduceId equals p.ID.ToString() into gp
                    from p in gp.DefaultIfEmpty()
                    join s in _context.suppliers
                        on p.SupplierId equals s.ID.ToString() into gs
                    from s in gs.DefaultIfEmpty()
                    group new { imp, d, p, s } by new
                    {
                        imp.ID,
                        imp.ImportCode,
                        imp.ImportName,
                        imp.ImportDate,
                        imp.TotalAmount,
                        imp.Status
                    }
                    into g
                    select new
                    {
                        id = g.Key.ID,
                        importCode = g.Key.ImportCode,
                        importName = g.Key.ImportName,
                        importDate = g.Key.ImportDate,
                        totalAmount = g.Key.TotalAmount,

                        // Lấy tên thuốc đầu tiên của phiếu nhập
                        productName = g.Select(x => x.p.ProductName)
                                        .Where(x => x != null)
                                        .FirstOrDefault(),

                        // Lấy MÃ LÔ đầu tiên
                        batchCode = g.Select(x => x.d.BatchCode)
                                    .Where(x => x != null)
                                    .FirstOrDefault(),

                        // Lấy nhà cung cấp đúng từ bảng Supplier
                        supplierName = g.Select(x => x.s.SupplierName)
                                        .Where(x => !string.IsNullOrEmpty(x))
                                        .Distinct()
                                        .FirstOrDefault() ?? "",

                        status = g.Key.Status,
                        approvedValue = (int)EnumApprodImport.approved
                    };

                // ---- Search ----
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(x =>
                        x.importCode.Contains(searchValue) ||
                        x.importName.Contains(searchValue) ||
                        x.productName.Contains(searchValue) ||
                        x.batchCode.Contains(searchValue) ||
                        x.supplierName.Contains(searchValue)
                    );
                }

                // ---- Sort ----
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = query.OrderBy($"{sortColumn} {sortDirection}");
                }

                // ---- Paging ----
                int recordsTotal = query.Count();
                var data = query.Skip(skip).Take(pageSize).ToList();

                return Ok(new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal = recordsTotal,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi server API ImportProductApi: " + ex.Message);
            }
        }

        // ========================= UNITS API =========================

        // 1. Lấy danh sách đơn vị theo LÔ (stockId)
        [HttpGet("GetUnitsByStock/{stockId}")]
        public IActionResult GetUnitsByStock(string stockId)
        {
            var stock = _context.stocks
                .FirstOrDefault(s => s.ID.ToString() == stockId);
            if (stock == null)
            {
                return NotFound();
            }

            // Lấy các đơn vị của thuốc tương ứng ProductId
            var units = _context.productUnits
                .Where(u => u.ProductId == stock.ProductId)
                .Select(u => new
                {
                    value = u.ID.ToString(),
                    text = u.UnitName
                })
                .ToList();

            return Ok(units);
        }

        // (Cũ) Lấy danh sách đơn vị theo ProductId – giữ lại để chỗ khác còn dùng
        [HttpGet("GetUnits/{productId}")]
        public IActionResult GetUnits(string productId)
        {
            var units = _context.productUnits
                .Where(u => u.ProductId == productId)
                .Select(u => new
                {
                    value = u.ID.ToString(),
                    text = u.UnitName
                })
                .ToList();

            return Ok(units);
        }

        // (Cũ) Lấy tỉ lệ + giá mặc định theo đơn vị (không theo lô)
        [HttpGet("GetUnitDetails/{unitId}")]
        public IActionResult GetUnitDetails(string unitId)
        {
            var unit = _context.productUnits
                .Where(u => u.ID.ToString() == unitId)
                .Select(u => new
                {
                    rate = u.Rate,
                    price = u.PriceBuy
                })
                .FirstOrDefault();

            if (unit == null)
            {
                return NotFound();
            }

            return Ok(unit);
        }

        // 2. MỚI: Lấy tỉ lệ + GIÁ NHẬP THEO LÔ + ĐƠN VỊ
        //    URL: /api/ImportProductApi/GetUnitDetailsByStock?stockId=...&unitId=...
        [HttpGet("GetUnitDetailsByStock")]
        public IActionResult GetUnitDetailsByStock(string stockId, string unitId)
        {
            // 1. Tìm lô trong bảng stocks
            var stock = _context.stocks
                .FirstOrDefault(s => s.ID.ToString() == stockId);

            if (stock == null)
            {
                return NotFound("Stock not found");
            }

            // 2. Lấy đúng chi tiết nhập theo:
            //   ProduceId        == stock.ProductId
            //   BatchCode        == stock.BatchCode
            //   UnitProductId    == unitId
            var detail = (from d in _context.ImportProductDetails
                          where d.ProduceId == stock.ProductId
                                && d.BatchCode == stock.BatchCode
                                && d.UnitProductId == unitId
                          orderby d.ProductionBatch descending
                          select new
                          {
                              rate = d.ConvertRate,
                              price = d.ImportPrice
                          })
                        .FirstOrDefault();

            // Nếu tìm được --> trả về giá đúng của LÔ
            if (detail != null)
            {
                return Ok(detail);
            }

            // 3. Fallback: nếu không tìm được chi tiết nhập theo lô
            var unit = _context.productUnits
                .Where(u => u.ID.ToString() == unitId)
                .Select(u => new
                {
                    rate = u.Rate,
                    price = u.PriceBuy
                })
                .FirstOrDefault();

            if (unit == null)
            {
                return NotFound("Unit not found");
            }

            return Ok(unit);
        }
    }
}
