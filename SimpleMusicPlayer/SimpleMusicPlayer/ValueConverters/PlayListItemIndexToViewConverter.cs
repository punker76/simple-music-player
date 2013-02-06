using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleMusicPlayer.ValueConverters
{
  public class PlayListItemIndexToViewConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value is int ? (int)value + 1 : -1;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return value is int ? (int)value - 1 : -1;
    }
  }
}