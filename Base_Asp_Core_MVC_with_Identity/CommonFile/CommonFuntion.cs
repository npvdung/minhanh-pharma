using System.Reflection;

namespace Base_Asp_Core_MVC_with_Identity.CommonMethod
{
    public  static class CommonFuntion
    {
        public static string GetDisplayName(this Object value)
        {
            if (value == null)
                return string.Empty;

            var enumType = value.GetType();
            if (!enumType.IsEnum)
                throw new ArgumentException("Value must be of Enum type.");

            var displayAttribute = enumType
                .GetMember(value.ToString())
                .FirstOrDefault()
                ?.GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.GetName() ?? value.ToString();
        }
    }
}
