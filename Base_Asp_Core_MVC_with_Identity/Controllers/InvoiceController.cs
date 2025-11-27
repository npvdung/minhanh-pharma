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
using Rotativa.AspNetCore;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private readonly UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public InvoiceController(
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

        // ========================= INDEX =========================
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Index()
        {
            IEnumerable<Sales> objCatlist = _context.Invoices;
            return View(objCatlist);
        }

        // ========================= CREATE - GET =========================
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            string prefix = "INV_";
            Guid ramdonId = Guid.NewGuid();
            Expression<Func<Sales, string>> codeSelector = c => c.InvoiceCode;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);

            var viewModel = new InvoiceViewModel
            {
                Sales = new Sales
                {
                    ID = ramdonId,
                    InvoiceCode = autoCode,
                    Description = "Không",
                    InvoiceDate = DateTime.Now
                }
            };

            // --------- danh sách sản phẩm trong kho (THEO LÔ) ----------
            var productList = (from p in _context.stocks
                               join s in _context.Products on p.ProductId equals s.ID.ToString()
                               select new
                               {
                                   ProductId = p.ProductId,                         // ID sản phẩm gốc
                                   ImportId = p.ID.ToString(),                     // ID của Warehouse (lô)
                                   Name = s.ProductName,
                                   BatchCode = p.BatchCode,                        // MÃ LÔ
                                   Price = s.Price.HasValue
                                       ? s.Price.Value.ToString("C", new CultureInfo("vi-VN"))
                                       : "0",
                                   Total = p.QuantityInStock,
                                   ExpirationData = p.ExpirationData.HasValue
                                       ? p.ExpirationData.Value.ToString("yyyy-MM-dd")
                                       : null
                               }).ToList();

            var itemData = new List<SelectListItem>();
            foreach (var item in productList)
            {
                // hiển thị luôn MÃ LÔ
                itemData.Add(new SelectListItem
                {
                    Text = $"{item.Name} - HSD: {item.ExpirationData} - Lô: {item.BatchCode} - Giá: {item.Price} - SL tồn: {item.Total}",
                    Value = item.ImportId   // chọn theo ID của Warehouse (lô)
                });
            }
            ViewBag.product_lst = itemData;

            // --------- đơn vị tính ----------
            var unitProduct = _context.productUnits.ToList();
            var unitItems = new List<SelectListItem>();
            foreach (var item in unitProduct)
            {
                unitItems.Add(new SelectListItem
                {
                    Text = item.UnitName,
                    Value = item.ID.ToString()
                });
            }
            ViewBag.unit_lst = unitItems;

            // --------- tài khoản người lập hóa đơn ----------
            var accountData = (from p in _userManager.Users
                               select new
                               {
                                   Id = p.Id,
                                   Name = p.FirstName + " " + p.LastName,
                               }).ToList();

            var accountItems = new List<SelectListItem>();
            foreach (var item in accountData)
            {
                accountItems.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id
                });
            }
            ViewBag.account_lst = accountItems;

            // --------- khách hàng ----------
            // --------- khách hàng (Tên + SĐT) ----------
            var customerList = (from p in _context.Customers
                                select new
                                {
                                    customerId = p.ID,
                                    displayName = p.FullName + " - " + p.PhoneNumber   // ⭐ THÊM SĐT ⭐
                                }).ToList();

            var customerItems = new List<SelectListItem>();
            foreach (var item in customerList)
            {
                customerItems.Add(new SelectListItem
                {
                    Text = item.displayName,                 // ⭐ HIỂN THỊ Tên + SĐT
                    Value = item.customerId.ToString()
                });
            }
            ViewBag.customer_lst = customerItems;


            return View(viewModel);
        }

        // ========================= CREATE - POST =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(InvoiceViewModel empobj)
        {
            // thêm vào bảng master
            var master = new Sales
            {
                InvoiceCode = empobj.Sales.InvoiceCode,
                UserId = empobj.Sales.UserId,
                Description = empobj.Sales.Description,
                InvoiceDate = DateTime.Now,
                TotalAmount = empobj.Sales.TotalAmount,
                CustomerId = empobj.Sales.CustomerId,
            };
            _context.Invoices.Add(master);

            // thêm vào bảng details
            var details = new List<SalesProducts>();

            foreach (var item in empobj.salesProductsDetails)
            {
                if (!string.IsNullOrEmpty(item.ProductId))
                {
                    // ProductId ở form đang là ImportId (ID của Warehouse / lô)
                    var importProduct = _context.stocks.Find(Guid.Parse(item.ProductId));
                    if (importProduct == null) continue;

                    details.Add(new SalesProducts
                    {
                        SaleId = master.ID.ToString(),
                        ProductId = importProduct.ProductId,     // ID sản phẩm gốc
                        ImportId = item.ProductId,               // ID của lô (Warehouse.ID)
                        UserId = master.UserId,
                        Description = item.Description,          // Tỉ lệ (ConvertRate)
                        Unit = item.Unit,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        TotalAmount = item.TotalAmount,
                        CreatedDate = DateTime.Now,
                        UnitProductId = item.UnitProductId,
                    });
                }
            }

            _context.Invoice_Details.AddRange(details);
            _context.SaveChanges();

            // cập nhật lại kho (trừ số lượng đã bán theo từng lô)
            foreach (var item in details)
            {
                var stockItem = _context.stocks
                    .FirstOrDefault(x => x.ID.ToString() == item.ImportId);
                if (stockItem == null) continue;

                // Description = tỉ lệ (ConvertRate)
                stockItem.QuantityInStock =
                    stockItem.QuantityInStock - (item.Quantity * int.Parse(item.Description));
                _context.stocks.Update(stockItem);
            }

            _context.SaveChanges();
            TempData["ResultOk"] = "Tạo dữ liệu thành công !";
            return RedirectToAction("Index");
        }

        // ========================= IN HÓA ĐƠN (PDF) =========================
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Print(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var invoice = _context.Invoices.Find(id);
            if (invoice == null)
            {
                return NotFound();
            }

            var details = _context.Invoice_Details
                                .Where(d => d.SaleId == invoice.ID.ToString())
                                .ToList();

            var viewModel = new InvoiceViewModel();

            // ----- Thông tin master -----
            var sellerName = _userManager.Users
                            .Where(u => u.Id == invoice.UserId)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault() ?? "";

            // Lấy cả tên + SĐT khách hàng
            var customer = _context.Customers
                                   .FirstOrDefault(c => c.ID.ToString() == invoice.CustomerId);

            var customerName = customer?.FullName ?? "";
            var customerPhone = customer?.PhoneNumber ?? "";

            // Ghép vào một field, sau đó tách ở View (để không sửa model)
            var customerDisplay = customerName;
            if (!string.IsNullOrEmpty(customerPhone))
            {
                customerDisplay = customerName + "||" + customerPhone;
            }

            viewModel.Sales = new Sales
            {
                InvoiceCode = invoice.InvoiceCode,
                Description = invoice.Description,
                InvoiceDate = invoice.InvoiceDate,
                TotalAmount = invoice.TotalAmount,
                // dùng tạm 2 field này để chứa thông tin hiển thị
                UserId = sellerName,          // tên người bán
                CustomerId = customerDisplay  // "Tên||SĐT" để view tách ra
            };

            // ----- Chi tiết + hiển thị MÃ LÔ -----
            foreach (var d in details)
            {
                string productDisplay = "";
                string unitName = "";
                string batchCode = "";

                // Lấy thông tin từ Warehouse (stocks) theo ImportId
                var stock = _context.stocks.FirstOrDefault(s => s.ID.ToString() == d.ImportId);
                if (stock != null)
                {
                    var product = _context.Products.FirstOrDefault(p => p.ID.ToString() == stock.ProductId);
                    var name = product?.ProductName ?? "";
                    var exp = stock.ExpirationData?.ToString("yyyy-MM-dd") ?? "";
                    batchCode = stock.BatchCode ?? "";
                    productDisplay = $"{name} - HSD: {exp}";
                }

                // Đơn vị
                if (!string.IsNullOrEmpty(d.UnitProductId))
                {
                    unitName = _context.productUnits
                                    .Where(u => u.ID.ToString() == d.UnitProductId)
                                    .Select(u => u.UnitName)
                                    .FirstOrDefault() ?? "";
                }

                viewModel.salesProductsDetails.Add(new SalesProducts
                {
                    Description = d.Description,        // Tỉ lệ
                    Quantity = d.Quantity,
                    Price = d.Price,
                    TotalAmount = d.TotalAmount,
                    ProductId = productDisplay,          // Tên + HSD
                    UnitProductId = unitName,            // Đơn vị
                    ImportId = batchCode                 // MÃ LÔ để in ra
                });
            }

            return new ViewAsPdf("Print", viewModel)
            {
                FileName = $"{viewModel.Sales.InvoiceCode}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }
        // ========================= EDIT - GET =========================
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Guid Id)
        {
            if (Id == Guid.Empty)
            {
                return NotFound();
            }

            var empfromdb = _context.Invoices.Find(Id);
            if (empfromdb == null)
            {
                return NotFound();
            }

            var empfromdbDetails = _context.Invoice_Details
                .Where(x => x.SaleId == empfromdb.ID.ToString())
                .ToList();

            var viewModel = new InvoiceViewModel
            {
                Sales = new Sales
                {
                    ID = empfromdb.ID,
                    InvoiceCode = empfromdb.InvoiceCode,
                    UserId = empfromdb.UserId,
                    Description = empfromdb.Description,
                    InvoiceDate = empfromdb.InvoiceDate,
                    TotalAmount = Math.Round((decimal)empfromdb.TotalAmount, 2),
                    CustomerId = empfromdb.CustomerId,
                }
            };

            foreach (var item in empfromdbDetails)
            {
                // Ở màn Edit chỉ xem, không sửa => ta cho ProductId = ImportId (ID lô)
                viewModel.salesProductsDetails.Add(new SalesProducts
                {
                    ID = item.ID,
                    SaleId = item.SaleId,
                    ProductId = item.ImportId,        // dùng ID lô cho dropdown
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

            // --------- sản phẩm (theo lô) ----------
            var productList = (from p in _context.stocks
                               join s in _context.Products on p.ProductId equals s.ID.ToString()
                               select new
                               {
                                   ProductId = p.ProductId,
                                   ImportId = p.ID.ToString(),
                                   Name = s.ProductName,
                                   BatchCode = p.BatchCode,
                                   Price = s.Price.HasValue
                                       ? s.Price.Value.ToString("C", new CultureInfo("vi-VN"))
                                       : "0",
                                   Total = p.QuantityInStock,
                                   ExpirationData = p.ExpirationData.HasValue
                                       ? p.ExpirationData.Value.ToString("yyyy-MM-dd")
                                       : null
                               }).ToList();

            var productItems = new List<SelectListItem>();
            foreach (var item in productList)
            {
                productItems.Add(new SelectListItem
                {
                    Text = $"{item.Name} - HSD: {item.ExpirationData} - Lô: {item.BatchCode} - Giá: {item.Price} - SL tồn: {item.Total}",
                    Value = item.ImportId     // dropdown chọn theo ID lô
                });
            }
            ViewBag.product_lst = productItems;

            // --------- tài khoản người lập hóa đơn ----------
            var accountData = (from p in _userManager.Users
                               select new
                               {
                                   Id = p.Id,
                                   Name = p.FirstName + " " + p.LastName,
                               }).ToList();

            var accountItems = new List<SelectListItem>();
            foreach (var item in accountData)
            {
                accountItems.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id
                });
            }
            ViewBag.account_lst = accountItems;

            // --------- đơn vị tính ----------
            var unitProduct = _context.productUnits.ToList();
            var unitItems = new List<SelectListItem>();
            foreach (var item in unitProduct)
            {
                unitItems.Add(new SelectListItem
                {
                    Text = item.UnitName,
                    Value = item.ID.ToString()
                });
            }
            ViewBag.unit_lst = unitItems;

            // --------- khách hàng ----------
            var customerList = (from p in _context.Customers
                                select new
                                {
                                    customerId = p.ID,
                                    customerName = p.FullName,
                                }).ToList();

            var customerItems = new List<SelectListItem>();
            foreach (var item in customerList)
            {
                customerItems.Add(new SelectListItem
                {
                    Text = item.customerName,
                    Value = item.customerId.ToString()
                });
            }
            ViewBag.customer_lst = customerItems;

            return View(viewModel);
        }

        // ========================= EDIT - POST =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(InvoiceViewModel empobj)
        {
            var empfromdb = _context.Invoices.Find(empobj.Sales.ID);
            if (empfromdb == null)
            {
                return NotFound();
            }

            // Hiện tại chỉ cho xem, không sửa dữ liệu gốc
            empfromdb.InvoiceCode = empfromdb.InvoiceCode;
            empfromdb.UserId = empfromdb.UserId;
            empfromdb.Description = empfromdb.Description;
            empfromdb.InvoiceDate = empfromdb.InvoiceDate;
            empfromdb.TotalAmount = empfromdb.TotalAmount;
            empfromdb.CustomerId = empfromdb.CustomerId;

            _context.Invoices.Update(empfromdb);

            var empfromdbDetails = _context.Invoice_Details
                .Where(x => x.SaleId == empfromdb.ID.ToString())
                .ToList();

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
        }
    }
}
