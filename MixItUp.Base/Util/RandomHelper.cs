using System;

namespace MixItUp.Base.Util
{
    public static class RandomHelper
    {
        public static int GenerateProbability() { return RandomHelper.GenerateRandomNumber(100) + 1; }

        public static int GenerateRandomNumber(int maxValue) { return RandomHelper.GenerateRandomNumber(0, maxValue); }

        public static int GenerateRandomNumber(int minValue, int maxValue)
        {
            Guid guid = Guid.NewGuid();
            int number = Math.Abs(guid.GetHashCode());
            return (number % (maxValue - minValue)) + minValue;
        }
    }
}
