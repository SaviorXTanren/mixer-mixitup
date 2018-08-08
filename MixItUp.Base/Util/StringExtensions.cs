using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MixItUp.Base.Util
{
    public static class StringExtensions
    {
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
    }
}
