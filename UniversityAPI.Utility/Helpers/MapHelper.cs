namespace UniversityAPI.Utility.Helpers
{
    using System.Collections.Concurrent;
    using System.Data;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class MapHelper
    {
        private static readonly ConcurrentDictionary<(Type From, Type To), Delegate> mapCache = new();

        public static TTo Map<TFrom, TTo>(TFrom from)
        {
            if (from is null)
            {
                return default!;
            }

            var mapper = (Func<TFrom, TTo>)mapCache.GetOrAdd((typeof(TFrom), typeof(TTo)), _ => CreateMapper<TFrom, TTo>());
            return mapper(from);
        }

        public static List<TTo> MapList<TFrom, TTo>(IEnumerable<TFrom> source)
        {
            if (source is null)
            {
                return [];
            }

            var mapper = (Func<TFrom, TTo>)mapCache.GetOrAdd((typeof(TFrom), typeof(TTo)), _ => CreateMapper<TFrom, TTo>());
            var list = new List<TTo>();
            foreach (var item in source)
            {
                list.Add(mapper(item));
            }

            return list;
        }

        public static TTo MapFromDataRow<TTo>(DataRow row)
        {
            if (row is null)
            {
                return default!;
            }

            var mapper = (Func<DataRow, TTo>)mapCache.GetOrAdd((typeof(DataRow), typeof(TTo)), _ => CreateDataRowMapper<TTo>(row.Table));
            return mapper(row);
        }

        public static List<TTo> MapFromDataTable<TTo>(DataTable table)
        {
            if (table?.Rows.Count == 0)
            {
                return [];
            }

            var mapper = (Func<DataRow, TTo>)mapCache.GetOrAdd((typeof(DataRow), typeof(TTo)), _ => CreateDataRowMapper<TTo>(table));
            var list = new List<TTo>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                list.Add(mapper(row));
            }

            return list;
        }

        private static Func<TFrom, TTo> CreateMapper<TFrom, TTo>()
        {
            var targetType = typeof(TTo);
            var sourceType = typeof(TFrom);
            var sourceParam = Expression.Parameter(sourceType, "src");

            var ctor = targetType.GetConstructors()
                                 .OrderByDescending(c => c.GetParameters().Length)
                                 .FirstOrDefault();

            if (ctor is not null && ctor.GetParameters().Length > 0)
            {
                var args = ctor.GetParameters()
                               .Select(p =>
                               {
                                   var srcProp = sourceType.GetProperty(p.Name!, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                                   if (srcProp is null || !srcProp.CanRead)
                                   {
                                       return (Expression)Expression.Default(p.ParameterType);
                                   }
                                   var propAccess = Expression.Property(sourceParam, srcProp);
                                   return Expression.Convert(propAccess, p.ParameterType);
                               })
                               .ToArray();

                var newExpr = Expression.New(ctor, args);
                return Expression.Lambda<Func<TFrom, TTo>>(newExpr, sourceParam).Compile();
            }
            else
            {
                var bindings = new List<MemberBinding>();

                foreach (var targetProp in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                     .Where(p => p.CanWrite))
                {
                    var sourceProp = sourceType.GetProperty(targetProp.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (sourceProp is null || !sourceProp.CanRead)
                    {
                        continue;
                    }

                    var propAccess = Expression.Property(sourceParam, sourceProp);
                    bindings.Add(Expression.Bind(targetProp, Expression.Convert(propAccess, targetProp.PropertyType)));
                }

                var body = Expression.MemberInit(Expression.New(targetType), bindings);
                return Expression.Lambda<Func<TFrom, TTo>>(body, sourceParam).Compile();
            }
        }

        private static Func<DataRow, TTo> CreateDataRowMapper<TTo>(DataTable table)
        {
            var targetType = typeof(TTo);
            var rowParam = Expression.Parameter(typeof(DataRow), "row");

            var fieldMethod = typeof(DataRowExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "Field" && m.IsGenericMethod && m.GetParameters().Length == 2);

            var ctor = targetType.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (ctor is not null && ctor.GetParameters().Length > 0)
            {
                var args = ctor.GetParameters()
                               .Select(p =>
                               {
                                   if (!table.Columns.Contains(p.Name!))
                                   {
                                       throw new InvalidOperationException($"No matching column for '{p.Name}'");
                                   }
                                   var colExpr = Expression.Constant(p.Name);
                                   var genericField = fieldMethod.MakeGenericMethod(p.ParameterType);
                                   return Expression.Call(genericField, rowParam, colExpr);
                               })
                               .ToArray();

                var newExpr = Expression.New(ctor, args);
                return Expression.Lambda<Func<DataRow, TTo>>(newExpr, rowParam).Compile();
            }
            else
            {
                var bindings = new List<MemberBinding>();

                foreach (var targetProp in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                     .Where(p => p.CanWrite))
                {
                    if (!table.Columns.Contains(targetProp.Name)) continue;

                    var colExpr = Expression.Constant(targetProp.Name);
                    var genericField = fieldMethod.MakeGenericMethod(targetProp.PropertyType);
                    var valueExpr = Expression.Call(genericField, rowParam, colExpr);
                    bindings.Add(Expression.Bind(targetProp, valueExpr));
                }

                var body = Expression.MemberInit(Expression.New(targetType), bindings);
                return Expression.Lambda<Func<DataRow, TTo>>(body, rowParam).Compile();
            }
        }
    }
}