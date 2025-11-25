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
using Rotativa.AspNetCore;
using System.Globalization;
using System.Linq.Expressions;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class ReturnProductController : Controller
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        private readonly UserManager<UserSystemIdentity> _userManager;
        private readonly ICommonService _commonService;

        public ReturnProductController(
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
            IEnumerable<DisposalRecords> objCatlist = _context.ReturnProducts;
            return View(objCatlist);
        }

        // ========================= CREATE - GET =========================
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create()
        {
            string prefix = "RET_";
            Expression<Func<DisposalRecords, string>> codeSelector = c => c.ImportId;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);

            var viewModel = new ReturnViewModel
            {
                disposalRecordsMaster = new DisposalRecords
                {
                    ImportId = autoCode,            // mã phiếu trả hàng
                    ExportDate = DateTime.Now,      // ngày xuất gốc (nếu cần)
                    ReturnDate = DateTime.Now,      // ngày trả
                    Description = string.Empty,
                    Reason = string.Empty
                },
                // tạo list trống, JS sẽ render dần
                ReturnsDetails = new List<DisposalProducts>()
            };

            // --------- Dropdown chọn lô trong kho ----------
            var productList = (from w in _context.stocks
                               join p in _context.Products on w.ProductId equals p.ID.ToString()
                               select new
                               {
                                   ImportId = w.ID.ToString(),           // ID của lô
                                   ProductName = p.ProductName,
                                   BatchCode = w.ProductionBatch,
                                   ExpirationData = w.ExpirationData,
                                   Quantity = w.QuantityInStock
                               }).ToList();

            var productItems = new List<SelectListItem>();
            foreach (var item in productList)
            {
                var exp = item.ExpirationData?.ToString("yyyy-MM-dd") ?? "";
                productItems.Add(new SelectListItem
                {
                    // chọn theo lô: Tên – Lô – HSD – SL tồn
                    Text = $"{item.ProductName} - Lô: {item.BatchCode} - HSD: {exp} - SL tồn: {item.Quantity}",
                    Value = item.ImportId
                });
            }
            ViewData["product_lst"] = productItems;

            // --------- Đơn vị tính ----------
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
            ViewData["unit_lst"] = unitItems;

            // --------- Tài khoản người lập phiếu ----------
            var accountData = (from u in _userManager.Users
                               select new
                               {
                                   Id = u.Id,
                                   Name = u.FirstName + " " + u.LastName
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
            ViewData["account_lst"] = accountItems;

            return View(viewModel);
        }

        // ========================= CREATE - POST =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Create(ReturnViewModel empobj)
        {
            // sinh lại mã đề phòng user sửa trong form
            string prefix = "RET_";
            Expression<Func<DisposalRecords, string>> codeSelector = c => c.ImportId;
            string autoCode = _commonService.GenerateCategoryCode(prefix, codeSelector);

            // GÁN ID MỚI CHO MASTER LUÔN, để dùng cho DisposalRecordsId
            var master = new DisposalRecords
            {
                ID = Guid.NewGuid(),
                ImportId = autoCode, // mã phiếu trả hàng
                ExportDate = empobj.disposalRecordsMaster.ExportDate ?? DateTime.Now,
                UserId = empobj.disposalRecordsMaster.UserId,
                // nếu sau này có Supplier thật thì map lại, tạm thời hard-code giống bản cũ
                SupplierId = "0d413f2c-1d8d-4c9e-a237-c6abdfc12f2b",
                Description = empobj.disposalRecordsMaster.Description,
                TotalAmount = 0, // sẽ tính lại bên dưới
                Reason = empobj.disposalRecordsMaster.Reason,
                ReturnDate = empobj.disposalRecordsMaster.ReturnDate ?? DateTime.Now
            };

            var details = new List<DisposalProducts>();

            // LƯU Ý: ProductId trên form đang chính là ID lô (stocks.ID)
            foreach (var item in empobj.ReturnsDetails)
            {
                if (!string.IsNullOrEmpty(item.ProductId))   // kiểm tra ProductId, KHÔNG phải ImportId
                {
                    // ProductId = ID của lô trong bảng stocks
                    var stockLot = _context.stocks.Find(Guid.Parse(item.ProductId));
                    if (stockLot == null) continue;

                    var quantity = item.Quantity ?? 0;
                    int rate = 1;
                    int.TryParse(item.Description, out rate);   // Description = tỉ lệ quy đổi

                    var price = item.Price ?? 0;                // giá nhập / đơn vị đang chọn
                    var lineTotal = quantity * price;

                    var detail = new DisposalProducts
                    {
                        DisposalRecordsId = master.ID.ToString(),   // dùng master.ID đã gán Guid.NewGuid()
                        ProductId = item.ProductId,                 // LƯU LẠI ID LÔ (stocks.ID)
                        Description = item.Description,             // tỉ lệ
                        Unit = item.Unit,                           // nếu có dùng
                        Quantity = quantity,
                        Price = price,
                        ImportPrice = price,                        // nếu muốn giá nhập = giá trả
                        TotalAmount = lineTotal,
                        UnitProductId = item.UnitProductId
                    };

                    details.Add(detail);
                }
            }

            // Tổng tiền phiếu trả = tổng thành tiền từng dòng
            master.TotalAmount = details.Sum(d => d.TotalAmount) ?? 0;

            // Lưu master + detail
            _context.ReturnProducts.Add(master);
            if (details.Any())
            {
                _context.Return_Product_Details.AddRange(details);
            }
            _context.SaveChanges();

            // =================== Cập nhật tồn kho: GIẢM THEO LÔ ===================
            foreach (var item in details)
            {
                // ProductId = ID của Warehouse (lô)
                var stockLot = _context.stocks
                    .FirstOrDefault(x => x.ID.ToString() == item.ProductId);

                if (stockLot == null) continue;

                int rate = 1;
                int.TryParse(item.Description, out rate); // tỉ lệ quy đổi sang đơn vị gốc

                // giảm tồn theo lô: QuantityInStock -= Quantity * Rate
                stockLot.QuantityInStock -= (item.Quantity ?? 0) * rate;
                if (stockLot.QuantityInStock < 0) stockLot.QuantityInStock = 0;

                _context.stocks.Update(stockLot);
            }

            _context.SaveChanges();

            TempData["ResultOk"] = "Tạo phiếu trả hàng thành công !";
            return RedirectToAction("Index");
        }

        // ========================= EDIT - GET (XEM CHI TIẾT) =========================
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Edit(Guid Id)
        {
            var empfromdb = _context.ReturnProducts.Find(Id);
            if (empfromdb == null)
            {
                return NotFound();
            }

            var empfromdbDetails = _context.Return_Product_Details
                .Where(x => x.DisposalRecordsId == empfromdb.ID.ToString())
                .ToList();

            var viewModel = new ReturnViewModel
            {
                disposalRecordsMaster = new DisposalRecords
                {
                    ID = empfromdb.ID,
                    SupplierId = empfromdb.SupplierId,
                    Description = empfromdb.Description,
                    ImportId = empfromdb.ImportId,
                    UserId = empfromdb.UserId,
                    TotalAmount = Math.Round((decimal)(empfromdb.TotalAmount ?? 0), 2),
                    Reason = empfromdb.Reason,
                    ReturnDate = empfromdb.ReturnDate
                },
                // QUAN TRỌNG: khởi tạo list
                ReturnsDetails = new List<DisposalProducts>()
            };

            foreach (var item in empfromdbDetails)
            {
                viewModel.ReturnsDetails.Add(new DisposalProducts
                {
                    ID = item.ID,
                    ProductId = item.ProductId,
                    ImportId = item.ImportId,
                    UnitProductId = item.UnitProductId,
                    Description = item.Description,
                    ReturnDate = item.ReturnDate,
                    Unit = item.Unit,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    ImportPrice = item.ImportPrice,
                    TotalAmount = item.TotalAmount
                });
            }

            // Dropdown sản phẩm theo lô (giống Create, chỉ để hiển thị)
            var productList = (from w in _context.stocks
                               join p in _context.Products on w.ProductId equals p.ID.ToString()
                               select new
                               {
                                   ImportId = w.ID.ToString(),
                                   ProductName = p.ProductName,
                                   BatchCode = w.ProductionBatch,
                                   ExpirationData = w.ExpirationData,
                                   Quantity = w.QuantityInStock
                               }).ToList();

            var productItems = new List<SelectListItem>();
            foreach (var item in productList)
            {
                var exp = item.ExpirationData?.ToString("yyyy-MM-dd") ?? "";
                productItems.Add(new SelectListItem
                {
                    Text = $"{item.ProductName} - Lô: {item.BatchCode} - HSD: {exp} - SL tồn: {item.Quantity}",
                    Value = item.ImportId
                });
            }
            ViewData["product_lst"] = productItems;

            // Đơn vị tính
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
            ViewData["unit_lst"] = unitItems;

            // Tài khoản
            var accountData = (from u in _userManager.Users
                               select new
                               {
                                   Id = u.Id,
                                   Name = u.FirstName + " " + u.LastName
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
            ViewData["account_lst"] = accountItems;

            if (viewModel == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }

        // ========================= PRINT - XUẤT PDF PHIẾU TRẢ =========================
        [Authorize(Roles = "Admin, Employee")]
        public IActionResult Print(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var master = _context.ReturnProducts.Find(id);
            if (master == null)
            {
                return NotFound();
            }

            var details = _context.Return_Product_Details
                                .Where(d => d.DisposalRecordsId == master.ID.ToString())
                                .ToList();

            var viewModel = new ReturnViewModel();

            // ----- thông tin master -----
            var userName = _userManager.Users
                            .Where(u => u.Id == master.UserId)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault() ?? "";

            viewModel.disposalRecordsMaster = new DisposalRecords
            {
                ImportId = master.ImportId,              // mã trả hàng
                ReturnDate = master.ReturnDate,          // ngày trả hàng
                UserId = userName,                       // tên người lập
                TotalAmount = master.TotalAmount,        // tổng tiền
                Description = master.Description,        // mô tả
                Reason = master.Reason                   // lí do trả
            };

            // ----- chi tiết -----
            foreach (var d in details)
            {
                string productDisplay = "";
                string unitName = "";
                string batchCode = "";

                var stock = _context.stocks
                                    .FirstOrDefault(s => s.ID.ToString() == d.ImportId);

                if (stock != null)
                {
                    var product = _context.Products
                                        .FirstOrDefault(p => p.ID.ToString() == stock.ProductId);

                    var name = product?.ProductName ?? "";
                    var batch = stock.ProductionBatch?.ToString("yyyy-MM-dd") ?? "";
                    var exp = stock.ExpirationData?.ToString("yyyy-MM-dd") ?? "";

                    productDisplay = $"{name} - HSD: {exp}";
                    batchCode = batch;
                }

                if (!string.IsNullOrEmpty(d.UnitProductId))
                {
                    unitName = _context.productUnits
                                    .Where(u => u.ID.ToString() == d.UnitProductId)
                                    .Select(u => u.UnitName)
                                    .FirstOrDefault() ?? "";
                }

                viewModel.ReturnsDetails.Add(new DisposalProducts
                {
                    Description = d.Description,
                    Quantity = d.Quantity,
                    TotalAmount = d.TotalAmount,
                    Price = d.Price,
                    ProductId = productDisplay,
                    UnitProductId = unitName,
                    ImportId = batchCode
                });
            }

            return new ViewAsPdf("Print", viewModel)
            {
                FileName = $"{viewModel.disposalRecordsMaster.ImportId}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }
    }
}
