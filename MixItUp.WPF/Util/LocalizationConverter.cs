using MixItUp.Base.Localization;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MixItUp.WPF.Util
{
    public class LocalizationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = parameter as string;
            if (!string.IsNullOrEmpty(key))
            {
                return LocalizationHandler.GetLocalizationString(key);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
