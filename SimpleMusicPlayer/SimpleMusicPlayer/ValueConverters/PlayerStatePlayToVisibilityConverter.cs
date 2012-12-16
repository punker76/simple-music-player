using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SimpleMusicPlayer.Common;

namespace SimpleMusicPlayer.ValueConverters
{
  public class PlayerStatePlayToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is PlayerState) {
        var playState = (PlayerState)value;
        return playState == PlayerState.Stop || playState == PlayerState.Pause ? Visibility.Visible : Visibility.Hidden;
      }
      return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return DependencyProperty.UnsetValue;
    }
  }
}