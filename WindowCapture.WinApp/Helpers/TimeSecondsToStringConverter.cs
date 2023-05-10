using Microsoft.UI.Xaml.Data;
using System;
using WindowCapture.WinApp.Extensios;

namespace WindowCapture.WinApp.Helpers
{
    public class TimeSecondsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is double timeSec)
            {
                var time = TimeSpan.FromSeconds(timeSec);
                return time.ConvertToStr();
            }

            throw new ArgumentException($"Exception {nameof(TimeSecondsToStringConverter)} ParameterMustBeAnEnumName");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new ArgumentException($"Exception {nameof(TimeSecondsToStringConverter)} ParameterMustBeAnEnumName");
        }
    }
}
