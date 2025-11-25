using Base_Asp_Core_MVC_with_Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using System.Linq;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReturnProductApiController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private readonly UserManager<UserSystemIdentity> _uid;

        public ReturnProductApiController(
            Base_Asp_Core_MVC_with_IdentityContext context,
            UserManager<UserSystemIdentity> uid)
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
                var sortColumn = Request.Query["columns[" + Request.Query["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Query["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Query["search[value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // base query cho master (phiếu trả)
                var query = from r in _context.ReturnProducts
                            select new
                            {
                                r.ID,
                                r.ReturnDate,
                                r.Description,
                                r.TotalAmount,
                                exportCode = r.ImportId,   // mã trả hàng
                                exportName = r.ImportId,
                                r.Reason
                            };

                // sort
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                // search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        m.exportCode.Contains(searchValue) ||
                        (m.Description ?? string.Empty).Contains(searchValue) ||
                        (m.Reason ?? string.Empty).Contains(searchValue));
                }

                recordsTotal = query.Count();

                var pageData = query.Skip(skip).Take(pageSize).ToList();

                // Lấy danh sách ID phiếu trả trên trang hiện tại
                var masterIds = pageData.Select(p => p.ID.ToString()).ToList();

                // ================== LẤY MÃ LÔ ==================
                // Return_Product_Details:
                //   DisposalRecordsId : ID phiếu trả (master)
                //   ProductId         : ID lô (stocks.ID)
                //
                // stocks:
                //   ID        : ID lô
                //   BatchCode : mã lô hiển thị

                var batchDict = _context.Return_Product_Details
                    .Where(d => masterIds.Contains(d.DisposalRecordsId))
                    .Join(
                        _context.stocks,
                        d => d.ProductId,              // <-- DÙNG ProductId, chính là stocks.ID
                        s => s.ID.ToString(),
                        (d, s) => new
                        {
                            d.DisposalRecordsId,
                            s.BatchCode
                        })
                    .AsEnumerable()
                    .GroupBy(x => x.DisposalRecordsId)
                    .ToDictionary(
                        g => g.Key,
                        g => string.Join(", ", g.Select(x => x.BatchCode).Distinct())
                    );

                var resultData = pageData.Select(item => new
                {
                    item.ID,
                    item.ReturnDate,
                    item.Description,
                    item.TotalAmount,
                    item.exportCode,
                    item.exportName,
                    item.Reason,
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
    }
}
