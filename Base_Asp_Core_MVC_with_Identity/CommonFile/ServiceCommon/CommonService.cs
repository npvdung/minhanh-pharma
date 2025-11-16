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

        public string GenerateCategoryCode<T>(string prefix, Expression<Func<T, string>> codeSelector) where T : class
        {
            var existingCodes = _context.Set<T>().Select(codeSelector).ToList();
            string newCode = prefix + GenerateUniqueSuffix(existingCodes);
            return newCode;
        }

        private string GenerateUniqueSuffix(List<string> existingCodes)
        {
            var random = new Random();
            const string chars = "ABCDE123456789";
            var suffixBuilder = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                suffixBuilder.Append(chars[random.Next(chars.Length)]);
            }
            string suffix = suffixBuilder.ToString();

            while (existingCodes.Contains(suffix))
            {
                suffix = GenerateUniqueSuffix(existingCodes);
            }

            return suffix;
        }
    }
}
