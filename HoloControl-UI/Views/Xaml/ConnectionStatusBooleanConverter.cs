using HoloControl.Models;
using System.Globalization;

namespace HoloControl.Views.Xaml
{
    internal class ConnectionStatusBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ConnectionStatus status) return status == ConnectionStatus.Connected;
            else throw new ArgumentException("Value must be of type ConnectionStatus", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
