using System.Globalization;
using System.Text;

namespace HoloControl.Views.Xaml
{
    internal class TimeSpanFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts) {
                StringBuilder sb = new();
                if (ts.TotalHours >= 1) sb.Append($"{ts.Hours} {Resources.Strings.Standard.Hours}  ");
                if (ts.TotalMinutes >= 1) sb.Append($"{ts.Minutes} {Resources.Strings.Standard.Minutes}  ");
                sb.Append($"{ts.Seconds} {Resources.Strings.Standard.Seconds}");
                return sb.ToString();
            }
            else throw new ArgumentException("This converter accepts only values of type TimeSpan", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
