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
    public class ReturnProductController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public ReturnProductController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> userManager, ICommonService commonService)
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
            IEnumerable<DisposalRecords> objCatlist = _context.ReturnProducts;
            return View(objCatlist);
        }
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            ReturnViewModel returnViewModel = new ReturnViewModel();

            var ramdonId = Guid.NewGuid();
            string prefix = "REtunr_";
            Expression<Func<DisposalRecords, string>> codeSelector = c => c.ImportId;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);

            var viewModel = new ReturnViewModel();
            viewModel.disposalRecordsMaster = new DisposalRecords()
            {
                ImportId = autoCode,
                ExportDate = DateTime.Now,
                SupplierId = "2342",
                Description = "Description",
                ReturnDate = DateTime.Now,
                Reason = ""
            };
            
            viewModel.ReturnsDetails = Enumerable.Range(0, 7).Select(_ => new Models.DisposalProducts()).ToList();


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
                    Text = $"{item.Name} - {item.ExpirationData} - {item.Total}",
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
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(ReturnViewModel empobj)
        {
            string prefix = "EP_";
            Expression<Func<DisposalRecords, string>> codeSelector = c => c.ImportId;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);
            var master = new DisposalRecords()
            {
               ImportId = autoCode,
               ExportDate = empobj.disposalRecordsMaster.ExportDate,
               UserId = empobj.disposalRecordsMaster.UserId,
               SupplierId = "0d413f2c-1d8d-4c9e-a237-c6abdfc12f2b",
               Description = empobj.disposalRecordsMaster.Description,
               TotalAmount = empobj.disposalRecordsMaster.TotalAmount,
               Reason = empobj.disposalRecordsMaster.Reason,
               ReturnDate = empobj.disposalRecordsMaster.ReturnDate,
            };
            _context.ReturnProducts.Add(master);
            //_context.SaveChanges();

            var details = new List<Models.DisposalProducts>();


            //thêm vào bảng details
            foreach (var item in empobj.ReturnsDetails)
            {
                if (item.ProductId != null)
                {
                    var importProduct = _context.stocks.Find(Guid.Parse(item.ProductId));
                    details.Add(new Models.DisposalProducts()
                    {
                       DisposalRecordsId = master.ID.ToString(),

                        ProductId = importProduct.ProductId,
                        ImportId = item.ProductId,
                        Description = item.Description,
                       TotalAmount = item.TotalAmount,
                       Unit = item.Unit,
                       Quantity = item.Quantity,
                       Price = item.Price,
                       ImportPrice = item.ImportPrice,
                       UnitProductId = item.UnitProductId,
                    });
                }
            }

            _context.Return_Product_Details.AddRange(details);
            _context.SaveChanges();


            //thêm vào kho

            //khi thêm vào thì check nếu mặt hàng đó chưa có (Có productId , nhà cung cấp, và hạn sử dụng thì update)

            var stock = new Warehouse();
            List<Warehouse> allStock = _context.stocks.ToList();
            //var existStock = allStock.FindAll(x => details.Select(y => y.ProductId).Contains(x.ProductId.ToString()));

            foreach (var item in details)
            {
                var itemE = _context.stocks.Where(x => x.ID.ToString() == item.ImportId).FirstOrDefault();
                itemE.QuantityInStock = itemE.QuantityInStock - (item.Quantity * int.Parse(item.Description));
                _context.stocks.Update(itemE);
                _context.SaveChanges();

            }
            //còn không thì thêm thếm
            //còn không thì thêm mới
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
            var empfromdb = _context.ReturnProducts.Find(Id);
            var empfromdbDetails = _context.Return_Product_Details.Where(x => x.DisposalRecordsId == empfromdb.ID.ToString());
            var viewModel = new ReturnViewModel();
            viewModel.disposalRecordsMaster = new DisposalRecords
            {
                
                SupplierId = empfromdb.SupplierId,
                Description = empfromdb.Description,
                ImportId = empfromdb.ImportId,
                UserId = empfromdb.UserId,
                TotalAmount = Math.Round((decimal)empfromdb.TotalAmount, 2),
                Reason = empfromdb.Reason,
                ReturnDate = empfromdb.ReturnDate,
            };

            foreach (var item in empfromdbDetails)
            {
                viewModel.ReturnsDetails.Add(new DisposalProducts
                {
                    ID = item.ID,
                    ProductId = item.ProductId,
                    UnitProductId = item.UnitProductId,
                    Description = item.Description,
                    ReturnDate = item.ReturnDate,
                    Unit = item.Unit,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    ImportPrice = item.ImportPrice,
                    TotalAmount = item.TotalAmount,
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
