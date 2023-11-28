using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MixItUp.Base.Util
{
    public static class ListExtensions
    {
        public static bool TryGetValue<T>(this IList<T> list, int index, out T value)
        {
            value = default(T);
            if (list != null && list.Count > index)
            {
                value = list[index];
                return true;
            }
            return false;
        }

        public static void MoveUp<T>(this IList<T> list, T item)
        {
            int index = list.IndexOf(item) - 1;
            if (index >= 0)
            {
                list.Remove(item);
                list.Insert(index, item);
            }
        }

        public static void MoveDown<T>(this IList<T> list, T item)
        {
            int index = list.IndexOf(item) + 1;
            if (index < list.Count)
            {
                list.Remove(item);
                list.Insert(index, item);
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list)
        {
            Random random = new Random();
            var l = new SortedList<int, T>();
            foreach (var i in list.ToArray())
            {
                var key = random.Next();
                while (l.ContainsKey(key))
                {
                    key = random.Next();
                }
                l.Add(key, i);
            }
            return l.Values;
        }

        public static T RemoveFirst<T>(this IList<T> list)
        {
            T result = list.FirstOrDefault();
            list.Remove(result);
            return result;
        }

        public static T Random<T>(this IEnumerable<T> list)
        {
            if (list.Count() > 0)
            {
                return list.ElementAt(RandomHelper.GenerateRandomNumber(0, list.Count()));
            }
            return default(T);
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> list, int size)
        {
            List<T> batch = new List<T>(size);
            foreach (var item in list)
            {
                batch.Add(item);
                if (batch.Count == size)
                {
                    yield return batch;
                    batch = new List<T>(size);
                }
            }
            
            if (batch.Count > 0)
            {
                yield return batch;
            }
        }

        public static T Top<T>(this IEnumerable<T> list, Func<T, int> selector)
        {
            T result = default(T);
            int top = int.MinValue;
            foreach (T t in list)
            {
                int tValue = selector(t);
                if (tValue > top)
                {
                    result = t;
                    top = tValue;
                }
            }
            return result;
        }

        public static T Bottom<T>(this IEnumerable<T> list, Func<T, int> selector)
        {
            T result = default(T);
            int bottom = int.MaxValue;
            foreach (T t in list)
            {
                int tValue = selector(t);
                if (tValue < bottom)
                {
                    result = t;
                    bottom = tValue;
                }
            }
            return result;
        }

        public static void AddRange<T>(this HashSet<T> list, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                list.Add(item);
            }
        }

        public static void RemoveRange<T>(this List<T> list, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                list.Remove(item);
            }
        }

        public static void AddRange<T>(this Collection<T> list, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                list.Add(item);
            }
        }

        public static void ClearAndAddRange<T>(this Collection<T> list, IEnumerable<T> items)
        {
            list.Clear();
            foreach (T item in items)
            {
                list.Add(item);
            }
        }

        public static void SortedInsert<T>(this Collection<T> list, T newItem) where T : IComparable<T>
        {
            DispatcherHelper.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].CompareTo(newItem) >= 0)
                    {
                        list.Insert(i, newItem);
                        return;
                    }
                }
                list.Add(newItem);
            });
        }
    }
}
