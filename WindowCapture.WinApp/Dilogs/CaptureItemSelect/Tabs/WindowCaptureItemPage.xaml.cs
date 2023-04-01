using CaptureHelper;
using CaptureHelper.Model;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using CaptureHelper.Extensions;
using Microsoft.UI.Xaml.Navigation;

namespace WindowCapture.WinApp.Dilogs.CaptureItemSelect.Tabs
{
    public sealed partial class WindowCaptureItemPage : Page
    {
        ObservableCollection<WindowInfo> WindowInfos { get; set; }

        public WindowCaptureItemPage()
        {
            InitializeComponent();

            WindowInfos = new();
            LoadWindowsInformation();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async void LoadWindowsInformation()
        {
            WindowInfos = new();
            if (ApiInformation.IsApiContractPresent(typeof(Windows.Foundation.UniversalApiContract).FullName, 8))
            {
                var processesWithWindows = from p in Process.GetProcesses()
                                           where !string.IsNullOrWhiteSpace(p.MainWindowTitle) && WindowEnumerationHelper.IsWindowValidForCapture(p.MainWindowHandle)
                                           select p;
                foreach (var process in processesWithWindows)
                {
                    WindowInfo newInfo = new(process);

                    //Microsoft.UI.WindowId windowIdM = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(process.MainWindowHandle);
                    //ProcessDiagnosticInfo pdi = Windows.System.Diagnostics.ProcessDiagnosticInfo.TryGetForProcessId((uint)process.Id);
                    //IntPtr window = Microsoft.UI.Win32Interop.GetWindowFromWindowId(windowIdM);

                    IntPtr hIcon = default;
                    hIcon = Win32.SendMessage(process.MainWindowHandle, Win32.WM_GETICON, Win32.ICON_SMALL, IntPtr.Zero);
                    if (hIcon == IntPtr.Zero) hIcon = Win32.GetClassLongPtr(process.MainWindowHandle, Win32.GCL_HICON);
                    if (hIcon == IntPtr.Zero) hIcon = Win32.LoadIcon(IntPtr.Zero, Win32.IDI_APPLICATION);
                    if (hIcon != IntPtr.Zero)
                    {
                        Bitmap icon = new(Icon.FromHandle(hIcon).ToBitmap(), 32, 32);

                        //icon.Save($"C:\\Users\\prode\\Downloads\\{newInfo.Title}.png");

                        var b = icon.GetRGBAVector();
                        IBuffer buffer = CryptographicBuffer.CreateFromByteArray(b);
                        SoftwareBitmap softwareBitmap = new(BitmapPixelFormat.Rgba8, icon.Width, icon.Height);
                        softwareBitmap.CopyFromBuffer(buffer);
                        //VideoFrame inputImage = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);

                        SoftwareBitmapSource sbs = new();
                        if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Ignore)
                            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore);
                        await sbs.SetBitmapAsync(softwareBitmap);
                        newInfo.IconSource = sbs;

                        //bmp.Save($"C:\\Users\\prode\\Downloads\\test.png");

                        //GraphicsCaptureItem _captureItem = CaptureCreateHelper.CreateItemForWindow(newInfo.HWND);

                        //CanvasDevice _canvasDevice = new CanvasDevice();
                        //var _framePool = Direct3D11CaptureFramePool.Create(
                        //    _canvasDevice,
                        //    Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized,
                        //    2,
                        //    _captureItem.Size);

                        //GraphicsCaptureSession _session = null;

                        //_framePool.FrameArrived += async (Direct3D11CaptureFramePool sender, object args) =>
                        //{
                        //    using Direct3D11CaptureFrame frame = _framePool.TryGetNextFrame();
                        //    var _currentFrame = frame.Surface;

                        //    SoftwareBitmap softwareBitmap = null;
                        //    softwareBitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(frame.Surface);
                        //    if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                        //        softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                        //    SoftwareBitmapSource sbs = new();
                        //    await sbs.SetBitmapAsync(softwareBitmap);
                        //    newInfo.Source = sbs;

                        //    _framePool.Dispose();
                        //    _session.Dispose();
                        //};

                        //_session = _framePool.CreateCaptureSession(_captureItem);
                        //_session.StartCapture();

                        icon.Dispose();
                        icon = null;
                    }

                    WindowInfos.Add(newInfo);
                }
            }
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WindowsList.SelectedItem is WindowInfo wf)
                App.CaptureItemSelected = new(CaptureItemSelectedType.Process, wf.HWND);
        }
    }
}
