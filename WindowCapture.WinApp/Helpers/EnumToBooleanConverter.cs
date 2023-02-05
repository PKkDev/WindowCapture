using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System;

namespace WindowCapture.WinApp.Helpers
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public EnumToBooleanConverter() { }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string enumString)
            {
                if (!Enum.IsDefined(typeof(ElementTheme), value))
                    throw new ArgumentException($"Exception {nameof(EnumToBooleanConverter)} ValueMustBeAnEnum");

                var enumValue = Enum.Parse(typeof(ElementTheme), enumString);

                return enumValue.Equals(value);
            }

            throw new ArgumentException($"Exception {nameof(EnumToBooleanConverter)} ParameterMustBeAnEnumName");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string enumString)
                return Enum.Parse(typeof(ElementTheme), enumString);

            throw new ArgumentException($"Exception {nameof(EnumToBooleanConverter)} ParameterMustBeAnEnumName");
        }
    }
}
