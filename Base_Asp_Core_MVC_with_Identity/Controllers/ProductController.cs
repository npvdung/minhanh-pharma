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
        private readonly UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public ProductController(
            Base_Asp_Core_MVC_with_IdentityContext context,
            UserManager<UserSystemIdentity> userManager,
            ICommonService commonService)
        {
            _context = context;
            _userManager = userManager;
            _commonService = commonService;
        }

        public Base_Asp_Core_MVC_with_IdentityContext Get_context()
        {
            return _context;
        }

        // ----------------- HÀM DÙNG LẠI ĐỂ ĐỔ DROPDOWN -----------------
        private void PopulateDropDowns()
        {
            // Danh mục (Category)
            var categoryList = _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.ID.ToString(),       // Value: ID
                    Text = c.CategoryName          // Text hiển thị: tên danh mục
                })
                .ToList();
            ViewData["CategoryList"] = categoryList;

            // Nhà cung cấp (Supplier)
            var supplierList = _context.suppliers
                .Select(s => new SelectListItem
                {
                    Value = s.ID.ToString(),       // Value: ID
                    Text = s.SupplierName          // Text hiển thị: tên NCC
                })
                .ToList();
            ViewData["SupplierList"] = supplierList;
        }
        //----------------------------------------------------------------

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Index()
        {
            IEnumerable<Product> objCatlist = _context.Products;
            return View(objCatlist);
        }

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            string prefix = "SP_";
            Expression<Func<Product, string>> codeSelector = c => c.ProductCode;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);

            var productViewModel = new ProductViewModel();
            // đảm bảo productMaster khác null (thường được khởi tạo trong ctor của ViewModel)
            productViewModel.productMaster.ProductCode = autoCode;
            productViewModel.productMaster.Note = "Note";

            // chuẩn bị dropdown
            PopulateDropDowns();

            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(ProductViewModel empobj)
        {
            if (!ModelState.IsValid)
            {
                // nếu lỗi validate thì đổ lại dropdown
                PopulateDropDowns();
                return View(empobj);
            }

            var product = new Product
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
                UserManual = empobj.productMaster.UserManual,
                Note = empobj.productMaster.Note,
            };
            _context.Products.Add(product);
            _context.SaveChanges();

            if (empobj.productUnits != null)
            {
                foreach (var item in empobj.productUnits)
                {
                    var productUnit = new ProductUnit
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
                }
                _context.SaveChanges();
            }

            TempData["ResultOk"] = "Tạo dữ liệu thành công !";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Guid Id)
        {
            var empfromdb = _context.Products.Find(Id);
            if (empfromdb == null)
            {
                return NotFound();
            }

            var empfromdbDetail = _context.productUnits
                .Where(x => x.ProductId == Id.ToString())
                .ToList();

            var productViewModel = new ProductViewModel
            {
                productMaster = new Product
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
                },
                productUnits = new List<ProductUnit>()
            };

            foreach (var item in empfromdbDetail)
            {
                productViewModel.productUnits.Add(new ProductUnit
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

            // chuẩn bị dropdown
            PopulateDropDowns();

            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(ProductViewModel empobj)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns();
                return View(empobj);
            }

            var empfromdb = _context.Products.Find(empobj.productMaster.ID);
            if (empfromdb == null)
            {
                return NotFound();
            }

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

            if (empobj.productUnits != null)
            {
                foreach (var item in empobj.productUnits)
                {
                    item.ProductId = empfromdb.ID.ToString();
                    _context.productUnits.Update(item);
                }
                _context.SaveChanges();
            }

            TempData["ResultOk"] = "Cập nhập dữ liệu thành công !";
            return RedirectToAction("Index");
        }
    }
}
