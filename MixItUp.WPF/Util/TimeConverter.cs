using System;
using System.Globalization;
using System.Windows.Data;
using MixItUp.Base.Util;

namespace MixItUp.WPF.Util
{
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTimeOffset)
            {
                return ((DateTimeOffset)value).ToFriendlyTimeString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
