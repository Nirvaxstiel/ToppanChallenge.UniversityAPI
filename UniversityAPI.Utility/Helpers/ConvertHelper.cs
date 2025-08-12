namespace UniversityAPI.Utility.Helpers
{
    using System.Data;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Text;
    using UniversityAPI.Utility.Helpers.Extensions;

    public sealed class ConvertHelper
    {
        private static IEnumerable<string> alphaList = new List<string>()
        {
            string.Empty, "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
        };

        //public static int ToInt(object p)
        //{
        //    throw new NotImplementedException();
        //}

        #region Convert To DateTime

        public static DateTime ToDate(string value, string format, IFormatProvider formatProvider)
        {
            var dateTime = default(DateTime);
            DateTime.TryParseExact(value, format, formatProvider, DateTimeStyles.None, out dateTime);

            return dateTime;
        }

        public static DateTime ToDate(string value, string format)
        {
            return ToDate(value, format, CultureInfo.CurrentCulture);
        }

        public static DateTime ToDate(string value)
        {
            var dateTime = default(DateTime);
            DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime);

            return dateTime;
        }

        public static DateTime ToDate(object value)
        {
            return value == null ? default : ToDate(value.ToString());
        }

        public static DateTime? ToNullableDate(string value, string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return ToDate(value, format, formatProvider);
        }

        public static DateTime? ToNullableDate(string value, string format)
        {
            return ToNullableDate(value, format, CultureInfo.CurrentCulture);
        }

        public static DateTime? ToNullableDate(object value)
        {
            if (ValueHelper.IsNullOrEmpty(value))
            {
                return null;
            }

            return ToDate(value.ToString());
        }

        #endregion Convert To DateTime

        #region Convert To Int

        public static int ToInt(string value)
        {
            var result = default(int);
            int.TryParse(value, out result);

            return result;
        }

        public static int ToInt(object value)
        {
            return value == null ? default : ToInt(value.ToString());
        }

        public static int? ToNullableInt(object value)
        {
            if (ValueHelper.IsNullOrEmpty(value))
            {
                return null;
            }

            return ToInt(value.ToString());
        }

        public static long ToInt64(string value)
        {
            var result = default(long);
            long.TryParse(value, out result);

            return result;
        }

        public static long ToInt64(object value)
        {
            return value == null ? default : ToInt64(value.ToString());
        }

        public static long? ToNullableInt64(object value)
        {
            if (ValueHelper.IsNullOrEmpty(value))
            {
                return null;
            }

            return ToInt64(value.ToString());
        }

        #endregion Convert To Int

        #region Convert To Decimal

        public static decimal ToDecimal(string value)
        {
            var result = default(decimal);
            decimal.TryParse(value, out result);

            return result;
        }

        public static decimal ToDecimal(object value)
        {
            return value == null ? default : ToDecimal(value.ToString());
        }

        public static decimal? ToNullableDecimal(object value)
        {
            if (ValueHelper.IsNullOrEmpty(value))
            {
                return null;
            }

            return ToDecimal(value.ToString());
        }

        #endregion Convert To Decimal

        #region Convert To Double

        public static double ToDouble(string value)
        {
            double result = default;
            double.TryParse(value, out result);

            return result;
        }

        public static double ToDouble(object value)
        {
            return value == null ? default : ToDouble(value.ToString());
        }

        public static double? ToNullableDouble(object value)
        {
            if (ValueHelper.IsNullOrEmpty(value))
            {
                return null;
            }

            return ToDouble(value);
        }

        #endregion Convert To Double

        #region Convert To Bool

        public static bool ToBool(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (StringHelper.Compare(value, "1") || StringHelper.Compare(value, "yes"))
            {
                return true;
            }

            if (StringHelper.Compare(value, "0") || StringHelper.Compare(value, "no"))
            {
                return false;
            }

            var result = false;
            bool.TryParse(value, out result);

            return result;
        }

        public static bool ToBool(object value)
        {
            return value == null ? false : ToBool(value.ToString());
        }

        public static bool? ToNullableBool(object value)
        {
            if (ValueHelper.IsNullOrEmpty(value))
            {
                return null;
            }

            return ToBool(value.ToString());
        }

        #endregion Convert To Bool

        #region Convert To String

        public static string ToString(object value)
        {
            if (ValueHelper.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            var str = value.ToString();
            return str ?? string.Empty;
        }

        public static string ToBase64String(string text)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(utf8Bytes);
        }

        public static string HexToBase64(string param)
        {
            try
            {
                var bytes = new byte[param.Length / 2];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(param.Substring(i * 2, 2), 16);
                }
                return Convert.ToBase64String(bytes);
            }
            catch (Exception)
            {
                return "-1";
            }
        }

        #endregion Convert To String

        #region Convert To Guid

        public static Guid ToGuid(string value)
        {
            var result = Guid.Empty;
            Guid.TryParse(value, out result);

            return result;
        }

        public static Guid ToGuid(object value)
        {
            return value == null ? Guid.Empty : ToGuid(value.ToString());
        }

        public static Guid? ToNullableGuid(object value)
        {
            if (ValueHelper.IsNullOrEmpty(value))
            {
                return null;
            }

            return ToGuid(value.ToString());
        }

        #endregion Convert To Guid

        #region Convert to Timpspan

        public static TimeSpan ToTimeSpan(string value)
        {
            return ToDate(value).TimeOfDay;
        }

        public static TimeSpan ToTimeSpan(object value)
        {
            return value == null ? default : ToTimeSpan(value.ToString());
        }

        #endregion Convert to Timpspan

        #region Convert to SqlDateTime

        public static DateTime ToSqlDate(object value)
        {
            return ToSqlMinDate(value);
        }

        public static DateTime ToSqlMinDate(object value)
        {
            if (value == null)
            {
                return SqlDateTime.MinValue.Value;
            }

            var result = ToDate(value.ToString());
            return result == default ? SqlDateTime.MinValue.Value : result;
        }

        public static DateTime ToSqlMaxDate(object value)
        {
            if (value == null)
            {
                return SqlDateTime.MaxValue.Value;
            }

            var result = ToDate(value.ToString());
            return result == default ? SqlDateTime.MaxValue.Value : result;
        }

        #endregion Convert to SqlDateTime

        public static string ToStringInvariantCulture(string format, decimal value)
        {
            return string.Format(CultureInfo.InvariantCulture, format, value);
        }

        public static string ToStringInvariantCulture(string format, decimal value1, decimal value2)
        {
            return string.Format(CultureInfo.InvariantCulture, format, value1, value2);
        }

        public static IEnumerable<string> ToKeys(object value)
        {
            if (value != null)
            {
                return PropertyHelper.GetProperties(value).Select(helper => helper.Name);
            }

            return new string[] { };
        }

        public static IDictionary<string, object> ToPairs(object value)
        {
            var pairs = new Dictionary<string, object>();
            if (value != null)
            {
                PropertyHelper.GetProperties(value).Each(helper => pairs.Add(helper.Name, helper.GetValue(value)));
            }

            return pairs;
        }

        public static string ToHtml(DataTable table)
        {
            StringBuilder html = new();

            html.Append("<table style='border-collapse: collapse;' border=1 >");
            html.Append("<tr>");

            foreach (DataColumn column in table.Columns)
            {
                html.Append("<th>");
                html.Append(column.ColumnName);
                html.Append("</th>");
            }

            html.Append("</tr>");

            foreach (DataRow row in table.Rows)
            {
                html.Append("<tr>");
                foreach (DataColumn column in table.Columns)
                {
                    html.Append("<td>");
                    html.Append(row[column.ColumnName]);
                    html.Append("</td>");
                }
                html.Append("</tr>");
            }

            html.Append("</table>");

            return html.ToString();
        }

        public static string ToHtmlTableVerticalHeader(DataTable table)
        {
            StringBuilder html = new();

            html.Append("<table style='border-collapse: collapse;' border=1 >");
            foreach (DataRow row in table.Rows)
            {
                html.Append("<tr>");
                foreach (DataColumn column in table.Columns)
                {
                    var tableDataOpeningTag = "<td>";
                    var tableDataClosingTag = "</td>";

                    if (column.Ordinal == 0)
                    {
                        tableDataOpeningTag = "<th style='text-align: left;'>";
                        tableDataClosingTag = "</th>";
                    }

                    html.Append(tableDataOpeningTag);
                    html.Append(row[column.ColumnName]);
                    html.Append(tableDataClosingTag);
                }
                html.Append("</tr>");
            }

            html.Append("</table>");

            return html.ToString();
        }
    }
}