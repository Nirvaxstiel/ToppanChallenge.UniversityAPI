using System.Data;
using System.Text.RegularExpressions;
using System.Web;

namespace UniversityAPI.Helpers
{
    public sealed class StringHelper
    {
        private static string[] ValidAttributes = new string[]
            { "STYLE", "SRC", "ALT", "HREF", "BORDER", "CELLPADDING", "CELLSPACING", "TARGET", "CLASS", "WIDTH", "HEIGHT", "CONTROLS" };

        public const string CAPITAL_LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string LinuxNewLine = "\n";

        private static readonly string[] WildCards = new string[] { "%", "^", "*", "_" };
        private static readonly string[] HtmlSanitizerAllowedTags = new string[] { "video" };

        public static bool Compare(string value1, int value2)
        {
            return Compare(value1, value2.ToString());
        }

        public static bool Compare(string value1, Guid value2)
        {
            return Compare(value1, value2.ToString());
        }

        public static bool Compare(string value1, string value2)
        {
            return string.Equals(ToNoSpace(value1), ToNoSpace(value2), StringComparison.OrdinalIgnoreCase);
        }

        public static bool Compare(object value1, object value2)
        {
            if (value1 == null || value2 == null)
            {
                return false;
            }

            if (value1 == DBNull.Value || value2 == DBNull.Value)
            {
                return false;
            }

            return Compare(value1.ToString(), value2.ToString());
        }

        public static bool CompareCase(string value1, string value2)
        {
            return string.Equals(ToNoSpace(value1), ToNoSpace(value2), StringComparison.Ordinal);
        }

        public static bool Contains(string value, string contain)
        {
            return value == null || contain == null ? false : ToPureUpper(value).Contains(ToPureUpper(contain));
        }

        public static bool Contains(IEnumerable<string> values, string contain)
        {
            return values.Any(value => Compare(value, contain));
        }

        public static bool HasValue(DataRow row, string columnName)
        {
            if (row.IsNull(columnName))
            {
                return false;
            }

            return ValueHelper.IsNullOrEmpty(row[columnName]) == false;
        }

        public static bool IsNullOrEmpty(object value)
        {
            if (value == null)
            {
                return true;
            }
            if (value == DBNull.Value)
            {
                return true;
            }

            return string.IsNullOrEmpty(value.ToString());
        }

        public static bool IsEmptyJson(string json)
        {
            return json == "[]" || json == "{}" || string.IsNullOrWhiteSpace(json);
        }

        public static string Fill(string template, object value)
        {
            var pairs = ConvertHelper.ToPairs(value);
            foreach (var key in pairs.Keys)
            {
                var text = pairs[key] == null ? string.Empty : pairs[key].ToString();
                template = Regex.Replace(template, $"##{key}##".Trim(), text, RegexOptions.IgnoreCase);
            }

            return template;
        }

        public static string Reverse(string value)
        {
            var chars = value.ToCharArray();
            Array.Reverse(chars);

            return new string(chars);
        }

        public static string ToRemoveNewLine(string value)
        {
            var newLine = new char[] { '\n', '\r' };
            return value.TrimEnd(newLine);
        }

        public static string ReplaceNewLine(string value)
        {
            value = value.Replace("\n", "");
            value = value.Replace("\t", "");
            value = value.Replace("\r", "");
            return value;
        }

        public static string Replace(string value, object append)
        {
            Regex reg = new Regex(@"(?is)<img[^>]*?src=(['""\s]?)((?:[^'""\s])*)\1[^>]*?(style=\"")([a-zA-Z0-9:;\.\s\(\)\-\,]*)(\"")>");

            if (!reg.IsMatch(value)) return value;

            var matches = reg.Matches(value);
            foreach (Match match in matches)
            {
                var url = match.Groups[2].Value;
                url = string.Format("{0}{1}", append, url);
                value = value.Replace(match.Groups[2].Value, url);
                value = value.Replace(match.Groups[4].Value, "max-width:100%");
            }

            return value;
        }

        public static string ToPureLowerNoSpace(string value)
        {
            return string.IsNullOrEmpty(value) ? value : value.Replace(" ", "").ToLowerInvariant();
        }

        public static string ToPureLowerNoSpace(object value)
        {
            return value == null ? string.Empty : value.ToString().Replace(" ", "").ToLowerInvariant();
        }

        public static string ToPureUpperNoSpace(string value)
        {
            return string.IsNullOrEmpty(value) ? value : value.Replace(" ", "").ToUpperInvariant();
        }

        public static string ToPureUpperNoSpace(object value)
        {
            return value == null ? string.Empty : value.ToString().Replace(" ", "").ToUpperInvariant();
        }

