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
    public class CustomerController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public CustomerController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> userManager, ICommonService commonService)
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
            IEnumerable<Customer> objCatlist = _context.Customers;
            return View(objCatlist);
        }

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            Customer customer = new Customer();
            string prefix = "KH_";
            Expression<Func<Customer, string>> codeSelector = c => c.CustomerCode;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);
            customer.CustomerCode = autoCode;
            customer.Desciption = "không";
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(Customer empobj)
        {
            if (ModelState.IsValid)
            {

                _context.Customers.Add(empobj);
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
            var empfromdb = _context.Customers.Find(Id);

            if (empfromdb == null)
            {
                return NotFound();
            }
            return View(empfromdb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Customer empobj)
        {
            if (ModelState.IsValid)
            {
                _context.Customers.Update(empobj);
                _context.SaveChanges();
                TempData["ResultOk"] = "Cập nhập dữ liệu thành công !";
                return RedirectToAction("Index");
            }
            return View(empobj);
        }
    }
}
