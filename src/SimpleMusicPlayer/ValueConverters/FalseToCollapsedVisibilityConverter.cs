using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleMusicPlayer.ValueConverters
{
    public class FalseToCollapsedVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is Visibility) || (Visibility)value != Visibility.Collapsed;
        }

        private static FalseToCollapsedVisibilityConverter instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static FalseToCollapsedVisibilityConverter()
        {
        }

        private FalseToCollapsedVisibilityConverter()
        {
        }

        public static FalseToCollapsedVisibilityConverter Instance => instance ?? (instance = new FalseToCollapsedVisibilityConverter());
    }
}