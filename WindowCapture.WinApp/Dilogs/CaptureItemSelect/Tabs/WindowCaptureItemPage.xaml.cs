using CaptureHelper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

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

                    IntPtr hIcon = default;
                    hIcon = Win32.SendMessage(process.MainWindowHandle, Win32.WM_GETICON, Win32.ICON_SMALL, IntPtr.Zero);
                    if (hIcon == IntPtr.Zero) hIcon = Win32.GetClassLongPtr(process.MainWindowHandle, Win32.GCL_HICON);
                    if (hIcon == IntPtr.Zero) hIcon = Win32.LoadIcon(IntPtr.Zero, Win32.IDI_APPLICATION);
                    if (hIcon != IntPtr.Zero)
                    {
                        var icon = new Bitmap(Icon.FromHandle(hIcon).ToBitmap(), 32, 32);

                        //icon.Save($"C:\\Users\\prode\\Downloads\\{newInfo.Title}.png");

                        //ImageConverter converter = new();
                        //var b = (byte[])converter.ConvertTo(icon, typeof(byte[]));
                        var b = GetVectorByImgPath(icon);
                        IBuffer buffer = CryptographicBuffer.CreateFromByteArray(b);
                        SoftwareBitmap softwareBitmap = new(BitmapPixelFormat.Rgba8, icon.Width, icon.Height);
                        softwareBitmap.CopyFromBuffer(buffer);
                        //VideoFrame inputImage = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);

                        SoftwareBitmapSource sbs = new();
                        if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Ignore)
                            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore);
                        await sbs.SetBitmapAsync(softwareBitmap);
                        newInfo.IconSource = sbs;


                        Rectangle bounds = new Rectangle(0, 0, 100, 100);
                        Bitmap bmp = new Bitmap(100, 100);

                        using Graphics g = Graphics.FromImage(bmp);
                        g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);

                        bmp.Save($"C:\\Users\\prode\\Downloads\\test.png");


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

                    }

                    WindowInfos.Add(newInfo);
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
            var selected = WindowsList.SelectedItem;
            if (selected is WindowInfo wf)
            {
                CaptureItemSelected cpis = new(CaptureItemSelectedType.Process, wf.HWND);
                App.CaptureItemSelected = cpis;
            }
        }
    }
}
