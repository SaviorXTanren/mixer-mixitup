using System;
using System.Windows;
using System.Windows.Data;

namespace MixItUp.WPF.Util
{
    [ValueConversion(typeof(bool), typeof(TextDecorationCollection))]
    public class StrikethroughConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(TextDecorationCollection))
            {
                throw new InvalidOperationException("The target must be a TextDecorationCollection");
            }

            bool booleanValue = (bool)value;
            return booleanValue ? TextDecorations.Strikethrough : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
