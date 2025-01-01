using MixItUp.Base.Util;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MixItUp.WPF.Util
{
    public class LocalizationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string key = null;
            if (value is Enum)
            {
                key = GetEnumName(value);
            }
            else if (value is string)
            {
                key = value as string;
            }
            else
            {
                key = value.ToString();
            }

            if (!string.IsNullOrEmpty(key))
            {
                return MixItUp.Base.Resources.ResourceManager.GetSafeString(key);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetEnumName(object value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (!string.IsNullOrEmpty(name))
            {
                NameAttribute[] nameAttributes = (NameAttribute[])type.GetField(name).GetCustomAttributes(typeof(NameAttribute), false);
                if (nameAttributes != null && nameAttributes.Length > 0)
                {
                    return nameAttributes[0].Name;
                }
                return name;
            }

            return null;
        }
    }
}
