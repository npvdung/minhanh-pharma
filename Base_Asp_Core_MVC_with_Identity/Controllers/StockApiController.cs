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
                // JOIN đầy đủ, BỔ SUNG BatchCode
                // ============================
                var query =
                    from stock in _context.stocks
                    join product in _context.Products on stock.ProductId equals product.ID.ToString() into prodJoin
                    from prod in prodJoin.DefaultIfEmpty()

                    join supplier in _context.suppliers on prod.SupplierId equals supplier.ID.ToString() into supJoin
                    from sup in supJoin.DefaultIfEmpty()

                    select new
                    {
                        id = stock.ID,
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
