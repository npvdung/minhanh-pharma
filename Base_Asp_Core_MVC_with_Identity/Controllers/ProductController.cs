using Base_Asp_Core_MVC_with_Identity.CommonFile.Enum;
using Base_Asp_Core_MVC_with_Identity.CommonFile.IServiceCommon;
using Base_Asp_Core_MVC_with_Identity.Data;
using Base_Asp_Core_MVC_with_Identity.Models;
using Base_Asp_Core_MVC_with_Identity.Models.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class ProductController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public ProductController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> userManager, ICommonService commonService)
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
            IEnumerable<Product> objCatlist = _context.Products;
            return View(objCatlist);
        }

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            Product product = new Product();
            string prefix = "SP_";
            Expression<Func<Product, string>> codeSelector = c => c.ProductCode;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);
            ProductViewModel productViewModel = new ProductViewModel();

            productViewModel.productMaster.ProductCode = autoCode;
            productViewModel.productMaster.Note = "Note";

            List<Category> categoryLst = _context.Categories.ToList();
            List<SelectListItem> category = new List<SelectListItem>();
            foreach (var item in categoryLst)
            {
                if (item.ID.ToString() != null)
                {
                    category.Add(new SelectListItem { Text = item.ID.ToString(), Value = item.CategoryName.ToString() });
                }
            }
            ViewBag.category_lst = category;

            List<Supplier> supplierLst = _context.suppliers.ToList();
            List<SelectListItem> supplier = new List<SelectListItem>();
            foreach (var item in supplierLst)
            {
                if (item.ID.ToString() != null)
                {
                    supplier.Add(new SelectListItem { Text = item.ID.ToString(), Value = item.SupplierName.ToString() });
                }
            }
            ViewBag.supplier_lst = supplier;
            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(ProductViewModel empobj)
        {
            //if (ModelState.IsValid)
            //{
            ProductViewModel productViewModel = new ProductViewModel();
            Product product = new Product()
            {
                ID = Guid.NewGuid(),
                ProductCode = empobj.productMaster.ProductCode,
                ProductName = empobj.productMaster.ProductName,
                CategoryId = empobj.productMaster.CategoryId,
                SupplierId = empobj.productMaster.SupplierId,
                Unit = empobj.productMaster.Unit,
                Price = empobj.productMaster.Price,
                Ingredient = empobj.productMaster.Ingredient,
                Content = empobj.productMaster.Content,
                Uses = empobj.productMaster.Uses,
                UserManual= empobj.productMaster.UserManual,
                Note = empobj.productMaster.Note,
            };
            _context.Products.Add(product);
            _context.SaveChanges();

            foreach (var item in empobj.productUnits)
            {
                ProductUnit productUnit = new ProductUnit()
                {
                    ID = Guid.NewGuid(),
                    ProductId = product.ID.ToString(),
                    UnitName = item.UnitName,
                    Rate = item.Rate,
                    PriceBuy = item.PriceBuy,
                    Contain = "BackUp",
                    PriceSell = item.PriceSell
                };
                _context.productUnits.Add(productUnit);
                _context.SaveChanges();

            }
            TempData["ResultOk"] = "Tạo dữ liệu thành công !";
            return RedirectToAction("Index");
            //}
            return View(empobj);
        }

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Guid Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var empfromdb = _context.Products.Find(Id);
            var empfromdbDetail = _context.productUnits.Where(x =>x.ProductId == Id.ToString());
            ProductViewModel productViewModel = new ProductViewModel();

            productViewModel.productMaster = new Product()
            {
                ID = Id,
                ProductCode = empfromdb.ProductCode,
                ProductName = empfromdb.ProductName,
                CategoryId = empfromdb.CategoryId,
                SupplierId = empfromdb.SupplierId,
                Unit = empfromdb.Unit,
                Price = empfromdb.Price,
                Ingredient = empfromdb.Ingredient,
                Content = empfromdb.Content,
                Uses = empfromdb.Uses,
                UserManual = empfromdb.UserManual,
                Note = empfromdb.Note
            };

            foreach (var item in empfromdbDetail)
            {
                productViewModel.productUnits.Add( new ProductUnit()
                {
                    ID = item.ID,
                    ProductId = item.ProductId,
                    UnitName = item.UnitName,
                    Rate = item.Rate,
                    PriceBuy = item.PriceBuy,
                    Contain = item.Contain,
                    PriceSell = item.PriceSell,
                });
            }

            List<Category> categoryLst = _context.Categories.ToList();
            List<SelectListItem> category = new List<SelectListItem>();
            foreach (var item in categoryLst)
            {
                if (item.ID.ToString() != null)
                {
                    category.Add(new SelectListItem { Text = item.ID.ToString(), Value = item.CategoryName.ToString() });
                }
            }
            ViewBag.category_lst = category;

            List<Supplier> supplierLst = _context.suppliers.ToList();
            List<SelectListItem> supplier = new List<SelectListItem>();
            foreach (var item in supplierLst)
            {
                if (item.ID.ToString() != null)
                {
                    supplier.Add(new SelectListItem { Text = item.ID.ToString(), Value = item.SupplierName.ToString() });
                }
            }
            ViewBag.supplier_lst = supplier;
            if (empfromdb == null)
            {
                return NotFound();
            }
            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(ProductViewModel empobj)
        {
            var empfromdb = _context.Products.Find(empobj.productMaster.ID);
            var empfromdbDetail = _context.productUnits.Where(x => x.ProductId == empfromdb.ID.ToString());
            ProductViewModel productViewModel = new ProductViewModel();



            empfromdb.ProductCode = empobj.productMaster.ProductCode;
            empfromdb.ProductName = empobj.productMaster.ProductName;
            empfromdb.CategoryId = empobj.productMaster.CategoryId;
            empfromdb.SupplierId = empobj.productMaster.SupplierId;
            empfromdb.Unit = empobj.productMaster.Unit;
            empfromdb.Price = empobj.productMaster.Price;
            empfromdb.Ingredient = empobj.productMaster.Ingredient;
            empfromdb.Content = empobj.productMaster.Content;
            empfromdb.Uses = empobj.productMaster.Uses;
            empfromdb.UserManual = empobj.productMaster.UserManual;
            empfromdb.Note = empobj.productMaster.Note;
            _context.Products.Update(empfromdb);
            _context.SaveChanges();


            foreach (var item in empobj.productUnits)
            {

                item.ProductId = empfromdb.ID.ToString();
                item.UnitName = item.UnitName;
                item.Rate = item.Rate;
                item.PriceBuy = item.PriceBuy;
                item.Contain = item.Contain;
                item.PriceSell = item.PriceSell;
                _context.productUnits.Update(item);
                _context.SaveChanges();
            }

            TempData["ResultOk"] = "Cập nhập dữ liệu thành công !";
                return RedirectToAction("Index");
        }
    }
}
