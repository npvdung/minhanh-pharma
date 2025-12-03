using Base_Asp_Core_MVC_with_Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private readonly UserManager<UserSystemIdentity> _uid;

        public ProductApiController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> uid)
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

                var customerData = from tempcustomer in _context.Products
                                   join tempsupllier in _context.suppliers on tempcustomer.SupplierId equals tempsupllier.ID.ToString() into tempTable1
                                   from tb1 in tempTable1.DefaultIfEmpty()
                                   join tempCategory in _context.Categories on tempcustomer.CategoryId equals tempCategory.ID.ToString() into tempTable2
                                   from tb2 in tempTable2.DefaultIfEmpty()
                                   join tempCategory1 in _context.suppliers on tempcustomer.SupplierId equals tempCategory1.ID.ToString() into tempTable3
                                   from tb3 in tempTable3.DefaultIfEmpty()
                                   select new
                                   {
                                       tempcustomer.ID,
                                       tempcustomer.ProductCode,
                                       tempcustomer.ProductName,
                                       supplierName = tb1.SupplierName,
                                       categoryName = tb2.CategoryName,
                                       tempcustomer.Note,
                                       tempcustomer.Content,
                                       tempcustomer.Ingredient,
                                   };

                if (string.IsNullOrEmpty(sortColumn))
                {
                    sortColumn = "ProductCode";
                    sortColumnDirection = "desc";
                }
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    customerData = customerData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    customerData = customerData.Where(m => m.ProductCode.Contains(searchValue) || m.ProductName.Contains(searchValue));
                }

                recordsTotal = customerData.Count();
                var data = customerData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);

            }
            catch
            {
                throw;
            }
        }

        // ====== XOÁ PRODUCT CÓ RÀNG BUỘC ======
        [HttpDelete]
        [Route("DeleteEmp")]
        public IActionResult DeleteEmp(Guid id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            var productIdString = product.ID.ToString();

            var hasImports = _context.ImportProductDetails.Any(d => d.ProduceId == productIdString);
            var hasInvoiceDetails = _context.Invoice_Details.Any(d => d.ProductId == productIdString);
            var hasStock = _context.stocks.Any(s => s.ProductId == productIdString);

            if (hasImports || hasInvoiceDetails || hasStock)
            {
                return BadRequest("Không thể xoá mặt hàng này vì đã phát sinh giao dịch hoặc đang còn tồn kho.");
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
            return Ok("Xoá mặt hàng thành công.");
        }
    }
}
