using MixItUp.Base.Util;
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
                object resource = Application.Current.TryFindResource(color);
                if (resource == null)
                {
                    try
                    {
                        return (SolidColorBrush)(new BrushConverter().ConvertFrom(color));
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                }
                else
                {
                    return resource;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
