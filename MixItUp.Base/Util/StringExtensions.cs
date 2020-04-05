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
            List<char> characters = new List<char>();

            // Remove all non-essential characters for parsing
            foreach (char c in str)
            {
                if (c == Comma)
                {
                    characters.Add(c);
                }
                else if (c == Decimal)
                {
                    characters.Add(c);
                }
                else if (char.IsDigit(c))
                {
                    characters.Add(c);
                }
            }

            if (characters.Contains(Decimal) && characters.Contains(Comma))
            {
                // Decimal appears after comma (EX: US Dollar)
                if (characters.IndexOf(Decimal) > characters.IndexOf(Comma))
                {
                    characters.RemoveAll(c => c == Comma);
                }
                // Decimal appears before comma (EX: Brazilian Real)
                else
                {
                    characters.RemoveAll(c => c == Decimal);
                    // Replace comma with decimal for invariant standardization
                    characters[characters.IndexOf(Comma)] = Decimal;
                }
            }
            else if (characters.Contains(Decimal) || characters.Contains(Comma))
            {
                int charIndex = -1;
                if (characters.Contains(Decimal))
                {
                    charIndex = characters.IndexOf(Decimal);
                }
                else if (characters.Contains(Comma))
                {
                    charIndex = characters.IndexOf(Comma);
                    // Replace comma with decimal for invariant standardization
                    characters[charIndex] = Decimal;
                }

                // Check if there are more than 2 numbers after the special denoting character.
                // If there are, then it's not a decimal cents character and can be removed.
                if (characters.Count - 1 - charIndex > 2)
                {
                    characters.RemoveAt(charIndex);
                }
            }

            return double.TryParse(new string(characters.ToArray()), NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out result);
        }
    }
}
