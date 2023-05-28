using System;
using System.Numerics;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using Windows.Foundation;
using WindowCapture.WinApp.MVVM.ViewModel;

namespace WindowCapture.WinApp.MVVM.View
{
    public sealed partial class CapturePage : Page
    {
        private readonly CaptureViewModel ViewModel;

        // Non-API related members.
        private CompositionGraphicsDevice _compositionGraphicsDevice;
        private Compositor _compositor;
        private CompositionDrawingSurface _surface;

        public CapturePage()
        {
            InitializeComponent();

            DataContext = ViewModel = App.GetService<CaptureViewModel>();

            ViewModel.FillSurfaceWithBitmap += (object sender, CanvasBitmap canvasBitmap) =>
            {
                try
                {
                    CanvasComposition.Resize(_surface, canvasBitmap.Size);
                    using var session = CanvasComposition.CreateDrawingSession(_surface);
                    session.Clear(Colors.Transparent);
                    session.DrawImage(canvasBitmap);
                }
                catch (Exception ex)
                {

                }
            };

            Setup();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void Setup()
        {
            _compositor = App.MainWindow.Compositor;

            _compositionGraphicsDevice = CanvasComposition
                .CreateCompositionGraphicsDevice(_compositor, ViewModel._canvasDevice);

            _surface = _compositionGraphicsDevice.CreateDrawingSurface(
                new Size(400, 400),
                Microsoft.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized,
                Microsoft.Graphics.DirectX.DirectXAlphaMode.Premultiplied);

            var visual = _compositor.CreateSpriteVisual();
            visual.RelativeSizeAdjustment = Vector2.One;
            var brush = _compositor.CreateSurfaceBrush(_surface);
            brush.HorizontalAlignmentRatio = 0.5f;
            brush.VerticalAlignmentRatio = 0.5f;
            brush.Stretch = CompositionStretch.Uniform;
            visual.Brush = brush;

            ElementCompositionPreview.SetElementChildVisual(gridToPreview, visual);
        }

    }

}

/*
        private MediaCapture mediaCapture;
        private MediaCaptureInitializationSettings mediaCaptureSettings;
        private LowLagMediaRecording MediaRecording;

        StorageFile fileMicroAudio;

        DeviceInformation toCapt;

        //private async void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        //{
        //    await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalCacheFolder);
        //}


            #region
            string midiInputQueryString = MidiInPort.GetDeviceSelector();
            DeviceInformationCollection midiInputDevices = await DeviceInformation.FindAllAsync(midiInputQueryString);
            #endregion

            #region
            string midiOutportQueryString = MidiOutPort.GetDeviceSelector();
            DeviceInformationCollection midiOutputDevices = await DeviceInformation.FindAllAsync(midiOutportQueryString);
            #endregion

            #region micro
            string audioCaptureSelector = MediaDevice.GetAudioCaptureSelector();
            var audioCapture = await DeviceInformation.FindAllAsync(audioCaptureSelector);

            string b = MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Communications);
            DeviceInformation b1 = await DeviceInformation.CreateFromIdAsync(b);
            #endregion micro

            #region динамики
            string audioRenderSelector = MediaDevice.GetAudioRenderSelector();
            var audioRender = await DeviceInformation.FindAllAsync(audioRenderSelector);

            string a = MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default);
            DeviceInformation a1 = await DeviceInformation.CreateFromIdAsync(a);
            #endregion динамики

 */


/*
 
                 Task sendSignalTask = Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            while (framesToSend.Count == 0) { }

                            var bytes = framesToSend.Dequeue();

                            SoftwareBitmap softwareBitmap = null;
                            softwareBitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(bytes);
                            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                            float newWidth = softwareBitmap.PixelWidth / 5;
                            float newHeight = softwareBitmap.PixelHeight / 5;
                            using var resourceCreator = CanvasDevice.GetSharedDevice();
                            using var canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(resourceCreator, softwareBitmap);
                            using CanvasRenderTarget canvasRenderTarget = new(resourceCreator, newWidth, newHeight, canvasBitmap.Dpi);
                            using var drawingSession = canvasRenderTarget.CreateDrawingSession();
                            using ScaleEffect scaleEffect = new();

                            scaleEffect.Source = canvasBitmap;
                            scaleEffect.Scale = new System.Numerics.Vector2(newWidth / softwareBitmap.PixelWidth, newHeight / softwareBitmap.PixelHeight);
                            drawingSession.DrawImage(scaleEffect);
                            drawingSession.Flush();

                            var pixels = canvasRenderTarget.GetPixelBytes();
                            var c = pixels.Count(x => x != 0);

                            if (pixels.Any())
                                await connection.InvokeAsync("UploadStream", pixels);

                            //var news = SoftwareBitmap.CreateCopyFromBuffer(canvasRenderTarget.GetPixelBytes().AsBuffer(), BitmapPixelFormat.Bgra8, (int)newWidth, (int)newHeight, BitmapAlphaMode.Premultiplied);

                            softwareBitmap?.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                });
 
 */