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
using System.Linq;
using System.Diagnostics.Metrics;
using System.Globalization;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class ReSalseController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public ReSalseController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> userManager, ICommonService commonService)
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
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            Import product = new Import();

            string prefix = "Return_";
            var ramdonId = Guid.NewGuid();
            Expression<Func<ReSales, string>> codeSelector = c => c.Sales;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);

            var viewModel = new ReSalseViewModel();
            viewModel.reSalesMaster.Sales = autoCode;
            viewModel.reSalesMaster.Description = "Không";
            viewModel.reSalesMaster.InvoiceDate = DateTime.Now;

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

            var data2 = (from p in _context.Customers
                         select new
                         {
                             Id = p.ID,
                             Name = p.FullName + " " + p.PhoneNumber,
                         }).ToList();

            List<SelectListItem> itemData2 = new List<SelectListItem>();

            foreach (var item in data2)
            {
                itemData2.Add(new SelectListItem
                {
                    Text = $"{item.Name}",
                    Value = item.Id.ToString()
                });
            }
            ViewBag.customer_lst = itemData2;

            var data3 = (from p in _context.Invoices
                         select new
                         {
                             Id = p.ID,
                             Name = p.InvoiceCode,
                         }).ToList();

            List<SelectListItem> itemData3 = new List<SelectListItem>();

            foreach (var item in data3)
            {
                itemData3.Add(new SelectListItem
                {
                    Text = $"{item.Name}",
                    Value = item.Id.ToString()
                });
            }
            ViewBag.inv_lst = itemData3;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(ReSalseViewModel empobj)
        {
            //var empobj = new ImportViewModel();
            //if (ModelState.IsValid)
            //{
            //thêm vào bảng master
            var master = new ReSales()
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
            //_context.SaveChanges();


            var details = new List<Models.ReSalesDetail>();


            //thêm vào bảng details
            foreach (var item in empobj.reSalesDetails)
            {
                var importProduct = _context.stocks.Find(Guid.Parse(item.ProductId));
                details.Add(new Models.ReSalesDetail()
                {
                    SaleId = master.ID.ToString(),
                    Description = "Không",
                    CreatedDate = DateTime.Now,
                    ProductId = importProduct.ProductId,
                    ImportId = item.ProductId,
                    Unit = item.Unit,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    UserId = master.UserId,
                    TotalAmount = item.TotalAmount,
                    UnitProductId = item.UnitProductId,
                    ConvertRate = item.ConvertRate,
                });

            }

            _context.reSalesDetail.AddRange(details);
            _context.SaveChanges();

            //khi thêm vào thì check nếu mặt hàng đó chưa có (Có productId , nhà cung cấp, và hạn sử dụng thì update)

            var stock = new Warehouse();
            List<Warehouse> allStock = _context.stocks.ToList();
            //var existStock = allStock.FindAll(x => details.Select(y => y.ProductId).Contains(x.ProductId.ToString()));

            foreach (var item in details)
            {
                var itemE = _context.stocks.Where(x => x.ID.ToString() == item.ImportId).FirstOrDefault();
                itemE.QuantityInStock = itemE.QuantityInStock + (item.Quantity * item.ConvertRate);
                _context.stocks.Update(itemE);
                _context.SaveChanges();

            }
            _context.SaveChanges();
            //thêm vào kho

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
            var empfromdb = _context.reSales.Find(Id);
            var empfromdbDetails = _context.reSalesDetail.Where(x => x.SaleId == empfromdb.ID.ToString());
            var viewModel = new ReSalseViewModel();
            viewModel.reSalesMaster = new ReSales
            {
                ID = empfromdb.ID,
                Sales = empfromdb.Sales,
                UserId = empfromdb.UserId,
                InvoiceDate = empfromdb.InvoiceDate,
                Reason = empfromdb.Reason,
                Description = empfromdb.Description,
                CustomerId = empfromdb.CustomerId,
                TotalAmount = Math.Round((decimal)empfromdb.TotalAmount, 2),
            };

            foreach (var item in empfromdbDetails)
            {
                viewModel.reSalesDetails.Add(new ReSalesDetail
                {
                    ID = item.ID,
                    SaleId = empfromdb.ID.ToString(),
                    Description = "Không",
                    CreatedDate = DateTime.Now,
                    ProductId = item.ProductId,
                    Unit = item.Unit,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    UserId = empfromdb.UserId,
                    TotalAmount = item.TotalAmount,
                    UnitProductId = item.UnitProductId,
                });
            }
            //viewModel.ProductDetails = Enumerable.Range(0, 7).Select(_ => new Models.View.Import_Product_Details()
            //{
            //    Description = "Khong",
            //    ImportProductId = ramdonId.ToString()
            //}).ToList();


            var productList = (from p in _context.Products
                               join s in _context.suppliers on p.SupplierId equals s.ID.ToString()
                               select new
                               {
                                   ProductId = p.ID,
                                   ProductName = p.ProductName,
                                   SupplierName = s.SupplierName
                               }).ToList();

            List<SelectListItem> itemData = new List<SelectListItem>();

            foreach (var item in productList)
            {
                itemData.Add(new SelectListItem
                {
                    Text = $"{item.ProductName} - {item.SupplierName}",
                    Value = item.ProductId.ToString()
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

            ViewBag.status_lst = Enum.GetValues(typeof(EnumApprodImport))
                            .Cast<EnumApprodImport>()
                            .Select(e => new SelectListItem
                            {
                                Text = e.GetDisplayName(),
                                Value = ((int)e).ToString()
                            }).ToList();

            var data2 = (from p in _context.Customers
                         select new
                         {
                             Id = p.ID,
                             Name = p.FullName + " " + p.PhoneNumber,
                         }).ToList();

            List<SelectListItem> itemData2 = new List<SelectListItem>();

            foreach (var item in data2)
            {
                itemData2.Add(new SelectListItem
                {
                    Text = $"{item.Name}",
                    Value = item.Id.ToString()
                });
            }
            ViewBag.customer_lst = itemData2;

            var data3 = (from p in _context.Invoices
                         select new
                         {
                             Id = p.ID,
                             Name = p.InvoiceCode,
                         }).ToList();

            List<SelectListItem> itemData3 = new List<SelectListItem>();

            foreach (var item in data3)
            {
                itemData3.Add(new SelectListItem
                {
                    Text = $"{item.Name}",
                    Value = item.Id.ToString()
                });
            }
            ViewBag.inv_lst = itemData3;


            if (viewModel == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(ImportViewModel empobj)
        {
            //var empobj = new ImportViewModel();
            //if (ModelState.IsValid)
            //{
            //thêm vào bảng master

            //var master = _context.ImportsProduct.Where(x => x.ID == empobj.ImportMaster.ID).FirstOrDefault();
            var master = new Import()
            {
                ID = empobj.ImportMaster.ID,
                ImportCode = empobj.ImportMaster.ImportCode,
                ImportName = empobj.ImportMaster.ImportName,
                ImportDate = empobj.ImportMaster.ImportDate,
                SupplierId = empobj.ImportMaster.SupplierId,
                Description = empobj.ImportMaster.Description,
                TotalAmount = empobj.ImportMaster.TotalAmount,
                AccountId = empobj.ImportMaster.AccountId,
                Status = empobj.ImportMaster.Status
            };
            _context.ImportsProduct.Update(master);
            _context.SaveChanges();


            var details = new List<Models.ImportProducts>();


            //thêm vào bảng details
            foreach (var item in empobj.ProductDetails)
            {
                if (item.ProduceId != null)
                {
                    details.Add(new Models.ImportProducts()
                    {
                        ID = item.ID,
                        ImportProductId = master.ID.ToString(),
                        Description = "Không",
                        ProductionBatch = DateTime.Now,
                        ManufacturingDate = item.ManufacturingDate,
                        ExpirationData = item.ExpirationData,
                        Unit = item.Unit,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        ProduceId = item.ProduceId,
                        ConvertRate = item.ConvertRate,
                        ImportPrice = item.ImportPrice,
                        TotalAmount = item.TotalAmount,
                        UnitProductId = item.UnitProductId,
                    });

                }
            }

            _context.ImportProductDetails.UpdateRange(details);
            _context.SaveChanges();

            if (empobj.ImportMaster.Status == (int)EnumApprodImport.approved)
            {
                //khi thêm vào thì check nếu mặt hàng đó chưa có (Có productId , nhà cung cấp, và hạn sử dụng thì update)

                var stock = new Warehouse();
                List<Warehouse> allStock = _context.stocks.ToList();
                var existStock = allStock.FindAll(x => details.Select(y => y.ProduceId).Contains(x.ProductId.ToString())
                                        && details.Select(z => z.ExpirationData).Contains(x.ExpirationData));
                foreach (var item in existStock)
                {
                    var id = item.ID;
                    var itemE = _context.stocks.Find(id);
                    itemE.QuantityInStock += item.QuantityInStock;
                    _context.stocks.Update(itemE);
                }
                var idExist = existStock.Select(x => x.ProductId.ToString());
                foreach (var item in details)
                {
                    if (!idExist.Contains(item.ProduceId))
                    {
                        var itemStock = new Warehouse()
                        {
                            ProductId = item.ProduceId,
                            ExpirationData = item.ExpirationData,
                            ManufacturingDate = item.ManufacturingDate,
                            ProductionBatch = DateTime.Now,
                            TotalValueImport = item.Price,
                            QuantityInStock = item.Quantity * (int)item.ConvertRate,
                        };
                        _context.stocks.Add(itemStock);
                    }
                }
                _context.SaveChanges();
            }
            //if (ModelState.IsValid)
            //{
            //    _context.Categories.Update(empobj);
            //    _context.SaveChanges();
            //}
            TempData["ResultOk"] = "Cập nhập dữ liệu thành công !";
            return RedirectToAction("Index");
            return View(empobj);
        }
    }
}
