using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleMusicPlayer.ValueConverters
{
    public class NullVisibilityConverter : IValueConverter
    {
        public Visibility TRUE { get; set; }

        public Visibility FALSE { get; set; }

        public bool Invert { get; set; }

        public NullVisibilityConverter()
        {
            TRUE = Visibility.Collapsed;
            FALSE = Visibility.Visible;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Invert ? (value == null ? FALSE : TRUE) : (value == null ? TRUE : FALSE);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}