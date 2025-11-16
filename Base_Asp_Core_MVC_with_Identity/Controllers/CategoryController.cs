using Base_Asp_Core_MVC_with_Identity.Areas.Identity.Data;
using Base_Asp_Core_MVC_with_Identity.CommonFile.Enum;
using Base_Asp_Core_MVC_with_Identity.CommonFile.IServiceCommon;
using Base_Asp_Core_MVC_with_Identity.Data;
using Base_Asp_Core_MVC_with_Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class CategoryController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;
        public CategoryController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> userManager, ICommonService commonService)
        {
            _context = context;
            _userManager = userManager;
            _commonService = commonService;
        }

        public Base_Asp_Core_MVC_with_IdentityContext Get_context()
        {
            return _context;
        }

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Index()
        {
            IEnumerable<Category> objCatlist = _context.Categories;
            return View(objCatlist);
        }

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            Category category = new Category();
            string prefix = "DMT_";
            Expression<Func<Category, string>> codeSelector = c => c.CategoryCode;
            string categoryCode = _commonService.GenerateCategoryCode(prefix, codeSelector);
            category.CategoryCode = categoryCode;
            category.Description = "không";
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(Category empobj)
        {
            if (ModelState.IsValid)
            {

                _context.Categories.Add(empobj);
                _context.SaveChanges();
                TempData["ResultOk"] = "Tạo dữ liệu thành công !";
                return RedirectToAction("Index");
            }

            return View(empobj);
        }
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Guid Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var empfromdb = _context.Categories.Find(Id);

            if (empfromdb == null)
            {
                return NotFound();
            }
            return View(empfromdb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Category empobj)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(empobj);
                _context.SaveChanges();
                TempData["ResultOk"] = "Cập nhập dữ liệu thành công !";
                return RedirectToAction("Index");
            }
            return View(empobj);
        }
        public IActionResult Delete(Guid Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var empfromdb = _context.Categories.Find(Id);

            if (empfromdb == null)
            {
                return NotFound();
            }
            return View(empfromdb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult DeleteEmp(Guid Id)
        {
            var deleterecord = _context.Categories.Find(Id);
            if (deleterecord == null)
            {
                return NotFound();
            }
            _context.Categories.Remove(deleterecord);
            _context.SaveChanges();
            TempData["ResultOk"] = "Thông tin xoá thành công !";
            return RedirectToAction("Index");
        }
    }
}
