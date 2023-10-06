using System.Globalization;

namespace HoloControl.Views.Xaml
{
    internal class BoolNegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            else throw new ArgumentException("Value must be of type bool", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            else throw new ArgumentException("Value must be of type bool", nameof(value));
        }
    }
}
