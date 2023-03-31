using System;

namespace CaptureHelper
{
    public class CaptureItemSelected
    {
        public CaptureItemSelectedType Type { get; set; }

        public IntPtr Handler { get; set; }

        public CaptureItemSelected(CaptureItemSelectedType type, IntPtr handler)
        {
            Type = type;
            Handler = handler;
        }
    }

    public enum CaptureItemSelectedType
    {
        Process,
        Monitor
    }
}
