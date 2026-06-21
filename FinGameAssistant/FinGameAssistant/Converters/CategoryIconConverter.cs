using System;
using System.Globalization;
using System.Windows.Data;

namespace FinGameAssistant.Converters
{
    public class CategoryIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string category = value as string ?? "";

            return category.ToLower() switch
            {
                string s when s.Contains("зарплата") => "💼",
                string s when s.Contains("фриланс") => "💻",
                string s when s.Contains("подарки") => "🎁",
                string s when s.Contains("инвестиции") => "📈",
                string s when s.Contains("продукты") => "🛒",
                string s when s.Contains("ресторан") => "🍽️",
                string s when s.Contains("кофе") => "☕",
                string s when s.Contains("транспорт") => "🚗",
                string s when s.Contains("развлечения") => "🎮",
                string s when s.Contains("здоровье") => "💊",
                string s when s.Contains("одежда") => "👕",
                string s when s.Contains("жильё") || s.Contains("квартира") => "🏠",
                string s when s.Contains("связь") || s.Contains("телефон") => "📱",
                string s when s.Contains("образование") => "📚",
                _ => "💳"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}