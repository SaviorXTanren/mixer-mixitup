using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MixItUp.WPF.Util
{
    [ValueConversion(typeof(string), typeof(Brush))]
    public class ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException("The target must be a Brush");
            }

            string color = (string)value;
            if (!string.IsNullOrEmpty(color))
            {
                return Application.Current.FindResource(color) as SolidColorBrush;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
