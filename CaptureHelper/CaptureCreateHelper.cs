using System;
using System.Runtime.InteropServices;
using Windows.Graphics.Capture;

namespace CaptureHelper
{
    public class CaptureCreateHelper
    {
        static readonly Guid GraphicsCaptureItemGuid = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

        [ComImport]
        [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        public interface IGraphicsCaptureItemInterop
        {
            IntPtr CreateForWindow(
                [In] IntPtr window,
                [In] ref Guid iid);

            IntPtr CreateForMonitor(
                [In] IntPtr monitor,
                [In] ref Guid iid);
        }

        public static GraphicsCaptureItem CreateItemForWindow(IntPtr hwnd)
        {
            IGraphicsCaptureItemInterop interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
            IntPtr itemPointer = interop.CreateForWindow(hwnd, GraphicsCaptureItemGuid);

            GraphicsCaptureItem item = GraphicsCaptureItem.FromAbi(itemPointer);

            var releaseRes = Marshal.Release(itemPointer);

            return item;
        }

        public static GraphicsCaptureItem CreateItemForMonitor(IntPtr hmon)
        {
            IGraphicsCaptureItemInterop interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
            IntPtr itemPointer = interop.CreateForMonitor(hmon, GraphicsCaptureItemGuid);

            GraphicsCaptureItem item = GraphicsCaptureItem.FromAbi(itemPointer);

            var releaseRes = Marshal.Release(itemPointer);

            return item;
        }
    }
}
