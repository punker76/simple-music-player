using System;
using System.Globalization;
using System.Windows;

namespace SimpleMusicPlayer.ValueConverters
{
    /// <summary>
    /// special converter: converts enums to bool according to the parameter given
    /// </summary>
    public class EnumBooleanConverter : BooleanConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                var equals = value != null && Equals(value, parameter);
                return equals ? this.TrueValue : this.FalseValue;
            }
            return this.FalseValue;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new EnumBooleanConverter();
        }
    }
}