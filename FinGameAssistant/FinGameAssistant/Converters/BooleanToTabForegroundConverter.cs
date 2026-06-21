using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FinGameAssistant.Converters
{
    public class BooleanToTabForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive && isActive)
            {
                return new SolidColorBrush(Colors.White);
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}