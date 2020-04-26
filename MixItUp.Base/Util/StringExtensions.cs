using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MixItUp.Base.Util
{
    public static class StringExtensions
    {
        private const char Comma = ',';
        private const char Decimal = '.';

        public static string ToFilePathString(this string source)
        {
            string directory = null;
            string filename = source;

            int lastSlash = source.LastIndexOf('\\');
            if (lastSlash > -1 && lastSlash < source.Length)
            {
                directory = source.Substring(0, lastSlash);
                filename = source.Substring(lastSlash + 1);
            }

            if (filename != null)
            {
                char[] invalidChars = Path.GetInvalidFileNameChars();
                filename = new string(filename.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            }

            if (directory != null)
            {
                char[] invalidChars = Path.GetInvalidPathChars();
                directory = new string(directory.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
                return Path.Combine(directory, filename);
            }

            return filename;
        }

        public static string Shuffle(this string str)
        {
            Random random = new Random();
            var list = new SortedList<int, char>();
            foreach (var c in str)
            {
                list.Add(random.Next(), c);
            }
            return new string(list.Values.ToArray());
        }

        public static string AddNewLineEveryXCharacters(this string str, int lineLength)
        {
            string newString = string.Empty;
            int x = 0;
            for (int i = 0; i < str.Length; i++)
            {
                x++;
                if (x >= lineLength && str[i] == ' ')
                {
                    newString += Environment.NewLine;
                    x = 0;
                }
                else
                {
                    newString += str[i];
                }
            }
            return newString;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static bool ParseCurrency(this string str, out double result)
        {
            // First try the current culture and then the invariant culture if that fails.
            if (!double.TryParse(str, NumberStyles.Currency, NumberFormatInfo.CurrentInfo, out result))
            {
                return double.TryParse(str, NumberStyles.Currency, NumberFormatInfo.InvariantInfo, out result);
            }
            return true;
        }
    }
}
