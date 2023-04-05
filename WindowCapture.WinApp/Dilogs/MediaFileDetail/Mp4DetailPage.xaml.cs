using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System.Drawing;
using System.IO;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using CaptureHelper.Extensions;
using System.Threading.Tasks;

namespace WindowCapture.WinApp.Dilogs.MediaFileDetail
{
    public sealed partial class Mp4DetailPage : Page
    {
        private MVVM.Model.MediaFileDetail _fileDetail { get; set; }

        public Mp4DetailPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MVVM.Model.MediaFileDetail fileDetail)
            {
                _fileDetail = fileDetail;
                await LoadDetail();
            }
            base.OnNavigatedTo(e);
        }

        private async Task LoadDetail()
        {
            using MemoryStream ras = new();
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.GetVideoThumbnail(_fileDetail.File.Path, ras, 1);
            //var bmp = Bitmap.FromStream(ras);
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
        }
    }
}
