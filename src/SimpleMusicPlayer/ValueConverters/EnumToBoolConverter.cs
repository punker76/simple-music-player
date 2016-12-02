using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleMusicPlayer.ValueConverters
{
    /// <summary>
    /// special converter: converts enums to bool according to the parameter given
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                var equals = value != null && Equals(value, parameter);
                return equals;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                if (value != null)
                {
                    // check for flags enum
                    if (value is bool)
                    {
                        return parameter;
                    }
                }
                return DefaultEnumValue(parameter.GetType());
            }
            return Binding.DoNothing;
        }

        private static object DefaultEnumValue(Type enumType)
        {
            if (enumType != null)
            {
                if (enumType.IsEnum)
                {
                    var convert = Enum.GetValues(enumType);
                    return convert.GetValue(0);
                }
                else
                {
                    throw new ArgumentException("given type is not an enum");
                }
            }
            return null;
        }
    }
}