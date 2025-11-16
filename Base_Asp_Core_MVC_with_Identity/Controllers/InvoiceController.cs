using Base_Asp_Core_MVC_with_Identity.CommonFile.Enum;
using Base_Asp_Core_MVC_with_Identity.CommonFile.IServiceCommon;
using Base_Asp_Core_MVC_with_Identity.CommonMethod;
using Base_Asp_Core_MVC_with_Identity.Data;
using Base_Asp_Core_MVC_with_Identity.Models;
using Base_Asp_Core_MVC_with_Identity.Models.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Linq.Expressions;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public InvoiceController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> userManager, ICommonService commonService)
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
            IEnumerable<Sales> objCatlist = _context.Invoices;
            return View(objCatlist);
        }
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            Sales product = new Sales();

            string prefix = "INV_";
            var ramdonId = Guid.NewGuid();
            Expression<Func<Sales, string>> codeSelector = c => c.InvoiceCode;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);
            product.InvoiceCode = autoCode;

            var viewModel = new InvoiceViewModel();
            viewModel.Sales.InvoiceCode = autoCode;
            viewModel.Sales.Description = "Không";
            viewModel.Sales.InvoiceDate = DateTime.Now;
            viewModel.Sales.ID = ramdonId;
            //viewModel.salesProductsDetails = Enumerable.Range(0, 5).Select(_ => new Models.SalesProducts()
            //{
            //    Description = "Khong",
            //}).ToList();


            var productList = (from p in _context.stocks
                               join s in _context.Products on p.ProductId equals s.ID.ToString()
                               select new
                               {
                                   ProductId = p.ProductId,
                                   Price = s.Price.Value.ToString("C", new CultureInfo("vi-VN")), // Định dạng tiền tệ
                                   Name = s.ProductName,
                                   Total = p.QuantityInStock,
                                   ImportId = p.ID.ToString(),
                                   ExpirationData = p.ExpirationData.HasValue
                                        ? p.ExpirationData.Value.ToString("yyyy-MM-dd")
                                        : null // Để xử lý nếu ExpirationData là null
                               }).ToList();

            List<SelectListItem> itemData = new List<SelectListItem>();

            foreach (var item in productList)
            {
                itemData.Add(new SelectListItem
                {
                    Text = $"{item.Name} - {item.ExpirationData} - {item.Price} - {item.Total}",
                    Value = item.ImportId.ToString()
                });
            }
            ViewBag.product_lst = itemData;

            var UnitProduct = _context.productUnits.ToList();

            List<SelectListItem> itemDataTemp = new List<SelectListItem>();

            foreach (var item in UnitProduct)
            {
                itemDataTemp.Add(new SelectListItem
                {
                    Text = $"{item.UnitName}",
                    Value = item.ID.ToString()
                });
            }
            ViewBag.unit_lst = itemDataTemp;

            var data1 = (from p in _userManager.Users
                         select new
                         {
                             Id = p.Id,
                             Name = p.FirstName + " " + p.LastName,
                         }).ToList();

            List<SelectListItem> itemData1 = new List<SelectListItem>();

            foreach (var item in data1)
            {
                itemData1.Add(new SelectListItem
                {
                    Text = $"{item.Name}",
                    Value = item.Id
                });
            }
            ViewBag.account_lst = itemData1;

            var customerList = (from p in _context.Customers
                                select new
                                {
                                    customerId = p.ID,
                                    customerName = p.FullName,
                                }).ToList();

            List<SelectListItem> itemDataCustomer = new List<SelectListItem>();

            foreach (var item in customerList)
            {
                itemDataCustomer.Add(new SelectListItem
                {
                    Value = $"{item.customerName}",
                    Text = item.customerId.ToString()
                });
            }
            ViewBag.customer_lst = itemDataCustomer;
            viewModel.Sales.InvoiceDate = DateTime.Now;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(InvoiceViewModel empobj)
        {
            //if (ModelState.IsValid)
            //{
            //thêm vào bảng master
            var master = new Sales()
            {
                InvoiceCode = empobj.Sales.InvoiceCode,
                UserId = empobj.Sales.UserId,
                Description = empobj.Sales.Description,
                InvoiceDate = DateTime.Now,
                TotalAmount = empobj.Sales.TotalAmount,
                CustomerId = empobj.Sales.CustomerId,
            };
            _context.Invoices.Add(master);
            //_context.SaveChanges();

            var details = new List<Models.SalesProducts>();


            //thêm vào bảng details
            foreach (var item in empobj.salesProductsDetails)
            {
                if (item.ProductId != null)
                {
                    var importProduct = _context.stocks.Find(Guid.Parse(item.ProductId));
                    details.Add(new Models.SalesProducts()
                    {
                        SaleId = master.ID.ToString(),
                        ProductId = importProduct.ProductId,
                        ImportId = item.ProductId,
                        UserId = master.UserId,
                        Description = item.Description,
                        Unit = item.Unit,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        TotalAmount = item.TotalAmount,
                        CreatedDate = DateTime.Now,
                        UnitProductId= item.UnitProductId,
                    });
                }
            }

            _context.Invoice_Details.AddRange(details);
            _context.SaveChanges();


            //thêm vào kho

            //khi thêm vào thì check nếu mặt hàng đó chưa có (Có productId , nhà cung cấp, và hạn sử dụng thì update)

            var stock = new Warehouse();
            List<Warehouse> allStock = _context.stocks.ToList();
            //var existStock = allStock.FindAll(x => details.Select(y => y.ProductId).Contains(x.ProductId.ToString()));

            foreach (var item in details)
            {
                var itemE = _context.stocks.Where(x =>x.ID.ToString() == item.ImportId).FirstOrDefault();
                itemE.QuantityInStock = itemE.QuantityInStock - (item.Quantity * int.Parse(item.Description));
                _context.stocks.Update(itemE);
                _context.SaveChanges();

            }
            //còn không thì thêm thếm
            //còn không thì thêm mới
            TempData["ResultOk"] = "Tạo dữ liệu thành công !";
            return RedirectToAction("Index");
            //}
        }

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Guid Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var empfromdb = _context.Invoices.Find(Id);
            var empfromdbDetails = _context.Invoice_Details.Where(x => x.SaleId == empfromdb.ID.ToString()).ToList();
            var viewModel = new InvoiceViewModel();
            viewModel.Sales = new Sales
            {
                InvoiceCode = empfromdb.InvoiceCode,
                UserId = empfromdb.UserId,
                Description = empfromdb.Description,
                InvoiceDate = empfromdb.InvoiceDate,
                TotalAmount = Math.Round((decimal)empfromdb.TotalAmount, 2),
                CustomerId = empfromdb.CustomerId,
            };
            foreach (var item in empfromdbDetails)
            {
                viewModel.salesProductsDetails.Add(new SalesProducts
                {
                    ID = item.ID,
                    SaleId = item.SaleId,
                    ProductId = item.ProductId,
                    ImportId = item.ImportId,
                    UserId = item.UserId,
                    Description = item.Description,
                    Unit = item.Unit,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    TotalAmount = item.TotalAmount,
                    CreatedDate = item.CreatedDate,
                    UnitProductId = item.UnitProductId,
                });
            }
            //viewModel.ProductDetails = Enumerable.Range(0, 7).Select(_ => new Models.View.Import_Product_Details()
            //{
            //    Description = "Khong",
            //    ImportProductId = ramdonId.ToString()
            //}).ToList();


            var productList = (from p in _context.stocks
                               join s in _context.Products on p.ProductId equals s.ID.ToString()
                               select new
                               {
                                   ProductId = p.ProductId,
                                   Price = s.Price.Value.ToString("C", new CultureInfo("vi-VN")), // Định dạng tiền tệ
                                   Name = s.ProductName,
                                   Total = p.QuantityInStock,
                                   ExpirationData = p.ExpirationData.HasValue
                                        ? p.ExpirationData.Value.ToString("yyyy-MM-dd")
                                        : null // Để xử lý nếu ExpirationData là null
                               }).ToList();

            List<SelectListItem> itemData = new List<SelectListItem>();

            foreach (var item in productList)
            {
                itemData.Add(new SelectListItem
                {
                    Text = $"{item.Name} - {item.ExpirationData} - {item.Price} - {item.Total}",
                    Value = item.ProductId.ToString()
                });
            }
            ViewBag.product_lst = itemData;

            var data1 = (from p in _userManager.Users
                         select new
                         {
                             Id = p.Id,
                             Name = p.FirstName + " " + p.LastName,
                         }).ToList();

            List<SelectListItem> itemData1 = new List<SelectListItem>();

            foreach (var item in data1)
            {
                itemData1.Add(new SelectListItem
                {
                    Text = $"{item.Name}",
                    Value = item.Id
                });
            }
            ViewBag.account_lst = itemData1;

            var UnitProduct = _context.productUnits.ToList();

            List<SelectListItem> itemDataTemp = new List<SelectListItem>();

            foreach (var item in UnitProduct)
            {
                itemDataTemp.Add(new SelectListItem
                {
                    Text = $"{item.UnitName}",
                    Value = item.ID.ToString()
                });
            }
            ViewBag.unit_lst = itemDataTemp;

            var customerList = (from p in _context.Customers
                                select new
                                {
                                    customerId = p.ID,
                                    customerName = p.FullName,
                                }).ToList();

            List<SelectListItem> itemDataCustomer = new List<SelectListItem>();

            foreach (var item in customerList)
            {
                itemDataCustomer.Add(new SelectListItem
                {
                    Value = $"{item.customerName}",
                    Text = item.customerId.ToString()
                });
            }
            ViewBag.customer_lst = itemDataCustomer;
            if (viewModel == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(InvoiceViewModel empobj)
        {
            //var empobj = new ImportViewModel();
            //if (ModelState.IsValid)
            //{
            //thêm vào bảng master
            var empfromdb = _context.Invoices.Find(empobj.Sales.ID);
            var empfromdbDetails = _context.Invoice_Details.Where(x => x.SaleId == empfromdb.ID.ToString()).ToList();

            empfromdb.InvoiceCode = empfromdb.InvoiceCode;
            empfromdb.UserId = empfromdb.UserId;
            empfromdb.Description = empfromdb.Description;
            empfromdb.InvoiceDate = empfromdb.InvoiceDate;
            empfromdb.TotalAmount = empfromdb.TotalAmount;
            empfromdb.CustomerId = empfromdb.CustomerId;

            _context.Invoices.Update(empfromdb);


            foreach (var item in empfromdbDetails)
            {
                    item.SaleId = item.SaleId;
                    item.ProductId = item.ProductId;
                    item.ImportId = item.ImportId;
                    item.UserId = item.UserId;
                    item.Description = item.Description;
                    item.Unit = item.Unit;
                    item.Quantity = item.Quantity;
                    item.Price = item.Price;
                    item.TotalAmount = item.TotalAmount;
                    item.CreatedDate = item.CreatedDate;
                    item.UnitProductId = item.UnitProductId;
            _context.Invoice_Details.Update(item);
            }

            _context.SaveChanges();
            TempData["ResultOk"] = "Cập nhập dữ liệu thành công !";
            return RedirectToAction("Index");
            return View(empobj);
        }
    }
}
