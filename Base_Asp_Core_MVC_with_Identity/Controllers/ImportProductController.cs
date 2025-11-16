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
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class ImportProductController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public ImportProductController(Base_Asp_Core_MVC_with_IdentityContext context, UserManager<UserSystemIdentity> userManager, ICommonService commonService)
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
            IEnumerable<Import> objCatlist = _context.ImportsProduct;
            return View(objCatlist);
        }
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            Import product = new Import();

            string prefix = "IP_";
            var ramdonId = Guid.NewGuid();
            Expression<Func<Import, string>> codeSelector = c => c.ImportCode;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);
            product.ImportCode = autoCode;

            var viewModel = new ImportViewModel();
            viewModel.ImportMaster.ImportCode = autoCode;
            viewModel.ImportMaster.Description = "Không";
            viewModel.ImportMaster.ImportDate = DateTime.Now;

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


            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(ImportViewModel empobj)
        {
            empobj.ImportMaster.SupplierId = "08dc620d-b70b-4bec-8957-92617c38b23b";
            //var empobj = new ImportViewModel();
            //if (ModelState.IsValid)
            //{
            //thêm vào bảng master
            var master = new Import()
            {
                ImportCode = empobj.ImportMaster.ImportCode,
                ImportName = empobj.ImportMaster.ImportName,
                ImportDate = empobj.ImportMaster.ImportDate,
                SupplierId = empobj.ImportMaster.SupplierId,
                Description = empobj.ImportMaster.Description,
                TotalAmount = empobj.ImportMaster.TotalAmount,
                AccountId = empobj.ImportMaster.AccountId,
                Status = empobj.ImportMaster.Status,
            };
            _context.ImportsProduct.Add(master);
            //_context.SaveChanges();


            var details = new List<Models.ImportProducts>();


            //thêm vào bảng details
            foreach (var item in empobj.ProductDetails)
            {
                if (item.ProduceId != null)
                {
                    details.Add(new Models.ImportProducts()
                    {
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
                        UnitProductId= item.UnitProductId,
                    });
                }
            }

            _context.ImportProductDetails.AddRange(details);
            _context.SaveChanges();

            if (empobj.ImportMaster.Status == (int)EnumApprodImport.approved)
            {
                var stock = new Warehouse();
                List<Warehouse> allStock = _context.stocks.ToList();
                var existStock = allStock.FindAll(x => details.Select(y => y.ProduceId).Contains(x.ProductId.ToString())
                                        && details.Select(z => z.ExpirationData).Contains(x.ExpirationData));

                //TH nếu cùng hạn xử dụng thì thêm vào đúng lo
                foreach (var item in existStock)
                {
                    var id = item.ID;
                    var itemE = _context.stocks.Find(id);
                    var CountItem = details.Where(x => x.ProduceId == item.ProductId && x.ExpirationData.Value.Date == item.ExpirationData.Value).FirstOrDefault();
                    var InsertItem = CountItem.Quantity * (int)CountItem.ConvertRate;
                    itemE.QuantityInStock += InsertItem;
                    _context.stocks.Update(itemE);
                    details.RemoveAll(x => x.ID == CountItem.ID);
                }
                //TH khong đúng hạn xử dụng thì thêm mới vào kho
                //var idExist = existStock.Select(x => x.ProductId.ToString());
                foreach (var item in details)
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
                _context.SaveChanges();
            }
            //thêm vào kho


            //tìm ra đúng loại và cập nhật số lượng

            //còn không thì thêm thếm
            //còn không thì thêm mới
            TempData["ResultOk"] = "Tạo dữ liệu thành công !";
            return RedirectToAction("Index");
            return View(empobj);
            //}
        }

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Guid Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var empfromdb = _context.ImportsProduct.Find(Id);
            var empfromdbDetails = _context.ImportProductDetails.Where(x => x.ImportProductId == empfromdb.ID.ToString());
            var viewModel = new ImportViewModel();
            viewModel.ImportMaster = new Import
            {
                ImportCode = empfromdb.ImportCode,
                ImportName = empfromdb.ImportName,
                ID = empfromdb.ID,
                ImportDate = empfromdb.ImportDate,
                AccountId = empfromdb.AccountId,
                SupplierId = empfromdb.SupplierId,
                Description = empfromdb.Description,
                TotalAmount = Math.Round((decimal)empfromdb.TotalAmount, 2),
                Status = empfromdb.Status,
            };

            foreach (var item in empfromdbDetails)
            {
                viewModel.ProductDetails.Add(new ImportProducts
                {
                    ID = item.ID,
                    ImportProductId = item.ImportProductId,
                    ProduceId = item.ProduceId,
                    Description = item.Description,
                    ProductionBatch = item.ProductionBatch,
                    ManufacturingDate = item.ManufacturingDate,
                    ExpirationData = item.ExpirationData,
                    Unit = item.Unit,
                    ConvertRate = item.ConvertRate,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    ImportPrice = item.ImportPrice,
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
                        UnitProductId= item.UnitProductId,
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

                //TH nếu cùng hạn xử dụng thì thêm vào đúng lo
                foreach (var item in existStock)
                {
                    var id = item.ID;
                    var itemE = _context.stocks.Find(id);
                    var CountItem = details.Where(x => x.ProduceId == item.ProductId && x.ExpirationData.Value.Date == item.ExpirationData.Value).FirstOrDefault();
                    var InsertItem = CountItem.Quantity * (int)CountItem.ConvertRate;
                    itemE.QuantityInStock += InsertItem;
                    _context.stocks.Update(itemE);
                    details.RemoveAll(x =>x.ID == CountItem.ID);
                }
                //TH khong đúng hạn xử dụng thì thêm mới vào kho
                //var idExist = existStock.Select(x => x.ProductId.ToString());
                foreach (var item in details)
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
