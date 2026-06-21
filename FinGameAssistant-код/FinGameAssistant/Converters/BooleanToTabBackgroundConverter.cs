using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FinGameAssistant.Converters
{
    public class BooleanToTabBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive && isActive)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F0F0"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}