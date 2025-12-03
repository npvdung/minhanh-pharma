using Base_Asp_Core_MVC_with_Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockApiController : ControllerBase
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private readonly UserManager<UserSystemIdentity> _uid;

        public StockApiController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> uid)
        {
            _uid = uid;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var draw = Request.Query["draw"].FirstOrDefault();
                var start = Request.Query["start"].FirstOrDefault();
                var length = Request.Query["length"].FirstOrDefault();

                var sortColumn = Request.Query["columns[" + Request.Query["order[0][column]"].FirstOrDefault() + "][name]"]
                                     .FirstOrDefault();
                var sortColumnDirection = Request.Query["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Query["search[value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // ============================
                // JOIN đầy đủ: Stocks + Product + Supplier + ImportDetail + Import (ImportCode)
                // ============================
                var query =
                    from stock in _context.stocks

                    // Product
                    join product in _context.Products
                        on stock.ProductId equals product.ID.ToString() into prodJoin
                    from prod in prodJoin.DefaultIfEmpty()

                    // Supplier
                    join supplier in _context.suppliers
                        on prod.SupplierId equals supplier.ID.ToString() into supJoin
                    from sup in supJoin.DefaultIfEmpty()

                    // Import detail: match theo ProductId + BatchCode
                    join detail in _context.ImportProductDetails
                        on new { ProdId = stock.ProductId, Batch = stock.BatchCode }
                        equals new { ProdId = detail.ProduceId, Batch = detail.BatchCode } into detailJoin
                    from detail in detailJoin.DefaultIfEmpty()

                    // Import master: lấy ImportCode
                    join imp in _context.ImportsProduct
                        on detail.ImportProductId equals imp.ID.ToString() into impJoin
                    from imp in impJoin.DefaultIfEmpty()

                    select new
                    {
                        id = stock.ID,

                        // Mã nhập hàng
                        importCode = imp != null ? imp.ImportCode : string.Empty,

                        productName = prod.ProductName,
                        batchCode = stock.BatchCode,
                        supplierName = sup.SupplierName,
                        quantityInStock = stock.QuantityInStock,
                        expirationData = stock.ExpirationData,
                        manufacturingDate = stock.ManufacturingDate,
                        unitPack = prod.Unit,
                        price = prod.Price
                    };

                // SORT
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                // SEARCH
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        m.importCode.Contains(searchValue) || 
                        m.productName.Contains(searchValue) ||
                        m.supplierName.Contains(searchValue) ||
                        m.batchCode.Contains(searchValue)
                    );
                }

                int recordsTotal = query.Count();
                var data = query.Skip(skip).Take(pageSize).ToList();

                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal = recordsTotal,
                    data = data
                };

                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
