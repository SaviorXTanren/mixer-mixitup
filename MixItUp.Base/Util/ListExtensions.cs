using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.Util
{
    public static class ListExtensions
    {
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
            foreach (var i in list.ToList())
            {
                l.Add(random.Next(), i);
            }
            return l.Values;
        }

        public static T Random<T>(this IEnumerable<T> list)
        {
            int index = RandomHelper.GenerateRandomNumber(0, list.Count());
            return list.ElementAt(index);
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
    }
}
