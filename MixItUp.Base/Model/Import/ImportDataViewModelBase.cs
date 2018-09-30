using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Import
{
    public abstract class ImportDataViewModelBase
    {
        public string GetRegexEntries(string text, string pattern, Func<string, string> replacement)
        {
            List<string> entries = new List<string>();

            int startIndex = 0;
            do
            {
                startIndex = text.IndexOf(pattern);
                if (startIndex >= 0)
                {
                    int endIndex = text.IndexOf(")", startIndex);
                    if (endIndex >= 0)
                    {
                        string fullEntry = text.Substring(startIndex, endIndex - startIndex + 1);
                        string entry = fullEntry.Replace(pattern, "").Replace(")", "");
                        entries.Add(entry);
                        text = text.Replace(fullEntry, replacement(entry));
                    }
                }
            } while (startIndex >= 0);

            return text;
        }
    }
}
