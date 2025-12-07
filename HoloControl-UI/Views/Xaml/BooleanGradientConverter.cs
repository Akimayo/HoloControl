using System.Globalization;

namespace HoloControl.Views.Xaml
{
    class BooleanGradientConverter : IValueConverter
    {
        private const double OFFSET_ON = 0.0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && parameter is double offset) return b ? OFFSET_ON : offset;
            else throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d && parameter is double offset)
            {
                if (d == OFFSET_ON) return true;
                else if (d == offset) return false;
                else throw new ArgumentException("Invalid offset value", nameof(value));
            }
            else throw new NotImplementedException();
        }
    }
}
