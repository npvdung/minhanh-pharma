using Base_Asp_Core_MVC_with_Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var customerData = from tempdata in _context.stocks
                                   join tempPorduct in _context.Products on tempdata.ProductId equals tempPorduct.ID.ToString() into tempTable1
                                   from tb1 in tempTable1.DefaultIfEmpty()
                                   join tempSupplier in _context.suppliers on tb1.SupplierId equals tempSupplier.ID.ToString() into tempTable2
                                   from tb2 in tempTable2.DefaultIfEmpty()
                                   select new
                                   {
                                       tempdata.ID,
                                       productName = tb1.ProductName,
                                       supplierName =tb2.SupplierName,
                                       tempdata.QuantityInStock,
                                       tempdata.ProductionBatch,
                                       tempdata.ManufacturingDate,
                                       tempdata.ExpirationData,
                                       tempdata.TotalValueImport,
                                       tb1.Price,
                                       tb1.Unit
                                       //quantityInStock = tempdata.QuantityInStock
                                   };
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    customerData = customerData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    customerData = customerData.Where(m => m.productName.Contains(searchValue) || m.supplierName.Contains(searchValue));
                }

                recordsTotal = customerData.Count();
                int sttCounter = skip + 1;
                var data = customerData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("GetUnits/{productId}")]
        public IActionResult GetUnits(string productId)
        {
            var getProduct = _context.stocks.Where(x => x.ID.ToString() == productId).FirstOrDefault();
            var units = _context.productUnits
                .Where(u => u.ProductId == getProduct.ProductId)
                .Select(u => new {
                    Value = u.ID,
                    text = u.UnitName
                })
                .ToList();

            return Ok(units);
        }

        [HttpGet("GetUnitDetails/{unitId}")]
        public IActionResult GetUnitDetails(string unitId)
        {
            var unit = _context.productUnits
                .Where(u => u.ID.ToString() == unitId)
                .Select(u => new
                {
                    u.Rate,
                    u.PriceBuy
                })
                .FirstOrDefault();

            if (unit == null)
            {
                return NotFound();
            }

            return Ok(unit);
        }
    }
}
