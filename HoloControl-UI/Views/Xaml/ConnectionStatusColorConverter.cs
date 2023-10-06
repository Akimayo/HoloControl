using HoloControl.Models;
using System.Globalization;

namespace HoloControl.Views.Xaml
{
    internal class ConnectionStatusColorConverter : IValueConverter
    {
        private readonly ResourceDictionary theme = Application.Current.Resources.MergedDictionaries.ElementAt(0);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ConnectionStatus status)
                return status switch
                {
                    ConnectionStatus.Connected => theme["Blue100Accent"],
                    ConnectionStatus.Connecting => theme["Blue300Accent"],
                    ConnectionStatus.Disconnected => theme["Orange300Accent"],
                    ConnectionStatus.Error => theme["Red100Accent"],
                    _ => theme["Red300Accent"],
                };
            else throw new ArgumentException("Value must be of type ConnectionStatus", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
