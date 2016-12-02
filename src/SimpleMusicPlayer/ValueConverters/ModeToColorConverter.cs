using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SimpleMusicPlayer.ValueConverters
{
    public class ModeToColorConverter : Freezable, IValueConverter
    {
        public static readonly DependencyProperty TrueColorProperty =
          DependencyProperty.Register("TrueColor", typeof(Color), typeof(ModeToColorConverter), new PropertyMetadata(default(Color)));

        public Color TrueColor
        {
            get { return (Color)this.GetValue(TrueColorProperty); }
            set { this.SetValue(TrueColorProperty, value); }
        }

        public static readonly DependencyProperty FalseColorProperty =
          DependencyProperty.Register("FalseColor", typeof(Color), typeof(ModeToColorConverter), new PropertyMetadata(default(Color)));

        public Color FalseColor
        {
            get { return (Color)this.GetValue(FalseColorProperty); }
            set { this.SetValue(FalseColorProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && (bool)value ? this.TrueColor : this.FalseColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ModeToColorConverter();
        }
    }
}