using Base_Asp_Core_MVC_with_Identity.CommonFile.IServiceCommon;
using Base_Asp_Core_MVC_with_Identity.Data;
using System.Linq.Expressions;
using System.Text;

namespace Base_Asp_Core_MVC_with_Identity.CommonFile.ServiceCommon
{
    public class CommonService : ICommonService
    {
        private readonly Base_Asp_Core_MVC_with_IdentityContext _context;
        public CommonService(Base_Asp_Core_MVC_with_IdentityContext context)
        {
            _context = context;
        }

        public string GenerateCategoryCode<T>(
            string prefix,
            Expression<Func<T, string>> codeSelector
        ) where T : class
        {
            // 1. lấy ngày hiện tại
            var today = DateTime.Now.ToString("yyyyMMdd");

            // 2. prefix chuẩn: ORDER_20251119_
            var datePrefix = $"{prefix}{today}_";

            // 3. lấy DbSet tương ứng
            var dbSet = _context.Set<T>();

            // 4. lấy tất cả code bắt đầu bằng "ORDER_20251119_"
            var existingCodes = dbSet
                .Select(codeSelector)
                .Where(code => code.StartsWith(datePrefix))
                .ToList();

            // 5. lấy số thứ tự lớn nhất
            int maxSeq = 0;

            foreach (var code in existingCodes)
            {
                // tách phần cuối: ORDER_20251119_0001 → 0001
                var parts = code.Split('_');
                var seqStr = parts.LastOrDefault();

                if (int.TryParse(seqStr, out int seq))
                {
                    if (seq > maxSeq)
                        maxSeq = seq;
                }
            }

            // 6. số mới = max + 1
            int newSeq = maxSeq + 1;

            // 7. ghép chuỗi hoàn chỉnh: ORDER_20251119_0001
            return $"{datePrefix}{newSeq:D4}";
        }
    }
}
