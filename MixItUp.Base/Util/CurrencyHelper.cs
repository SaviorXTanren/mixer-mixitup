using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MixItUp.Base.Util
{
    public static class CurrencyHelper
    {
        private static readonly Dictionary<string, CultureInfo> SymbolsByCode = new Dictionary<string, CultureInfo>();

        public static string ToCurrencyString(double amount) { return CurrencyHelper.ToCurrencyString(null, amount); }

        public static string ToCurrencyString(string code, double amount)
        {
            amount = Math.Round(amount, 2);
            if (!string.IsNullOrEmpty(code))
            {
                if (!CurrencyHelper.SymbolsByCode.TryGetValue(code, out CultureInfo culture))
                {
                    culture = CultureInfo.GetCultures(CultureTypes.SpecificCultures).FirstOrDefault(c => string.Equals(c.Name, code, StringComparison.OrdinalIgnoreCase));
                }

                if (culture != null)
                {
                    CurrencyHelper.SymbolsByCode[code] = culture;
                    return amount.ToString("C2", culture);
                }
            }
            return amount.ToString("C2");
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
