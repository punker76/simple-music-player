using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SimpleMusicPlayer.Core;

namespace SimpleMusicPlayer.ValueConverters
{
    public class PlayerStateToToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PlayerState)
            {
                var playState = (PlayerState)value;
                return playState == PlayerState.Stop || playState == PlayerState.Pause ? "Play (Space)" : "Pause (Space)";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}