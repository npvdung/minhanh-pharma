using Base_Asp_Core_MVC_with_Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReturnProductApiController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private readonly UserManager<UserSystemIdentity> _uid;

        public ReturnProductApiController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> uid)
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
                var customerData = from tempcustomer in _context.ReturnProducts
                                   join tempsupllier in _context.suppliers on tempcustomer.SupplierId equals tempsupllier.ID.ToString() into tempTable1
                                   from tb1 in tempTable1.DefaultIfEmpty()
                                   join tempImportProduct in _context.ImportsProduct on tempcustomer.ImportId equals tempImportProduct.ID.ToString() into tempTable2
                                   from tb2 in tempTable2.DefaultIfEmpty()
                                   select new
                                   {
                                       tempcustomer.ID,
                                       tempcustomer.ReturnDate,
                                       tempcustomer.Description,
                                       tempcustomer.TotalAmount,
                                       exportCode = tempcustomer.ImportId,
                                       exportName = tempcustomer.ImportId,
                                       tempcustomer.Reason
                                   };

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    customerData = customerData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    customerData = customerData.Where(m => m.exportCode.Contains(searchValue) || m.Description.Contains(searchValue));
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
    }
}
