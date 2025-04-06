namespace System.Collections
{
    public class HashtableHelper
    {
        public static void TryAdd<TKey>(Hashtable hashtable, TKey key, object value)
        {
            if (key != null)
            {
                TryAdd(hashtable, key.ToString(), value);
            }
        }

        public static void TryAdd(Hashtable hashtable, Guid key, object value)
        {
            TryAdd(hashtable, key.ToString(), value);
        }

        public static void TryAdd(Hashtable hashtable, string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (hashtable.ContainsKey(key))
            {
                return;
            }

            hashtable.Add(key, value);
        }

        public static T GetValue<T>(Hashtable hashtable, Guid key) where T : class
        {
            return GetValue<T>(hashtable, key.ToString());
        }

        public static T GetValue<T>(Hashtable hashtable, string key) where T : class
        {
            if (string.IsNullOrEmpty(key) || hashtable.ContainsKey(key) == false)
            {
                return default(T);
            }

            return hashtable[key] as T;
        }

        public static Hashtable ToHashtable<T>(IEnumerable<T> items, Func<T, Guid> keySelector)
        {
            return ToHashtable<T>(items, it => keySelector(it).ToString());
        }

        public static Hashtable ToHashtable<T>(IEnumerable<T> items, Func<T, string> keySelector)
        {
            var hashtable = new Hashtable();
            items.Each(item => TryAdd(hashtable, keySelector(item), item));

            return hashtable;
        }

        public static bool ContainsKey(Hashtable table, string key)
        {
            return string.IsNullOrEmpty(key) == false && table.ContainsKey(key);
        }

        public static Hashtable GetHashtable<TModel>(IEnumerable<TModel> items, Func<TModel, string> func)
        {
            Hashtable hashtable = new Hashtable();
            items.Each(item => TryAdd(hashtable, func(item), item));

            return hashtable;
        }

        public static HashItem<T> GetHashItem<T>(Hashtable hashtable, int key)
        {
            return GetHashItem<T>(hashtable, key.ToString());
        }

        public static HashItem<T> GetHashItem<T>(Hashtable hashtable, Guid key)
        {
            return GetHashItem<T>(hashtable, key.ToString());
        }

        public static HashItem<T> GetHashItem<T>(Hashtable hashtable, string key)
        {
            if (hashtable == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(key) || hashtable.ContainsKey(key) == false)
            {
                return null;
            }

            return hashtable[key] as HashItem<T>;
        }

        public static Hashtable ToHashItems<T>(IEnumerable<T> items, Func<T, Guid> keySelector)
        {
            return ToHashItems(items, (item) => keySelector(item).ToString());
        }

        public static Hashtable ToHashItems<T>(IEnumerable<T> items, Func<T, string> keySelector)
        {
            var hashtable = new Hashtable();
            items.Each(item => AddHashItem(hashtable, keySelector(item), item));

            return hashtable;
        }

        public static void AddHashItem<T>(Hashtable hashtable, int key, T item)
        {
            AddHashItem<T>(hashtable, key.ToString(), item);
        }

        public static void AddHashItems<T>(Hashtable hashtable, int key, IEnumerable<T> items)
        {
            AddHashItems<T>(hashtable, key.ToString(), items);
        }

        public static void AddHashItem<T>(Hashtable hashtable, string key, T item)
        {
            if (ContainsKey(hashtable, key) == false)
            {
                TryAdd(hashtable, key, new HashItem<T>(key, item));
            }
            else
            {
                HashItem<T> hashtableItem = GetValue<HashItem<T>>(hashtable, key);
                hashtableItem.Add(item);
            }
        }

        public static void AddHashItems<T>(Hashtable hashtable, string key, IEnumerable<T> items)
        {
            if (ContainsKey(hashtable, key) == false)
            {
                TryAdd(hashtable, key, new HashItem<T>(key, items));
            }
            else
            {
                HashItem<T> hashtableItem = GetValue<HashItem<T>>(hashtable, key);
                items.Each(item => hashtableItem.Add(item));
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HashItem<T>
    {
        public string Key { get; private set; }

        public int Count => this.Items.Count;

        /// <summary>
        ///
        /// </summary>
        public bool IsList { get; private set; }

        public bool HasValue => ValueHelper.IsNullOrEmpty(this.Items) == false;

        /// <summary>
        ///
        /// </summary>
        public T First { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public List<T> Items { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="item"></param>
        public HashItem(string key, T item)
        {
            this.Key = key;
            this.Add(item);
        }

        public HashItem(string key, IEnumerable<T> items)
        {
            this.Key = key;
            items.Each(item => this.Add(item));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (this.Items == null)
            {
                this.IsList = false;
                this.First = item;
                this.Items = new List<T>() { item };
            }
            else
            {
                this.IsList = true;
                this.Items.Add(item);
            }
        }
    }
}