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

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class ImportProductController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private readonly UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public ImportProductController(
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
            // Product + Supplier
            var productList = (from p in _context.Products
                               join s in _context.suppliers on p.SupplierId equals s.ID.ToString()
                               select new SelectListItem
                               {
                                   Value = p.ID.ToString(),
                                   Text = $"{p.ProductName} - {s.SupplierName}"
                               }).ToList();
            ViewData["ProductList"] = productList;

            // Unit
            var unitList = _context.productUnits
                .Select(u => new SelectListItem
                {
                    Value = u.ID.ToString(),
                    Text = u.UnitName
                })
                .ToList();
            ViewData["UnitList"] = unitList;

            // Account/User
            var accountList = _userManager.Users
                .Select(p => new SelectListItem
                {
                    Value = p.Id,
                    Text = p.FirstName + " " + p.LastName
                })
                .ToList();
            ViewData["AccountList"] = accountList;

            // Status
            var statusList = Enum.GetValues(typeof(EnumApprodImport))
                .Cast<EnumApprodImport>()
                .Select(e => new SelectListItem
                {
                    Text = e.GetDisplayName(),
                    Value = ((int)e).ToString()
                })
                .ToList();
            ViewData["StatusList"] = statusList;
        }
        //----------------------------------------------------------------

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Index()
        {
            IEnumerable<Import> objCatlist = _context.ImportsProduct;
            return View(objCatlist);
        }

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            string prefix = "IP_";
            Expression<Func<Import, string>> codeSelector = c => c.ImportCode;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);

            var viewModel = new ImportViewModel();
            viewModel.ImportMaster.ImportCode = autoCode;
            viewModel.ImportMaster.Description = "Không";
            viewModel.ImportMaster.ImportDate = DateTime.Now;

            // chuẩn bị dropdown
            PopulateDropDowns();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(ImportViewModel empobj)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns();
                return View(empobj);
            }

            // tạm fix SupplierId (bạn có thể thay bằng dropdown sau)
            empobj.ImportMaster.SupplierId = "08dc620d-b70b-4bec-8957-92617c38b23b";

            var master = new Import
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

            var details = new List<ImportProducts>();

            if (empobj.ProductDetails != null)
            {
                foreach (var item in empobj.ProductDetails)
                {
                    if (item.ProduceId != null)
                    {
                        details.Add(new ImportProducts
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
                            UnitProductId = item.UnitProductId,
                        });
                    }
                }
            }

            _context.ImportProductDetails.AddRange(details);
            _context.SaveChanges();

            // Cập nhật kho nếu được duyệt
            if (empobj.ImportMaster.Status == (int)EnumApprodImport.approved)
            {
                List<Warehouse> allStock = _context.stocks.ToList();
                var existStock = allStock.FindAll(x =>
                    details.Select(y => y.ProduceId).Contains(x.ProductId.ToString()) &&
                    details.Select(z => z.ExpirationData).Contains(x.ExpirationData));

                foreach (var item in existStock)
                {
                    var itemE = _context.stocks.Find(item.ID);
                    var countItem = details
                        .Where(x => x.ProduceId == item.ProductId && x.ExpirationData.Value.Date == item.ExpirationData.Value)
                        .FirstOrDefault();

                    var insertItem = countItem.Quantity * (int)countItem.ConvertRate;
                    itemE.QuantityInStock += insertItem;
                    _context.stocks.Update(itemE);

                    details.RemoveAll(x => x.ID == countItem.ID);
                }

                foreach (var item in details)
                {
                    var itemStock = new Warehouse
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

            TempData["ResultOk"] = "Tạo dữ liệu thành công !";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Guid Id)
        {
            var empfromdb = _context.ImportsProduct.Find(Id);
            if (empfromdb == null)
            {
                return NotFound();
            }

            var empfromdbDetails = _context.ImportProductDetails
                .Where(x => x.ImportProductId == empfromdb.ID.ToString())
                .ToList();

            var viewModel = new ImportViewModel
            {
                ImportMaster = new Import
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
                },
                ProductDetails = new List<ImportProducts>()
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

            // chuẩn bị dropdown
            PopulateDropDowns();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(ImportViewModel empobj)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns();
                return View(empobj);
            }

            var master = new Import
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

            var details = new List<ImportProducts>();

            if (empobj.ProductDetails != null)
            {
                foreach (var item in empobj.ProductDetails)
                {
                    if (item.ProduceId != null)
                    {
                        details.Add(new ImportProducts
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
            }

            _context.ImportProductDetails.UpdateRange(details);
            _context.SaveChanges();

            if (empobj.ImportMaster.Status == (int)EnumApprodImport.approved)
            {
                List<Warehouse> allStock = _context.stocks.ToList();
                var existStock = allStock.FindAll(x =>
                    details.Select(y => y.ProduceId).Contains(x.ProductId.ToString()) &&
                    details.Select(z => z.ExpirationData).Contains(x.ExpirationData));

                foreach (var item in existStock)
                {
                    var itemE = _context.stocks.Find(item.ID);
                    var countItem = details
                        .Where(x => x.ProduceId == item.ProductId && x.ExpirationData.Value.Date == item.ExpirationData.Value)
                        .FirstOrDefault();

                    var insertItem = countItem.Quantity * (int)countItem.ConvertRate;
                    itemE.QuantityInStock += insertItem;
                    _context.stocks.Update(itemE);
                    details.RemoveAll(x => x.ID == countItem.ID);
                }

                foreach (var item in details)
                {
                    var itemStock = new Warehouse
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

            TempData["ResultOk"] = "Cập nhập dữ liệu thành công !";
            return RedirectToAction("Index");
        }
    }
}
