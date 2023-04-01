using CaptureHelper;
using CaptureHelper.Model;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using CaptureHelper.Extensions;
using Microsoft.UI.Xaml.Navigation;

namespace WindowCapture.WinApp.Dilogs.CaptureItemSelect.Tabs
{
    public sealed partial class MonitorCaptureItemPage : Page
    {
        ObservableCollection<MonitorInfo> MonitorInfos { get; set; }

        public MonitorCaptureItemPage()
        {
            InitializeComponent();

            MonitorInfos = new();
            LoadMonitorsInformation();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
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
                    Rectangle bounds = new(location, size);
                    Bitmap preview = new((int)monitor.MonitorArea.Width, (int)monitor.MonitorArea.Height);

                    using Graphics previewG = Graphics.FromImage(preview);
                    previewG.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);

                    //bmp.Save($"C:\\Users\\prode\\Downloads\\{monitor.DeviceName}.png");

                    var previewVector = preview.GetRGBAVector();
                    IBuffer buffer = CryptographicBuffer.CreateFromByteArray(previewVector);
                    SoftwareBitmap previewSB = new(BitmapPixelFormat.Rgba8, preview.Width, preview.Height);
                    previewSB.CopyFromBuffer(buffer);

                    SoftwareBitmapSource previewSBS = new();
                    if (previewSB.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || previewSB.BitmapAlphaMode != BitmapAlphaMode.Ignore)
                        previewSB = SoftwareBitmap.Convert(previewSB, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore);
                    await previewSBS.SetBitmapAsync(previewSB);
                    monitor.PreviewSource = previewSBS;

                    MonitorInfos.Add(monitor);

                    preview.Dispose();
                    preview = null;
                }
            }
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MonitorsList.SelectedItem is MonitorInfo wf)
                App.CaptureItemSelected = new(CaptureItemSelectedType.Monitor, wf.Hmon);
        }
    }
}