        public static string ToPureUpper(string value)
        {
            return string.IsNullOrEmpty(value) ? value : value.ToUpperInvariant();
        }

        public static string ToPureUpper(object value)
        {
            return value == null ? string.Empty : value.ToString().ToUpperInvariant();
        }

        public static string ToRemoveExtraSpace(string value)
        {
            var whiteSpace = new char[] { ' ' };
            var split = value.Split(whiteSpace, StringSplitOptions.RemoveEmptyEntries);

            return string.Join(" ", split);
        }

        public static string ToRemoveTabCharacters(string value)
        {
            return value == null ? string.Empty : Regex.Replace(value, @"\s", "");
        }

        public static string ToSafeString(string value)
        {
            return string.IsNullOrEmpty(value) ? value : ToNoHtml(value);
        }

        public static string ToSafeString(object value)
        {
            if (value == null)
            {
                return null;
            }

            return ToSafeString(value.ToString());
        }

        public static string ToUserName(string value)
        {
            return ToPureLowerNoSpace(value);
        }

        public static string ToUserName(object value)
        {
            return value == null ? null : ToPureLowerNoSpace(value);
        }

        public static string ToSubstring(string value, int length)
        {
            return value.Length > length ? value.Substring(0, length) + "..." : value;
        }

        public static string ToNoSpace(object value)
        {
            return ValueHelper.IsNullOrEmpty(value) ? string.Empty : ToNoSpace(value.ToString());
        }

        public static string ToNoSpace(string value)
        {
            return string.IsNullOrEmpty(value) ? value : value.Replace(" ", "");
        }

        public static string ToHtml(string value)
        {
            return value.Replace(Environment.NewLine, @"<br />").Replace(LinuxNewLine, @"<br />");
        }

        public static string ToPartialHtml(string content)
        {
            content = content.Length > 300 ? content.Substring(0, 300) + "..." : content;

            return content.Replace(Environment.NewLine, @"<br />");
        }

        public static bool HasHtml(string input)
        {
            var patterns = new string[]
            {
                "<.*?>",
                "&lt;.*?&gt",
                "<.*//",
            };
            string combinedPattern = string.Join("|", patterns);

            return Regex.IsMatch(input, combinedPattern);
        }

        public static string ToNoHtml(object value)
        {
            return IsNullOrEmpty(value) ? string.Empty : ToNoHtml(value.ToString());
        }

        public static string ToRemoveSpecialCharacters(string value)
        {
            if (IsNullOrEmpty(value))
            {
                return value;
            }
            value = ReplaceNewLine(value);
            value = value.Replace("\u0027", "'");
            // return string.IsNullOrEmpty(value) ? value : value.Replace('"', ' ').Trim();
            return value;
        }

        public static string ToFileName(string value)
        {
            value = value.Replace(" ", "").Replace("+", "");
            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int i = 0, j = invalidChars.Length; i < j; i++)
            {
                value = value.Replace(invalidChars[i], '_');
            }

            return string.IsNullOrEmpty(value) ? "undefine" : value;
        }

        public static string ToFolderName(string value)
        {
            char[] invalidChars = Path.GetInvalidPathChars();
            for (int i = 0, j = invalidChars.Length; i < j; i++)
            {
                value = value.Replace(invalidChars[i], '_');
            }

            return value;
        }

        public static string ToIntegralString(int input, int length)
        {
            return input.ToString(string.Format("D{0}", length));
        }

        public static string ToNoWildCards(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            value = value.Replace("[", "[[]");
            WildCards.Each(wildCard => value = value.Replace(wildCard, $"[{wildCard}]"));

            return value;
        }

        public static string ToCrypticEmail(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var index = value.IndexOf("@");
            if (index < 0)
            {
                return value;
            }

            var firstPart = value.Substring(0, index);
            var lastPart = value.Substring(index, value.Length - index);

            return $"{firstPart[0]}******{lastPart}";
        }

        public static string ToMaskString(string value, int disCharLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.Length < disCharLength)
            {
                return value;
            }

            var firstPart = new string('*', value.Length - disCharLength);
            var lastPart = value.Substring(value.Length - disCharLength, disCharLength);

            return firstPart + lastPart;
        }

        public static string ToMask(string value)
        {
            return ToMask(value, unmaskCharCount: 4);
        }

        public static string ToMask(string value, int unmaskCharCount)
        {
            if (value.Length <= unmaskCharCount) return value;
            return Regex.Replace(value, ".(?=.{" + unmaskCharCount + "})", "*");
        }
    }
}