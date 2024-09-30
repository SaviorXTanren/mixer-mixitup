using System;

namespace MixItUp.Base.Util
{
    public static class RandomHelper
    {
        private static readonly char[] FilePathDelimitedSplit = new char[] { '|' };

        public static int GenerateProbability() { return RandomHelper.GenerateRandomNumber(100) + 1; }

        public static double GenerateDecimalProbability() { return ((double)RandomHelper.GenerateRandomNumber(0, 1000000)) / 1000000.0; }

        public static int GenerateRandomNumber(int maxValue) { return RandomHelper.GenerateRandomNumber(0, maxValue); }

        public static int GenerateRandomNumber(int minValue, int maxValue)
        {
            Guid guid = Guid.NewGuid();
            int number = Math.Abs(guid.GetHashCode());
            return (number % Math.Max(maxValue - minValue, 1)) + minValue;
        }

        public static string PickRandomFileFromDelimitedString(string filepaths)
        {
            if (!string.IsNullOrWhiteSpace(filepaths) && filepaths.Contains("|"))
            {
                string[] splits = filepaths.Split(FilePathDelimitedSplit, StringSplitOptions.RemoveEmptyEntries);
                if (splits != null && splits.Length > 0)
                {
                    return splits.Random();
                }
            }
            return filepaths;
        }
    }
}
