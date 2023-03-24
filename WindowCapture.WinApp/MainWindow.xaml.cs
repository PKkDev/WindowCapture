using Microsoft.UI.Xaml;
using System;
using System.Drawing;
using Windows.Graphics.Imaging;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Windows.Storage.Streams;
using Windows.Media;
using Windows.Security.Cryptography;
using Windows.Graphics.Capture;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Foundation.Metadata;
using static WindowCapture.WinApp.WindowEnumerationHelper;
using System.Threading.Tasks;

namespace WindowCapture.WinApp
{
    public class WindowInfo
    {
        public string Title { get; set; }
        public IntPtr HWND { get; set; }
        public Bitmap IconB { get; set; }
        public SoftwareBitmap IconSB { get; set; }
    }

    public sealed partial class MainWindow : Window
    {
        private IntPtr hWnd;
        private Microsoft.UI.Windowing.AppWindow App;
        Microsoft.UI.WindowId windowId;

        public MainWindow()
        {
            InitializeComponent();

            hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            App = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd));

            //App.TitleBar.ExtendsContentIntoTitleBar = true;
            //App.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            //App.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            //App.TitleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(50, 255, 255, 255);
            //App.TitleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(90, 255, 255, 255);

            //MakeTransparent();

            //Maximize();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            //await StartPickerCaptureAsync();

            ObservableCollection<Process> processes = new();

            #region InitWindowList

            if (ApiInformation.IsApiContractPresent(typeof(Windows.Foundation.UniversalApiContract).FullName, 8))
            {
                var processesWithWindows = from p in Process.GetProcesses()
                                           where !string.IsNullOrWhiteSpace(p.MainWindowTitle) && WindowEnumerationHelper.IsWindowValidForCapture(p.MainWindowHandle)
                                           select p;
                processes = new ObservableCollection<Process>(processesWithWindows);
            }

            #endregion InitWindowList

            var process = processes
                .FirstOrDefault(x => x.MainWindowTitle.Equals("Диспетчер задач"));
            var processHWND = process.MainWindowHandle;
            Microsoft.UI.WindowId h = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(processHWND);
            Windows.UI.WindowId hh = new Windows.UI.WindowId(h.Value);

            IntPtr hWnd2 = WinRT.Interop.WindowNative.GetWindowHandle(this);

            var pdi = Windows.System.Diagnostics.ProcessDiagnosticInfo.TryGetForProcessId((uint)process.Id);

            // var g = GraphicsCaptureItem.TryCreateFromWindowId(hh);
            StartHwndCapture(processHWND);

            ObservableCollection<MonitorInfo> monitors = new();

            #region InitMonitorList

            if (ApiInformation.IsApiContractPresent(typeof(Windows.Foundation.UniversalApiContract).FullName, 8))
                monitors = new ObservableCollection<MonitorInfo>(MonitorEnumerationHelper.GetMonitors());

            #endregion InitMonitorList

            //var monitor = monitors.FirstOrDefault(x => x.DeviceName.Equals(""));
            //var monitorHMON = monitor.Hmon;
            //StartHmonCapture(monitorHMON);

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

            var first = windowInfo1.FirstOrDefault(x => x.Title.Equals("WindowCapture (Running) - Microsoft Visual Studio"));
            Microsoft.UI.WindowId windowIdM = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(first.HWND);
            IntPtr window = Microsoft.UI.Win32Interop.GetWindowFromWindowId(windowIdM);
            //var apw = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowIdM);
        }

        private void Maximize() => Win32.ShowWindow(hWnd, 3);

        private void Normal() => Win32.ShowWindow(hWnd, 1);

        private async Task StartPickerCaptureAsync()
        {
            GraphicsCapturePicker picker = new();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);
            GraphicsCaptureItem item = await picker.PickSingleItemAsync();

            //var picker = new GraphicsCapturePicker();
            //picker.SetWindow(hWnd);
            //GraphicsCaptureItem item = await picker.PickSingleItemAsync();

            if (item != null)
            {
                //sample.StartCaptureFromItem(item);
            }
        }

        private void StartHwndCapture(IntPtr hwnd)
        {
            GraphicsCaptureItem item = CaptureHelper.CreateItemForWindow(hwnd);
            if (item != null)
            {
                //sample.StartCaptureFromItem(item);
            }
        }

        private void StartHmonCapture(IntPtr hmon)
        {
            GraphicsCaptureItem item = CaptureHelper.CreateItemForMonitor(hmon);
            if (item != null)
            {
                //sample.StartCaptureFromItem(item);
            }
        }

    }
}
