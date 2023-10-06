using HoloControl.Models;
using System.Globalization;

namespace HoloControl.Views.Xaml
{
    internal class ConnectionStatusNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ConnectionStatus status)
                switch (status)
                {
                    case ConnectionStatus.Connected: return Resources.Strings.Standard.Port_Status_Connected;
                    case ConnectionStatus.Disconnected: return Resources.Strings.Standard.Port_Status_Disconnected;
                    case ConnectionStatus.Connecting: return Resources.Strings.Standard.Port_Status_Connecting;
                    case ConnectionStatus.Error: return Resources.Strings.Standard.Port_Status_Error;
                    default: return "";
                }
            else throw new ArgumentException("Value must be of thy ConnectionStatus", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
