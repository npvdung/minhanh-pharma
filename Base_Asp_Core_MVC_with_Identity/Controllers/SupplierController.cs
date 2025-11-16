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
    public class SupplierController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public SupplierController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> userManager, ICommonService commonService)
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
            IEnumerable<Supplier> objCatlist = _context.suppliers;
            return View(objCatlist);
        }

        [Authorize(Roles = "Admin, Employee")]

        public IActionResult Create()
        {
            Supplier supplier = new Supplier();
            string prefix = "NCC_";
            Expression<Func<Supplier, string>> codeSelector = c => c.SupplierCode;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);
            supplier.SupplierCode = autoCode;
            supplier.Description = "không";
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(Supplier empobj)
        {
            //idUser = _userManager.GetUserId(HttpContext.User);
            //empobj.CreateId = idUser;
            if (ModelState.IsValid)
            {

                _context.suppliers.Add(empobj);
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
            var empfromdb = _context.suppliers.Find(Id);

            if (empfromdb == null)
            {
                return NotFound();
            }
            return View(empfromdb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Supplier empobj)
        {
            if (ModelState.IsValid)
            {
                _context.suppliers.Update(empobj);
                _context.SaveChanges();
                TempData["ResultOk"] = "Cập nhập dữ liệu thành công !";
                return RedirectToAction("Index");
            }
            return View(empobj);
        }
    }
}
