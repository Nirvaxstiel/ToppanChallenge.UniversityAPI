using System.Collections.Generic;
using System.Linq;

namespace System
{
    public sealed class ValueHelper
    {
        public static bool IsNullOrEmpty(object value)
        {
            return value == null || value == DBNull.Value || string.IsNullOrEmpty(value.ToString().Trim());
        }

        public static bool IsNullOrEmpty<TModel>(IEnumerable<TModel> items)
        {
            return items != null && items.Count() > 0 ? false : true;
        }

        public static string GetTextValue(object value)
        {
            return IsNullOrEmpty(value) ? string.Empty : value.ToString();
        }
    }
}
