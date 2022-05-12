using Jace;
using MixItUp.Base.Model.Settings;
using StreamingClient.Base.Util;
using System;

namespace MixItUp.Base.Util
{
    public static class MathHelper
    {
        public static int Clamp(int number, int min, int max)
        {
            return Math.Min(Math.Max(number, min), max);
        }

        public static double ProcessMathEquation(string equation)
        {
            double result = 0;
            try
            {
                equation = equation.Replace("random(", "customrandom(");

                // Process Math
                CalculationEngine engine = new CalculationEngine(Languages.GetLanguageLocaleCultureInfo());
                engine.AddFunction("customrandom", Random);
                engine.AddFunction("randomrange", RandomRange);

                // If they used +1, then trim it off
                equation = equation.TrimStart(' ', '+');

                result = engine.Calculate(equation);
            }
            catch (Exception ex)
            {
                // Calculation failed, log and set to 0
                Logger.Log(ex);
            }
            return result;
        }

        private static double Random(double max)
        {
            return RandomHelper.GenerateRandomNumber(1, (int)max);
        }

        private static double RandomRange(double min, double max)
        {
            return RandomHelper.GenerateRandomNumber((int)min, (int)max);
        }
    }
}
