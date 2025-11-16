using Base_Asp_Core_MVC_with_Identity.Models;
using System.Linq.Expressions;

namespace Base_Asp_Core_MVC_with_Identity.CommonFile.IServiceCommon
{
    public interface ICommonService
    {
        string GenerateCategoryCode<T>(string prefix, Expression<Func<T, string>> codeSelector) where T : class;
    }
}
