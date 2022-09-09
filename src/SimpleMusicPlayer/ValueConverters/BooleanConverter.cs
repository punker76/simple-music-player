using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleMusicPlayer.ValueConverters
{
    public class BooleanConverter : Freezable, IValueConverter
    {
        public static readonly DependencyProperty TrueValueProperty
            = DependencyProperty.Register(nameof(TrueValue), typeof(object), typeof(BooleanConverter), new PropertyMetadata(default(object)));

        public object TrueValue
        {
            get => (object) this.GetValue(TrueValueProperty);
            set => this.SetValue(TrueValueProperty, value);
        }

        public static readonly DependencyProperty FalseValueProperty
            = DependencyProperty.Register(nameof(FalseValue), typeof(object), typeof(BooleanConverter), new PropertyMetadata(default(object)));

        public object FalseValue
        {
            get => (object) this.GetValue(FalseValueProperty);
            set => this.SetValue(FalseValueProperty, value);
        }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as bool?).GetValueOrDefault() ? this.TrueValue : this.FalseValue;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BooleanConverter();
        }
    }
}