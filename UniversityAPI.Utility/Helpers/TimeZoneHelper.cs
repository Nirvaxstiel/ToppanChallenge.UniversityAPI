namespace UniversityAPI.Utility.Helpers
{
    using System.Collections.Concurrent;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Reflection;
    using System.Reflection.Emit;
    using UniversityAPI.Utility.Helpers.Extensions;
    using UniversityAPI.Utility.Interfaces;

    public static class TimeZoneHelper
    {
        private static string DEFAULTSTANDARDTIME;
        private static IConfigHelper? configHelper;

        public static void SetInstance(IConfigHelper config)
        {
            configHelper = config;
            DEFAULTSTANDARDTIME = GetDefaultTimeZoneId();
        }

        public static TimeZoneInfo StandardTimeZone => TimeZoneInfo.FindSystemTimeZoneById(DEFAULTSTANDARDTIME);

        public static string GetDefaultTimeZoneId()
        {
            string defaultTimeZoneId = configHelper.GetValue<string>("default_standard_time");

            return string.IsNullOrWhiteSpace(defaultTimeZoneId) ? "Singapore Standard Time" : defaultTimeZoneId;
        }

        public static void SetTimeToUtc8<T>(T item, TimeZoneInfo sourceTimeZone, string[] ignores = null)
        {
            if (item == null || !IsConvertableType(typeof(T)))
            {
                return;
            }

            var builder = UtcBuilder.GetTimeToUtc8Action<T>(ignores);
            if (builder != null)
            {
                builder(item, sourceTimeZone);
            }
        }

        public static void SetTimeFromUtc8<T>(T item, TimeZoneInfo destinationTimeZone)
        {
            if (item == null || !IsConvertableType(typeof(T)))
            {
                return;
            }

            var builder = UtcBuilder.GetTimeFromUtc8Builder<T>();
            if (builder != null)
            {
                builder(item, destinationTimeZone);
            }
        }

        public static void SetTimeToUtc8(IList<DateTime> items, TimeZoneInfo sourceTimeZone)
        {
            if (items.Count == 0)
            {
                return;
            }

            for (var i = 0; i < items.Count; i++)
            {
                items[i] = ToUtc8(items[i], sourceTimeZone);
            }
        }

        public static void SetTimeToUtc8<T>(IEnumerable<T> items, TimeZoneInfo sourceTimeZone, string[] ignores = null)
        {
            if (items.Count() == 0)
            {
                return;
            }

            var action = UtcBuilder.GetTimeToUtc8Action<T>(ignores);
            if (action != null)
            {
                items.Each(item => action(item, sourceTimeZone));
            }
        }

        public static void SetTimeFromUtc8(IList<DateTime> items, TimeZoneInfo destinationTimeZone)
        {
            if (items.Count == 0)
            {
                return;
            }

            for (var i = 0; i < items.Count; i++)
            {
                items[i] = FromUtc8(items[i], destinationTimeZone);
            }
        }

        public static void SetTimeFromUtc8<T>(IEnumerable<T> items, TimeZoneInfo destinationTimeZone)
        {
            if (items.Count() == 0)
            {
                return;
            }

            var action = UtcBuilder.GetTimeFromUtc8Builder<T>();
            if (action != null)
            {
                items.Each(item => action(item, destinationTimeZone));
            }
        }

        public static void SetTimeFromUtc8(DataSet dataSet, TimeZoneInfo destinationTimeZone)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                SetTimeFromUtc8(table, destinationTimeZone);
            }
        }

        public static void SetTimeFromUtc8(DataTable table, TimeZoneInfo destinationTimeZone)
        {
            var columns = table.Columns.Cast<DataColumn>().Where(column => column.DataType == typeof(DateTime));

            columns.Each(column => column.ReadOnly = false);
            foreach (DataRow row in table.Rows)
            {
                columns.Each(column => SetTimeFromUtc8(row, column, destinationTimeZone));
            }

            table.AcceptChanges();
        }

        //
        public static DateTime ToUtc8(DateTime dateTime, TimeZoneInfo sourceTimeZone)
        {
            if (IsConvertableDateTime(dateTime, sourceTimeZone, StandardTimeZone))
            {
                return TimeZoneInfo.ConvertTime(dateTime, sourceTimeZone, StandardTimeZone);
            }

            return dateTime;
        }

        //
        public static DateTime? ToNullableUtc8(DateTime? dateTime, TimeZoneInfo sourceTimeZone)
        {
            if (!dateTime.HasValue)
            {
                return null;
            }

            if (IsConvertableDateTime(dateTime.Value, sourceTimeZone, StandardTimeZone))
            {
                return TimeZoneInfo.ConvertTime(dateTime.Value, sourceTimeZone, StandardTimeZone);
            }

            return dateTime;
        }

        //
        public static DateTime FromUtc8(DateTime dateTime, TimeZoneInfo destinationTimeZone)
        {
            if (IsConvertableDateTime(dateTime, StandardTimeZone, destinationTimeZone))
            {
                return TimeZoneInfo.ConvertTime(dateTime, StandardTimeZone, destinationTimeZone);
            }

            return dateTime;
        }

        //
        public static DateTime? FromNullableUtc8(DateTime? dateTime, TimeZoneInfo destinationTimeZone)
        {
            if (!dateTime.HasValue)
            {
                return null;
            }

            if (IsConvertableDateTime(dateTime.Value, StandardTimeZone, destinationTimeZone))
            {
                return TimeZoneInfo.ConvertTime(dateTime.Value, StandardTimeZone, destinationTimeZone);
            }

            return dateTime;
        }

        private static void SetTimeFromUtc8(DataRow row, DataColumn column, TimeZoneInfo destinationTimeZone)
        {
            var dateTime = ConvertHelper.ToNullableDate(row[column]);
            if (IsConvertableValue(dateTime))
            {
                row[column] = TimeZoneInfo.ConvertTime(dateTime.Value, StandardTimeZone, destinationTimeZone);
            }
        }

        private static bool IsConvertableType(Type type)
        {
            if (type == typeof(char) || type == typeof(string))
            {
                return false;
            }
            if (type == typeof(short) || type == typeof(int) || type == typeof(long))
            {
                return false;
            }
            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                return false;
            }
            if (type == typeof(bool))
            {
                return false;
            }

            return true;
        }

        private static bool IsConvertableDateTime(DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
        {
            if (sourceTimeZone.Id == destinationTimeZone.Id)
            {
                return false;
            }
            if (dateTime.Date <= default(DateTime).Date)
            {
                return false;
            }
            if (dateTime.Date >= DateTime.MaxValue.Date)
            {
                return false;
            }
            if (dateTime.Date <= SqlDateTime.MinValue.Value.Date)
            {
                return false;
            }
            if (dateTime.Date >= SqlDateTime.MaxValue.Value.Date)
            {
                return false;
            }

            return true;
        }

        private static bool IsConvertableValue(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return false;
            }
            if (dateTime.Value.Date <= default(DateTime).Date)
            {
                return false;
            }
            if (dateTime.Value.Date >= DateTime.MaxValue.Date)
            {
                return false;
            }
            if (dateTime.Value.Date.Equals(SqlDateTime.MinValue.Value.Date))
            {
                return false;
            }
            if (dateTime.Value.Date.Equals(SqlDateTime.MaxValue.Value.Date))
            {
                return false;
            }

            return true;
        }
    }

    public sealed class UtcBuilder
    {
        private static readonly MethodInfo toUtc8Method = typeof(TimeZoneHelper).GetMethod("ToUtc8");
        private static readonly MethodInfo toNullableUtc8Method = typeof(TimeZoneHelper).GetMethod("ToNullableUtc8");

        private static readonly MethodInfo fromUtc8Method = typeof(TimeZoneHelper).GetMethod("FromUtc8");
        private static readonly MethodInfo fromNullableUtc8Method = typeof(TimeZoneHelper).GetMethod("FromNullableUtc8");

        private static ConcurrentDictionary<string, Delegate> timeToUtc8Cache = new();
        private static ConcurrentDictionary<string, Delegate> timeFromUtc8Cache = new();

        public static Action<T, TimeZoneInfo> GetTimeToUtc8Action<T>(string[] ignores)
        {
            var key = $"{typeof(T)}.TimeToUtc8";
            if (!timeToUtc8Cache.ContainsKey(key))
            {
                timeToUtc8Cache[key] = CreateTimeToUtc8Action<T>(ignores);
            }

            return timeToUtc8Cache[key] as Action<T, TimeZoneInfo>;
        }

        public static Action<T, TimeZoneInfo> GetTimeFromUtc8Builder<T>()
        {
            var key = $"{typeof(T)}.TimeFromUtc8";
            if (!timeFromUtc8Cache.ContainsKey(key))
            {
                timeFromUtc8Cache[key] = CreatedTimeFromUtc8Builder<T>();
            }

            return timeFromUtc8Cache[key] as Action<T, TimeZoneInfo>;
        }

        private static Action<T, TimeZoneInfo> CreateTimeToUtc8Action<T>(string[] ignores)
        {
            var method = new DynamicMethod("TimeToUtc8Action", null, [typeof(T), typeof(TimeZoneInfo)]);
            var il = method.GetILGenerator();

            var properties = GetSupportProperies(typeof(T), ignores);
            foreach (var property in properties)
            {
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Callvirt, property.GetMethod);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, GetToUtc8Method(property));
                il.Emit(OpCodes.Callvirt, property.SetMethod);
                il.Emit(OpCodes.Nop);
            }

            il.Emit(OpCodes.Ret);

            return (Action<T, TimeZoneInfo>)method.CreateDelegate(typeof(Action<T, TimeZoneInfo>));
        }

        private static Action<T, TimeZoneInfo> CreatedTimeFromUtc8Builder<T>()
        {
            var method = new DynamicMethod("TimeFromUtc8Action", null, [typeof(T), typeof(TimeZoneInfo)]);
            var il = method.GetILGenerator();

            var properties = GetSupportProperies(typeof(T), null);
            foreach (var property in properties)
            {
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Callvirt, property.GetMethod);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, GetFromUtc8Method(property));
                il.Emit(OpCodes.Callvirt, property.SetMethod);
                il.Emit(OpCodes.Nop);
            }

            il.Emit(OpCodes.Ret);

            return (Action<T, TimeZoneInfo>)method.CreateDelegate(typeof(Action<T, TimeZoneInfo>));
        }

        private static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static bool IsSupportProperty(PropertyInfo property)
        {
            var canReadAndWrite = property.CanRead && property.CanWrite;
            var isPropery = property.MemberType == MemberTypes.Property;

            return isPropery & canReadAndWrite && GetType(property.PropertyType) == typeof(DateTime) && !IsIgnored(property);
        }

        private static Type GetType(Type type)
        {
            return IsNullable(type) ? Nullable.GetUnderlyingType(type) : type;
        }

        private static MethodInfo GetToUtc8Method(PropertyInfo property)
        {
            return IsNullable(property.PropertyType) ? toNullableUtc8Method : toUtc8Method;
        }

        private static MethodInfo GetFromUtc8Method(PropertyInfo property)
        {
            return IsNullable(property.PropertyType) ? fromNullableUtc8Method : fromUtc8Method;
        }

        private static IEnumerable<PropertyInfo> GetSupportProperies(Type type, string[] ignores)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public;
            var properties = type.GetProperties(flags).Where(property => IsSupportProperty(property));

            return ignores != null && ignores.Length > 0 ? properties.Where(property => !StringHelper.Contains(ignores, property.Name)) : properties;
        }

        private static bool IsIgnored(PropertyInfo property)
        {
            var attributes = property.GetCustomAttributes();
            var attribute = attributes.FirstOrDefault(model => model is TimeZoneColumnAttribute);

            return attribute == null ? false : ((TimeZoneColumnAttribute)attribute).IsIgnored;
        }
    }

    public class TimeZoneColumnAttribute(bool isIgnored) : Attribute
    {
        public bool IsIgnored { get; set; } = isIgnored;
    }
}