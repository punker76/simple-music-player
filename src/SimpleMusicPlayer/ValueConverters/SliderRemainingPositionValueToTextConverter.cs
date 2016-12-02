using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace SimpleMusicPlayer.ValueConverters
{
    public class SliderRemainingPositionValueToTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 1 && values[0] is double && values[1] is double)
            {
                // all values in ms
                var currentposition = TimeSpan.FromMilliseconds((double)values[0]);
                var length = TimeSpan.FromMilliseconds((double)values[1]);
                return string.Format("-{0:m\\:ss}", length - currentposition);
            }
            return "-0:00";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return targetTypes.Select(t => DependencyProperty.UnsetValue).ToArray();
        }
    }
}