namespace UniversityAPI.Utility.Helpers.Extensions
{
    using System.Data;

    public static class StringExtension
    {
        public static IEnumerable<int> AsInts(this string source)
        {
            return source.AsInts(',');
        }

        public static IEnumerable<int> AsInts(this string source, char separator)
        {
            return source.Split(separator).Select(s => ConvertToInt(s));
        }

        public static IEnumerable<Guid> AsGuids(this string source)
        {
            return source.AsGuids(',');
        }

        public static IEnumerable<Guid> AsGuids(this string source, char separator)
        {
            return source.Split(separator).Select(s => ConvertToGuid(s));
        }

        private static Guid ConvertToGuid(string value)
        {
            var result = Guid.Empty;
            Guid.TryParse(value, out result);

            return result;
        }

        public static int ConvertToInt(string value)
        {
            int result = default;
            int.TryParse(value, out result);

            return result;
        }

        public static string[] DefaultSplit(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return [];
            }

            value = value.Replace(";", ",").Replace("，", ",").Replace("；", ",");

            return value.Split(',');
        }
    }
}