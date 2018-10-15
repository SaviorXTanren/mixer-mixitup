using System;

namespace MixItUp.Base.Util
{
    public static class RandomHelper
    {
        private static int randomSeed = (int)DateTime.Now.Ticks;

        public static int GenerateProbability() { return RandomHelper.GenerateRandomNumber(100) + 1; }

        public static int GenerateRandomNumber(int maxValue) { return RandomHelper.GenerateRandomNumber(0, maxValue); }

        public static int GenerateRandomNumber(int minValue, int maxValue)
        {
            Random random = new Random(RandomHelper.randomSeed);
            RandomHelper.randomSeed -= random.Next(100);
            random = new Random(RandomHelper.randomSeed);
            return random.Next(minValue, maxValue);
        }
    }
}
