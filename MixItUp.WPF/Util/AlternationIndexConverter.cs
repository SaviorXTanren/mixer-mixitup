using System;
using System.Windows.Data;

namespace MixItUp.WPF.Util
{
    [ValueConversion(typeof(int), typeof(int))]
    public class AlternationIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("The target must be a string");
            }
            return (int.Parse(value.ToString()) + 1).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
