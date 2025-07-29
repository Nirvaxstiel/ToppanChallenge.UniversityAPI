namespace System
{
    public sealed class ValueHelper
    {
        public static bool IsNullOrEmpty(object value)
        {
            if (value == null || value == DBNull.Value)
                return true;
            var str = value.ToString();
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool IsNullOrEmpty<TModel>(IEnumerable<TModel> items)
        {
            return items != null && items.Count() > 0 ? false : true;
        }

        public static string GetTextValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;
            var str = value.ToString();
            return IsNullOrEmpty(str) ? string.Empty : str;
        }
    }
}