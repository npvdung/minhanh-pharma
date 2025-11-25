using Base_Asp_Core_MVC_with_Identity.CommonFile.Enum;
using Base_Asp_Core_MVC_with_Identity.CommonFile.IServiceCommon;
using Base_Asp_Core_MVC_with_Identity.CommonMethod;
using Base_Asp_Core_MVC_with_Identity.Data;
using Base_Asp_Core_MVC_with_Identity.Models.View;
using Base_Asp_Core_MVC_with_Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using System.Globalization;
using System.Linq;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class ReSalseController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public ReSalseController(
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

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Index()
        {
            return View();
        }

        // ---------------- CREATE (GET) ----------------
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            string prefix = "Return_";
            Expression<Func<ReSales, string>> codeSelector = c => c.Sales;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);

            var viewModel = new ReSalseViewModel
            {
                reSalesMaster = new ReSales
                {
                    Sales = autoCode,
                    InvoiceDate = DateTime.Now,
                    Description = "",
                },
                reSalesDetails = new List<ReSalesDetail>()
            };

            // ---- DANH SÁCH LÔ HÀNG TRONG KHO (CHỌN THEO LÔ) ----
            var productList = (from w in _context.stocks
                               join p in _context.Products on w.ProductId equals p.ID.ToString()
                               select new
                               {
                                   StockId = w.ID.ToString(),      // Mã lô (Warehouse.ID)
                                   ProductName = p.ProductName,
                                   Batch = w.ProductionBatch,
                                   Exp = w.ExpirationData,
                                   Quantity = w.QuantityInStock
                               }).ToList();

            var itemData = new List<SelectListItem>();

            foreach (var item in productList)
            {
                string batch = item.Batch.HasValue
                    ? item.Batch.Value.ToString("yyyy-MM-dd")
                    : "";
                string exp = item.Exp.HasValue
                    ? item.Exp.Value.ToString("yyyy-MM-dd")
                    : "";

                itemData.Add(new SelectListItem
                {
                    Text = $"{item.ProductName} - Lô: {batch} - HSD: {exp} - Tồn: {item.Quantity}",
                    Value = item.StockId
                });
            }
            ViewData["product_lst"] = itemData;

            // ---- ĐƠN VỊ TÍNH ----
            var UnitProduct = _context.productUnits.ToList();
            var itemDataTemp = new List<SelectListItem>();
            foreach (var item in UnitProduct)
            {
                itemDataTemp.Add(new SelectListItem
                {
                    Text = item.UnitName,
                    Value = item.ID.ToString()
                });
            }
            ViewData["unit_lst"] = itemDataTemp;

            // ---- NGƯỜI DÙNG ----
            var data1 = (from p in _userManager.Users
                         select new
                         {
                             Id = p.Id,
                             Name = p.FirstName + " " + p.LastName,
                         }).ToList();

            var itemData1 = new List<SelectListItem>();
            foreach (var item in data1)
            {
                itemData1.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id
                });
            }
            ViewData["account_lst"] = itemData1;

            // ---- KHÁCH HÀNG ----
            var data2 = (from c in _context.Customers
                         select new
                         {
                             Id = c.ID,
                             Name = c.FullName + " " + c.PhoneNumber,
                         }).ToList();

            var itemData2 = new List<SelectListItem>();
            foreach (var item in data2)
            {
                itemData2.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }
            ViewData["customer_lst"] = itemData2;

            // ---- HÓA ĐƠN GỐC ----
            var data3 = (from i in _context.Invoices
                         select new
                         {
                             Id = i.ID,
                             Name = i.InvoiceCode,
                         }).ToList();

            var itemData3 = new List<SelectListItem>();
            foreach (var item in data3)
            {
                itemData3.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }
            ViewData["inv_lst"] = itemData3;

            return View(viewModel);
        }

        // ---------------- CREATE (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(ReSalseViewModel empobj)
        {
            var master = new ReSales
            {
                Sales = empobj.reSalesMaster.Sales,
                UserId = empobj.reSalesMaster.UserId,
                InvoiceDate = empobj.reSalesMaster.InvoiceDate,
                Reason = empobj.reSalesMaster.Reason,
                Description = empobj.reSalesMaster.Description,
                TotalAmount = empobj.reSalesMaster.TotalAmount,
                CustomerId = empobj.reSalesMaster.CustomerId,
            };
            _context.reSales.Add(master);
            _context.SaveChanges();

            var details = new List<ReSalesDetail>();

            foreach (var item in empobj.reSalesDetails)
            {
                if (string.IsNullOrWhiteSpace(item.ProductId))
                    continue;

                if (!Guid.TryParse(item.ProductId, out Guid stockId))
                    continue;

                var stock = _context.stocks.Find(stockId);
                if (stock == null)
                    continue;

                var detail = new ReSalesDetail
                {
                    SaleId = master.ID.ToString(),
                    Description = "Không",
                    CreatedDate = DateTime.Now,
                    ProductId = stock.ProductId,       // ID sản phẩm gốc
                    ImportId = item.ProductId,         // ID lô (Warehouse.ID)
                    Unit = item.Unit,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    UserId = master.UserId,
                    TotalAmount = item.TotalAmount,
                    UnitProductId = item.UnitProductId,
                    ConvertRate = item.ConvertRate,
                };

                details.Add(detail);

                stock.QuantityInStock += (item.Quantity * item.ConvertRate);
                _context.stocks.Update(stock);
            }

            if (details.Any())
            {
                _context.reSalesDetail.AddRange(details);
            }

            decimal totalFromDetails = details.Sum(d => d.TotalAmount ?? 0);
            master.TotalAmount = totalFromDetails;
            _context.reSales.Update(master);

            _context.SaveChanges();

            TempData["ResultOk"] = "Tạo dữ liệu thành công !";
            return RedirectToAction("Index");
        }

        // ---------------- EDIT (VIEW CHI TIẾT) ----------------
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Guid Id)
        {
            if (Id == Guid.Empty)
            {
                return NotFound();
            }

            var empfromdb = _context.reSales.Find(Id);
            if (empfromdb == null)
            {
                return NotFound();
            }

            var empfromdbDetails = _context.reSalesDetail
                                           .Where(x => x.SaleId == empfromdb.ID.ToString())
                                           .ToList();

            var viewModel = new ReSalseViewModel
            {
                reSalesMaster = new ReSales
                {
                    ID = empfromdb.ID,
                    Sales = empfromdb.Sales,
                    UserId = empfromdb.UserId,
                    InvoiceDate = empfromdb.InvoiceDate,
                    Reason = empfromdb.Reason,
                    Description = empfromdb.Description,
                    CustomerId = empfromdb.CustomerId,
                    TotalAmount = Math.Round((decimal)(empfromdb.TotalAmount ?? 0), 2),
                },
                reSalesDetails = new List<ReSalesDetail>()
            };

            foreach (var item in empfromdbDetails)
            {
                viewModel.reSalesDetails.Add(new ReSalesDetail
                {
                    ID = item.ID,
                    SaleId = item.SaleId,
                    Description = item.Description,
                    CreatedDate = item.CreatedDate,
                    ProductId = item.ImportId,
                    Unit = item.Unit,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    UserId = item.UserId,
                    TotalAmount = item.TotalAmount,
                    UnitProductId = item.UnitProductId,
                    ConvertRate = item.ConvertRate
                });
            }

            var productList = (from w in _context.stocks
                               join p in _context.Products on w.ProductId equals p.ID.ToString()
                               select new
                               {
                                   StockId = w.ID.ToString(),
                                   ProductName = p.ProductName,
                                   Batch = w.ProductionBatch,
                                   Exp = w.ExpirationData,
                                   Quantity = w.QuantityInStock
                               }).ToList();

            var itemData = new List<SelectListItem>();
            foreach (var item in productList)
            {
                string batch = item.Batch.HasValue
                    ? item.Batch.Value.ToString("yyyy-MM-dd")
                    : "";
                string exp = item.Exp.HasValue
                    ? item.Exp.Value.ToString("yyyy-MM-dd")
                    : "";

                itemData.Add(new SelectListItem
                {
                    Text = $"{item.ProductName} - Lô: {batch} - HSD: {exp} - Tồn: {item.Quantity}",
                    Value = item.StockId
                });
            }
            ViewData["product_lst"] = itemData;

            var UnitProduct = _context.productUnits.ToList();
            var itemDataTemp = new List<SelectListItem>();
            foreach (var item in UnitProduct)
            {
                itemDataTemp.Add(new SelectListItem
                {
                    Text = item.UnitName,
                    Value = item.ID.ToString()
                });
            }
            ViewData["unit_lst"] = itemDataTemp;

            var data1 = (from p in _userManager.Users
                         select new
                         {
                             Id = p.Id,
                             Name = p.FirstName + " " + p.LastName,
                         }).ToList();

            var itemData1 = new List<SelectListItem>();
            foreach (var item in data1)
            {
                itemData1.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id
                });
            }
            ViewData["account_lst"] = itemData1;

            var data2 = (from c in _context.Customers
                         select new
                         {
                             Id = c.ID,
                             Name = c.FullName + " " + c.PhoneNumber,
                         }).ToList();

            var itemData2 = new List<SelectListItem>();
            foreach (var item in data2)
            {
                itemData2.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }
            ViewData["customer_lst"] = itemData2;

            var data3 = (from i in _context.Invoices
                         select new
                         {
                             Id = i.ID,
                             Name = i.InvoiceCode,
                         }).ToList();

            var itemData3 = new List<SelectListItem>();
            foreach (var item in data3)
            {
                itemData3.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }
            ViewData["inv_lst"] = itemData3;

            return View(viewModel);
        }
    }
}
