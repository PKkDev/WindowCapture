using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Drawing;
using System.IO;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using CaptureHelper.Extensions;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowCapture.WinApp.Dilogs.MediaFileDetail
{
    public sealed partial class Mp4DetailPage : Page, INotifyPropertyChanged
    {
        public MVVM.Model.MediaFileDetail FileDetail { get; set; }

        public VideoProperties VideoProps { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public Mp4DetailPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MVVM.Model.MediaFileDetail fileDetail)
            {
                FileDetail = fileDetail;
                await LoadDetail();
            }
            base.OnNavigatedTo(e);
        }

        private async Task LoadDetail()
        {
            VideoProps = await FileDetail.File.Properties.GetVideoPropertiesAsync();
            OnPropertyChanged("VideoProps");

            using MemoryStream ras = new();
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.GetVideoThumbnail(FileDetail.File.Path, ras, 1);
            var bmp = new Bitmap(ras);

            var previewVector = bmp.GetRGBAVector();
            IBuffer buffer = CryptographicBuffer.CreateFromByteArray(previewVector);
            SoftwareBitmap previewSB = new(BitmapPixelFormat.Rgba8, bmp.Width, bmp.Height);
            previewSB.CopyFromBuffer(buffer);

            SoftwareBitmapSource previewSBS = new();
            if (previewSB.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || previewSB.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                previewSB = SoftwareBitmap.Convert(previewSB, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            await previewSBS.SetBitmapAsync(previewSB);

            bmp.Dispose();
            ras.Close();
            ras.Dispose();

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                filePreview.Source = previewSBS;
            });
        }
    }
}
