using System.Globalization;

namespace HoloControl.Views.Xaml
{
    class ConsoleHeightConverter : IValueConverter
    {
        const double HEIGHT_OFFSET = 160;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double height) return height - HEIGHT_OFFSET;
            else throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double height) return height + HEIGHT_OFFSET;
            else throw new NotImplementedException();
        }
    }
}
