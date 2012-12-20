using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SimpleMusicPlayer.ValueConverters
{
  public class ModeToColorConverter : IValueConverter
  {
    public Color TrueColor { get; set; }
    public Color FalseColor { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value is bool && (bool)value ? this.TrueColor : this.FalseColor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return DependencyProperty.UnsetValue;
    }
  }
}