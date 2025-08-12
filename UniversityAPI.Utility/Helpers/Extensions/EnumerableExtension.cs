namespace UniversityAPI.Utility.Helpers.Extensions
{
    using System.Collections;
    using UniversityAPI.Utility.Helpers;
    using UniversityAPI.Utility.Helpers.Extensions;

    public static class EnumerableExtension
    {
        private static Random random = new();

        public static void Each<TSource>(this IEnumerable<TSource> source, Action<TSource> execution) //where TSource : new()
        {
            foreach (TSource item in source)
            {
                execution(item);
            }
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.OrderBy(model => keySelector(model)).FirstOrDefault();
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.OrderByDescending(model => keySelector(model)).FirstOrDefault();
        }

        public static TKey MaxOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, TKey defaultValue = default)
        {
            return source.Select(keySelector).Any() ? source.Max(keySelector) : defaultValue;
        }

        public static IList<TSource> DistinctListBy<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            var hashtable = new Hashtable();
            source.Each(model => HashtableHelper.TryAdd(hashtable, selector(model), model));

            return hashtable.Values.OfType<TSource>().ToList();
        }

        public static IEnumerable<TResult> Distinct<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(model => selector(model)).Distinct();
        }

        public static IEnumerable<TSource> SortByPosition<TSource>(this IEnumerable<TSource> source)
            where TSource : ISortable
        {
            var position = 1;

            foreach (TSource model in source)
            {
                model.Position = position;
                position++;
            }

            return source;
        }

        public static void AddNotNull<TSource>(this IList<TSource> source, TSource item)
        {
            if (source != null && item != null)
            {
                source.Add(item);
            }
        }

        public static IList<TSource> Shuffle<TSource>(this IList<TSource> source)
        {
            int n = source.Count();
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                TSource value = source[k];
                source[k] = source[n];
                source[n] = value;
            }

            return source;
        }

        public static IList<TSource> Shift<TSource>(this IList<TSource> source, int from, int to)
            where TSource : class, ISortable, new()
        {
            int start = from > to ? to : from + 1;
            int end = from > to ? from - 1 : to;
            int increase = from > to ? 1 : -1;

            var model = source.First(i => i.Position.Equals(from));
            var changes = source.Where(i => i.Position >= start && i.Position <= end);
            foreach (var change in changes)
            {
                change.Position += increase;
            }

            model.Position = to;

            return source;
        }

        public static IList<TSource> DistinctList<TSource>(this IEnumerable<TSource> source)
        {
            return source.Distinct().ToList();
        }

        public static IList<TResult> DistinctList<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(model => selector(model)).Distinct().ToList();
        }

        public static IList<TSource> WhereList<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.Where(model => predicate(model)).ToList();
        }

        public static IList<TResult> SelectList<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(model => selector(model)).ToList();
        }

        public static IList<TResult> SelectManyList<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            return source.SelectMany(model => selector(model)).ToList();
        }

        public static IList<TSource> OrderByList<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.OrderBy(model => keySelector(model)).ToList();
        }

        public static IList<TSource> OrderByDescendingList<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.OrderByDescending(model => keySelector(model)).ToList();
        }

        public static IList<TSource> ThenByList<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ThenBy(model => keySelector(model)).ToList();
        }

        public static IList<TSource> ThenByDescendingList<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.ThenByDescending(model => keySelector(model)).ToList();
        }

        public static IList<TSource> SortByPositionList<TSource>(this IEnumerable<TSource> source)
            where TSource : ISortable
        {
            return source.SortByPosition().ToList();
        }

        public static Hashtable ToHashtable<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector)
        {
            return HashtableHelper.ToHashtable(source, keySelector);
        }

        public static IList<TSource> ExpectList<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> target, Func<TSource, Guid> selector)
        {
            IList<Guid> keys = target.SelectList(it => selector(it));

            return source.WhereList(it => keys.Contains(selector(it)) == false);
        }

        public static IList<TSource> IntersectList<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> target, Func<TSource, Guid> selector)
        {
            IList<Guid> keys = target.SelectList(it => selector(it));

            return source.WhereList(it => keys.Contains(selector(it)));
        }

        public static bool AddOrInitialize<T>(this IList<T> list, T item)
        {
            if (list == null)
            {
                return false;
            }

            list.Add(item);
            return true;
        }
    }

    public interface ISortable
    {
        int Position { get; set; }
    }
}