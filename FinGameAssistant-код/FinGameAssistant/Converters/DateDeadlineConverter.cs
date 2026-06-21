using System;
using System.Globalization;
using System.Windows.Data;

namespace FinGameAssistant.Converters
{
    public class DateDeadlineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                var daysLeft = (date - DateTime.Now).TotalDays;

                if (daysLeft < 0)
                    return $"Просрочено";
                else if (daysLeft < 1)
                    return "Сегодня";
                else if (daysLeft < 7)
                    return $"Осталось {(int)daysLeft} дн.";
                else
                    return date.ToString("dd.MM.yyyy");
            }

            return "Без срока";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}