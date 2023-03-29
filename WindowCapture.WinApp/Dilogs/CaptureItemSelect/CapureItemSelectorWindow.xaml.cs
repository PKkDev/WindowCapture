using CaptureHelper;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation.Metadata;
using System.Text;
using System.Drawing;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Graphics.Capture;

namespace WindowCapture.WinApp.Dilogs.CaptureItemSelect
{
    public class WindowInfo
    {
        public string Title { get; set; }
        public IntPtr HWND { get; set; }
        public Bitmap IconB { get; set; }
        public SoftwareBitmap IconSB { get; set; }
    }

    public sealed partial class CapureItemSelectorWindow : Window
    {
        public CapureItemSelectorWindow()
        {
            this.InitializeComponent();

            #region InitWindowList

            ObservableCollection<Process> processes = new();

            if (ApiInformation.IsApiContractPresent(typeof(Windows.Foundation.UniversalApiContract).FullName, 8))
            {
                var processesWithWindows = from p in Process.GetProcesses()
                                           where !string.IsNullOrWhiteSpace(p.MainWindowTitle) && WindowEnumerationHelper.IsWindowValidForCapture(p.MainWindowHandle)
                                           select p;
                processes = new ObservableCollection<Process>(processesWithWindows);
            }

            #endregion InitWindowList

            #region InitMonitorList

            ObservableCollection<MonitorInfo> monitors = new();

            if (ApiInformation.IsApiContractPresent(typeof(Windows.Foundation.UniversalApiContract).FullName, 8))
                monitors = new ObservableCollection<MonitorInfo>(MonitorEnumerationHelper.GetMonitors());

            #endregion InitMonitorList


            var process = processes
                .FirstOrDefault(x => x.MainWindowTitle.Equals("Диспетчер задач"));

            var processHWND = process.MainWindowHandle;
            Microsoft.UI.WindowId h = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(processHWND);

            var pdi = Windows.System.Diagnostics.ProcessDiagnosticInfo.TryGetForProcessId((uint)process.Id);


            #region test

            List<WindowInfo> windowInfo1 = new();
            Win32.EnumDelegate filter = delegate (IntPtr hWnd, int lParam)
            {
                StringBuilder strbTitle = new(255);
                int nLength = Win32.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                string strTitle = strbTitle.ToString();

                if (Win32.IsWindowVisible(hWnd) && !string.IsNullOrEmpty(strTitle))
                {
                    WindowInfo wi = new();
                    wi.Title = strTitle;
                    wi.HWND = hWnd;

                    IntPtr hIcon = default;
                    hIcon = Win32.SendMessage(hWnd, Win32.WM_GETICON, Win32.ICON_SMALL2, IntPtr.Zero);
                    if (hIcon == IntPtr.Zero) hIcon = Win32.GetClassLongPtr(hWnd, Win32.GCL_HICON);
                    if (hIcon == IntPtr.Zero) hIcon = Win32.LoadIcon(IntPtr.Zero, Win32.IDI_APPLICATION);
                    if (hIcon != IntPtr.Zero)
                    {
                        var icon = new Bitmap(Icon.FromHandle(hIcon).ToBitmap(), 16, 16);
                        wi.IconB = icon;

                        ImageConverter converter = new();
                        var b = (byte[])converter.ConvertTo(icon, typeof(byte[]));
                        IBuffer buffer = CryptographicBuffer.CreateFromByteArray(b);
                        SoftwareBitmap softwareBitmap = new(BitmapPixelFormat.Gray8, icon.Width, icon.Height);
                        softwareBitmap.CopyFromBuffer(buffer);
                        VideoFrame inputImage = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
                        wi.IconSB = softwareBitmap;
                    }

                    windowInfo1.Add(wi);
                }
                return true;
            };
            Win32.EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero);

            var first = windowInfo1.FirstOrDefault(x => x.Title.Equals("Диспетчер задач"));
            Microsoft.UI.WindowId windowIdM = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(first.HWND);
            IntPtr window = Microsoft.UI.Win32Interop.GetWindowFromWindowId(windowIdM);

            #endregion test

            IntPtr hwnd = processHWND;
            GraphicsCaptureItem item = CaptureCreateHelper.CreateItemForWindow(hwnd);

        }
    }
}
