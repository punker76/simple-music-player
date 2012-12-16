using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleMusicPlayer.ValueConverters
{
  public class EnumToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (parameter != null) {
        if (value == null) {
          return Visibility.Hidden;
        } else {
          var equals = Equals(value, parameter);
          return equals ? Visibility.Visible : Visibility.Hidden;
        }
      }
      return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return DependencyProperty.UnsetValue;
    }
  }
}