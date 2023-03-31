using CaptureHelper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Security.Cryptography.Xml;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace WindowCapture.WinApp.Dilogs.CaptureItemSelect.Tabs
{
    public sealed partial class MonitorCaptureItemPage : Page
    {
        ObservableCollection<MonitorInfo> MonitorInfos { get; set; }

        public MonitorCaptureItemPage()
        {
            this.InitializeComponent();

            MonitorInfos = new();
            LoadMonitorsInformation();
        }

        private async void LoadMonitorsInformation()
        {
            MonitorInfos = new();

            if (ApiInformation.IsApiContractPresent(typeof(Windows.Foundation.UniversalApiContract).FullName, 8))
            {
                var monitors = MonitorEnumerationHelper.GetMonitors();
                foreach (var monitor in monitors)
                {
                    Point location = new((int)monitor.MonitorArea.X, (int)monitor.MonitorArea.Y);
                    Size size = new((int)monitor.MonitorArea.Width, (int)monitor.MonitorArea.Height);
                    Rectangle bounds = new Rectangle(location, size);
                    Bitmap bmp = new Bitmap((int)monitor.MonitorArea.Width, (int)monitor.MonitorArea.Height);

                    using Graphics g = Graphics.FromImage(bmp);
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);

                    //bmp.Save($"C:\\Users\\prode\\Downloads\\{monitor.DeviceName}.png");

                    var b = GetVectorByImgPath(bmp);
                    IBuffer buffer = CryptographicBuffer.CreateFromByteArray(b);
                    SoftwareBitmap softwareBitmap = new(BitmapPixelFormat.Rgba8, bmp.Width, bmp.Height);
                    softwareBitmap.CopyFromBuffer(buffer);
                    //VideoFrame inputImage = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);

                    SoftwareBitmapSource sbs = new();
                    if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Ignore)
                        softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore);
                    await sbs.SetBitmapAsync(softwareBitmap);
                    monitor.PreviewSource = sbs;

                    MonitorInfos.Add(monitor);
                }
            }
        }

        byte[] GetVectorByImgPath(Bitmap btmToPrse)
        {
            List<byte> listT = new();
            //if (btmToPrse.Height != ImageNetSettings.imageHeight || btmToPrse.Width != ImageNetSettings.imageWidth)
            //    btmToPrse = new Bitmap(btmToPrse, ImageNetSettings.imageWidth, ImageNetSettings.imageHeight);
            for (int y = 0; y < btmToPrse.Height; y++)
            {
                for (int x = 0; x < btmToPrse.Width; x++)
                {
                    Color clr = btmToPrse.GetPixel(x, y);
                    listT.Add(clr.R);
                    listT.Add(clr.G);
                    listT.Add(clr.B);
                    listT.Add(clr.A);
                }
            }
            //btmToPrse.Dispose();
            return listT.ToArray();
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = MonitorsList.SelectedItem;
            if (selected is MonitorInfo wf)
            {
                IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(Window.Current);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            }
        }
    }
}
