using System.Text.RegularExpressions;

namespace Base_Asp_Core_MVC_with_Identity.CommonMethod
{
    public static class BatchCodeHelper
    {
        /// <summary>
        /// Tạo mã lô dạng: ten-thuoc-yyyyMMdd-yyyyMMdd-xx
        /// </summary>
        public static string GenerateBatchCode(
            string productName,
            DateTime manufacturingDate,
            DateTime importDate,
            int index)
        {
            // Chuẩn hoá tên thuốc thành dạng slug: panadol-500mg -> panadol-500mg
            var normalizedName = productName.ToLowerInvariant();
            normalizedName = Regex.Replace(normalizedName, @"[^a-z0-9]+", "-");
            normalizedName = normalizedName.Trim('-');

            return $"{normalizedName}-{manufacturingDate:yyyyMMdd}-{importDate:yyyyMMdd}-{index:00}";
        }
    }
}
