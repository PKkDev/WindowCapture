using System;

namespace WindowCapture.WinApp.Extensios
{
    public static class TimeSpanExtensions
    {
        public static string ConvertToStr(this TimeSpan span)
        {
            string formatted = string.Format(
                "{0}{1}{2}{3}",
                span.Duration().Days > 0
                    ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s")
                    : string.Empty,
                span.Duration().Hours > 0
                    ? string.Format("{0:0} h{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s")
                    : string.Empty,
                span.Duration().Minutes > 0
                    ? string.Format("{0:0} min{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s")
                    : string.Empty,
                span.Duration().Seconds > 0
                    ? string.Format("{0:0} sec{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s")
                    : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 sec";

            return formatted;
        }
    }
}
