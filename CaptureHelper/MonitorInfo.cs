using Microsoft.UI.Xaml.Media;
using System;
using System.Numerics;
using Windows.Foundation;

namespace CaptureHelper
{
    public class MonitorInfo
    {
        public bool IsPrimary { get; set; }

        public Vector2 ScreenSize { get; set; }

        public Rect MonitorArea { get; set; }

        public Rect WorkArea { get; set; }

        public string DeviceName { get; set; }

        public IntPtr Hmon { get; set; }

        public ImageSource PreviewSource { get; set; }
    }
}
