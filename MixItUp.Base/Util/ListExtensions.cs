using System.Collections.Generic;

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
    }
}
