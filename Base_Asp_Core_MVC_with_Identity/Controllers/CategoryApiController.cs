using Base_Asp_Core_MVC_with_Identity.Data;
using Base_Asp_Core_MVC_with_Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private readonly UserManager<UserSystemIdentity> _uid;

        public CategoryApiController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> uid)
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

                var customerData = (from tempcustomer in _context.Categories
                                    select tempcustomer);

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    customerData = customerData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    customerData = customerData.Where(m =>
                        m.CategoryName.Contains(searchValue) ||
                        m.CategoryCode.Contains(searchValue));
                }

                recordsTotal = customerData.Count();
                var data = customerData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch
            {
                // Có thể log thêm nếu muốn
                return StatusCode(500, "Đã xảy ra lỗi khi tải dữ liệu.");
            }
        }

        [HttpDelete]
        [Route("DeleteEmp")]
        public IActionResult DeleteEmp(Guid id)
        {
            try
            {
                var category = _context.Categories.Find(id);
                if (category == null)
                {
                    return NotFound("Không tìm thấy loại thuốc cần xoá.");
                }

                // ❗ Kiểm tra xem có sản phẩm nào đang dùng Category này không
                bool hasProducts = _context.Products
                    .Any(p => p.CategoryId == category.ID.ToString());

                if (hasProducts)
                {
                    return BadRequest("Không thể xoá loại thuốc này vì đang có mặt hàng thuộc loại thuốc này. Vui lòng đổi loại hoặc xoá các mặt hàng đó trước.");
                }

                _context.Categories.Remove(category);
                _context.SaveChanges();

                return Ok("Xoá loại thuốc thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Có lỗi xảy ra khi xoá: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMes(Category model)
        {
            // Giữ nguyên logic placeholder của bạn
            return Ok("Thao tác đã thành công.");
        }
    }
}
