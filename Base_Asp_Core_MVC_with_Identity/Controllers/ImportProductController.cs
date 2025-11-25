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
            var productList = (from p in _context.Products
                               join s in _context.suppliers on p.SupplierId equals s.ID.ToString()
                               select new SelectListItem
                               {
                                   Value = p.ID.ToString(),
                                   Text = $"{p.ProductName} - {s.SupplierName}"
                               }).ToList();
            ViewData["ProductList"] = productList;

            var unitList = _context.productUnits
                .Select(u => new SelectListItem
                {
                    Value = u.ID.ToString(),
                    Text = u.UnitName
                })
                .ToList();
            ViewData["UnitList"] = unitList;

            var accountList = _userManager.Users
                .Select(p => new SelectListItem
                {
                    Value = p.Id,
                    Text = p.FirstName + " " + p.LastName
                })
                .ToList();
            ViewData["AccountList"] = accountList;

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

            // Mặc định trạng thái là ĐANG CHỜ – user không tự chọn nữa
            viewModel.ImportMaster.Status = (int)EnumApprodImport.Wait;

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

            // tạm fix SupplierId
            empobj.ImportMaster.SupplierId = "08dc620d-b70b-4bec-8957-92617c38b23b";

            // luôn set trạng thái Waiting khi tạo mới
            empobj.ImportMaster.Status = (int)EnumApprodImport.Wait;

            // ----- MASTER -----
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
                    if (string.IsNullOrEmpty(item.ProduceId))
                        continue;

                    var product = _context.Products
                        .FirstOrDefault(p => p.ID.ToString() == item.ProduceId);

                    var manufacturingDate = item.ManufacturingDate ?? DateTime.Now.Date;
                    var importDate = master.ImportDate ?? DateTime.Now;

                    var existingCount = _context.stocks.Count(w =>
                        w.ProductId == item.ProduceId &&
                        w.ManufacturingDate.HasValue &&
                        w.ManufacturingDate.Value.Date == manufacturingDate.Date &&
                        w.ProductionBatch.HasValue &&
                        w.ProductionBatch.Value.Date == importDate.Date);

                    var index = existingCount + 1;

                    var batchCode = BatchCodeHelper.GenerateBatchCode(
                        product?.ProductName ?? "unknown",
                        manufacturingDate,
                        importDate,
                        index
                    );

                    var detailEntity = new ImportProducts
                    {
                        ImportProductId = master.ID.ToString(),
                        Description = item.Description ?? "Không",
                        ProductionBatch = importDate,
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
                        BatchCode = batchCode
                    };

                    details.Add(detailEntity);
                }
            }

            _context.ImportProductDetails.AddRange(details);
            _context.SaveChanges();

            // LÚC CREATE KHÔNG CẬP NHẬT TỒN KHO NỮA
            // chỉ cập nhật khi ấn nút Phê duyệt

            TempData["ResultOk"] = "Tạo dữ liệu thành công (đang chờ phê duyệt)!";
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
                    BatchCode = item.BatchCode
                });
            }

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

            // LẤY LẠI STATUS GỐC TỪ DB – KHÔNG CHO EDIT THAY ĐỔI
            var oldMaster = _context.ImportsProduct
                .AsNoTracking()
                .FirstOrDefault(x => x.ID == empobj.ImportMaster.ID);

            var status = oldMaster?.Status ?? empobj.ImportMaster.Status;

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
                Status = status   // giữ nguyên trạng thái
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
                            Description = item.Description ?? "Không",
                            ProductionBatch = item.ProductionBatch,
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
                            BatchCode = item.BatchCode
                        });
                    }
                }
            }

            _context.ImportProductDetails.UpdateRange(details);
            _context.SaveChanges();

            TempData["ResultOk"] = "Cập nhập dữ liệu thành công !";
            return RedirectToAction("Index");
        }

        // ======================= PHÊ DUYỆT + CẬP NHẬT TỒN KHO =======================
        [HttpPost]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Approve(Guid id)
        {
            var import = _context.ImportsProduct.FirstOrDefault(x => x.ID == id);
            if (import == null)
            {
                return Json(new { success = false, message = "Không tìm thấy phiếu nhập." });
            }

            if (import.Status == (int)EnumApprodImport.approved)
            {
                return Json(new { success = false, message = "Phiếu nhập này đã được phê duyệt trước đó." });
            }

            var details = _context.ImportProductDetails
                .Where(d => d.ImportProductId == import.ID.ToString())
                .ToList();

            if (!details.Any())
            {
                return Json(new { success = false, message = "Phiếu nhập không có chi tiết để cập nhật kho." });
            }

            var importDate = import.ImportDate ?? DateTime.Now;

            foreach (var item in details)
            {
                var baseQty = (item.Quantity ?? 0) * (int)(item.ConvertRate ?? 1);

                var existingStock = _context.stocks.FirstOrDefault(w =>
                    w.ProductId == item.ProduceId &&
                    w.BatchCode == item.BatchCode);

                if (existingStock != null)
                {
                    existingStock.QuantityInStock =
                        (existingStock.QuantityInStock ?? 0) + baseQty;

                    existingStock.ExpirationData = item.ExpirationData;
                    existingStock.ManufacturingDate = item.ManufacturingDate;
                    existingStock.ProductionBatch = item.ProductionBatch ?? importDate;
                    existingStock.TotalValueImport = item.Price;

                    _context.stocks.Update(existingStock);
                }
                else
                {
                    var stock = new Warehouse
                    {
                        ProductId = item.ProduceId,
                        BatchCode = item.BatchCode,
                        ExpirationData = item.ExpirationData,
                        ManufacturingDate = item.ManufacturingDate,
                        ProductionBatch = item.ProductionBatch ?? importDate,
                        TotalValueImport = item.Price,
                        QuantityInStock = baseQty
                    };
                    _context.stocks.Add(stock);
                }
            }

            // cập nhật trạng thái -> approved
            import.Status = (int)EnumApprodImport.approved;
            _context.ImportsProduct.Update(import);

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Đã phê duyệt phiếu nhập và cập nhật tồn kho theo lô."
            });
        }
    }
}
