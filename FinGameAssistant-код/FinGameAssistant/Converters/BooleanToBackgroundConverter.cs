using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FinGameAssistant.Converters
{
    public class BooleanToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isUnlocked && isUnlocked)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F9F0"));
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}