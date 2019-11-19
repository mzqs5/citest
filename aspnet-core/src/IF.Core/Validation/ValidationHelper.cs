using System.Text.RegularExpressions;
using Abp.Extensions;

namespace IF.Validation
{
    public static class ValidationHelper
    {
        public const string EmailRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        public static bool IsEmail(string value)
        {
            if (value.IsNullOrEmpty())
            {
                return false;
            }
            return true;
            //var regex = new Regex(EmailRegex);
            //return regex.IsMatch(value);
        }
    }
}
