using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;

using Windows.System;
using Windows.Devices.Enumeration;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics;
using Windows.Media.Core;
using Windows.Media.Devices;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Devices.PointOfService;
using Windows.Foundation;
// NAudio
using NAudio.Wave;
// SignalR
using Microsoft.AspNetCore.SignalR.Client;
using NAudio.CoreAudioApi;
using WindowCapture.WinApp.Helpers;
using Windows.Media.Audio;
using System.Linq;
using Windows.Media.Capture;

namespace WindowCapture.WinApp.MVVM.View
{
    public sealed class SurfaceWithInfo : IDisposable
    {
        public IDirect3DSurface Surface { get; internal set; }
        public TimeSpan SystemRelativeTime { get; internal set; }

        public void Dispose()
        {
            Surface?.Dispose();
            Surface = null;
        }
    }

    public static class CaptureSettings
    {
        // WxH
        public static SizeUInt32[] Resolutions => new SizeUInt32[]
        {
            new SizeUInt32() { Width = 640, Height = 480 },
            new SizeUInt32() { Width = 1280, Height = 720 },
            new SizeUInt32() { Width = 1920, Height = 1080 },
            new SizeUInt32() { Width = 3840, Height = 2160 },
            new SizeUInt32() { Width = 7680, Height = 4320 }
        };

        //  N/1000000 Mbps
        //var temp = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
        //var bitrate = temp.Video.Bitrate;
        public static uint[] Bitrates => new uint[] { 9000000, 18000000, 36000000, 72000000 };

        // N fps
        public static uint[] FrameRates => new uint[] { 24, 30, 60 };
    }

    public sealed partial class CapturePage : Page
    {
        // PCAudioCapture API objects.
        private SizeInt32 _lastSize;
        private GraphicsCaptureItem _captureItem;
        private Direct3D11CaptureFramePool _framePool;
        private GraphicsCaptureSession _session;

        // Non-API related members.
        private CanvasDevice _canvasDevice;
        private CompositionGraphicsDevice _compositionGraphicsDevice;
        private Compositor _compositor;
        private CompositionDrawingSurface _surface;
        private IDirect3DSurface _currentFrame;

        public TimeSpan StartRecoedVide;
        private double _videoRecordedSeconds;
        public double VideoRecordedSeconds
        {
            get => _videoRecordedSeconds;
            set { _videoRecordedSeconds = value; }
        }

        #region
        public Queue<SurfaceWithInfo> framesToSave = new();
        public Queue<IDirect3DSurface> framesToSend = new();
        public bool _isRecording = false;
        #endregion

        #region capture microphone v1
        private MediaCapture mediaCapture;
        private MediaCaptureInitializationSettings mediaCaptureSettings;
        private LowLagMediaRecording MediaRecording;
        #endregion capture microphone v1

        #region capture microphone v2
        //AudioGraph audioGraph;
        //AudioDeviceInputNode deviceInputNode;
        ////AudioDeviceOutputNode deviceOutputNode;
        //AudioFileOutputNode fileOutputNode;
        //private double _microAudioRecordedSeconds;
        //public double MicroAudioRecordedSeconds
        //{
        //    get => _microAudioRecordedSeconds;
        //    set { _microAudioRecordedSeconds = value; }
        //}
        #endregion capture microphone v2

        #region capture PC audio
        private WasapiLoopbackCapture PCAudioCapture = null;
        private WaveFileWriter PCAudioWriter = null;
        private WaveOutEvent SilenceWaveOut = null;
        private double _pCAudioRecordedSeconds;
        public double PCAudioRecordedSeconds
        {
            get => _pCAudioRecordedSeconds;
            set { _pCAudioRecordedSeconds = value; }
        }
        #endregion capture PC audio

        StorageFile filePCAudio;
        StorageFile fileMicroAudio;
        StorageFile fileVideo;

        HubConnection connection;

        public CapturePage()
        {
            InitializeComponent();

            //Task t1 = Task.Run(async () =>
            //{
            //    try
            //    {
            //        MMDevice defaultLoopbackCaptureDevice = WasapiLoopbackCapture.GetDefaultLoopbackCaptureDevice();
            //        MMDevice defaultCaptureDevice = WasapiLoopbackCapture.GetDefaultCaptureDevice();

            //        var l = new MMDeviceEnumerator();
            //        var lll1 = l.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);
            //        MMDevice? lll1Activ = lll1.FirstOrDefault(x => x.FriendlyName.Equals("Динамики (H310-1)"));
            //        MMDevice? lll1Activ1 = lll1.FirstOrDefault(x => x.FriendlyName.Equals("Громкоговорители / головные телефоны (IDT High Definition Audio CODEC)"));
            //        var lll2 = l.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All);
            //        MMDevice? lll2Activ = lll2.FirstOrDefault(x => x.FriendlyName.Equals("Микрофон (H310-1)"));


            //        var gg = defaultLoopbackCaptureDevice.AudioEndpointVolume.VolumeRange;
            //        var ss1 = defaultLoopbackCaptureDevice.AudioEndpointVolume.StepInformation;
            //        var ss2 = defaultLoopbackCaptureDevice.AudioEndpointVolume.MasterVolumeLevel;

            //        defaultLoopbackCaptureDevice.AudioEndpointVolume.VolumeStepUp();

            //        var ss11 = defaultLoopbackCaptureDevice.AudioEndpointVolume.StepInformation;
            //        var ss22 = defaultLoopbackCaptureDevice.AudioEndpointVolume.MasterVolumeLevel;

            //        defaultLoopbackCaptureDevice.AudioEndpointVolume.VolumeStepUp();
            //        defaultLoopbackCaptureDevice.AudioEndpointVolume.VolumeStepUp();
            //        defaultLoopbackCaptureDevice.AudioEndpointVolume.VolumeStepUp();

            //        var ss111 = defaultLoopbackCaptureDevice.AudioEndpointVolume.StepInformation;
            //        var ss2222 = defaultLoopbackCaptureDevice.AudioEndpointVolume.MasterVolumeLevel;

            //        PCAudioCapture = new WasapiLoopbackCapture(defaultLoopbackCaptureDevice);
            //        filePCAudio = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"pc.mp3", CreationCollisionOption.ReplaceExisting);
            //        PCAudioWriter = new WaveFileWriter(filePCAudio.Path, PCAudioCapture.WaveFormat);

            //        PCAudioCapture.DataAvailable += (s, a) =>
            //        {
            //            PCAudioWriter.Write(a.Buffer, 0, a.BytesRecorded);

            //            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            //            {
            //                PCAudioRecordedSeconds = PCAudioWriter.Position / PCAudioCapture.WaveFormat.AverageBytesPerSecond;
            //                PCRecordedSecondsStr.Text = $"pc: {TimeSpan.FromSeconds(PCAudioRecordedSeconds).ConvertToStr()}";
            //            });

            //        };

            //        PCAudioCapture.RecordingStopped += (s, a) =>
            //        {
            //            PCAudioWriter?.Dispose();
            //            PCAudioWriter = null;
            //            PCAudioCapture?.Dispose();
            //            PCAudioCapture = null;
            //        };

            //        var silence = new SilenceProvider(new WaveFormat(44100, 2)).ToSampleProvider();
            //        SilenceWaveOut = new WaveOutEvent();
            //        SilenceWaveOut.Init(silence);
            //        SilenceWaveOut.PlaybackStopped += (s, a) =>
            //        {
            //            SilenceWaveOut?.Dispose();
            //            SilenceWaveOut = null;
            //        };

            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //});
            //t1.Wait();

            //Task t2 = Task.Run(async () =>
            // {
            //     try
            //     {
            //         //fileMicroAudio = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"micro.mp3", CreationCollisionOption.GenerateUniqueName);

            //         //string a = MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default);
            //         //DeviceInformation a1 = await DeviceInformation.CreateFromIdAsync(a);

            //         //string defaultAudioCaptureId = MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Communications);
            //         //DeviceInformation defaultAudioCapture = await DeviceInformation.CreateFromIdAsync(defaultAudioCaptureId);

            //         //string audioCaptureSelector = MediaDevice.GetAudioCaptureSelector();
            //         //var audioCapture = await DeviceInformation.FindAllAsync(audioCaptureSelector);

            //         //AudioGraphSettings settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);
            //         //settings.PrimaryRenderDevice = a1;
            //         //CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
            //         //if (result.Status != AudioGraphCreationStatus.Success)
            //         //{
            //         //}
            //         //audioGraph = result.Graph;

            //         //audioGraph.QuantumProcessed += (AudioGraph a, object b) =>
            //         //{
            //         //    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            //         //    {
            //         //        MicroAudioRecordedSeconds = a.CompletedQuantumCount * 10;
            //         //        MicroRecordedSecondsStr.Text = $"micro: {TimeSpan.FromMilliseconds(a.CompletedQuantumCount * 10).ConvertToStr()}";
            //         //    });
            //         //};

            //         //audioGraph.QuantumStarted += (AudioGraph a, object b) =>
            //         //{
            //         //};

            //         //audioGraph.UnrecoverableErrorOccurred += (a, b) => { };


            //         //// Create a device output node
            //         //CreateAudioDeviceInputNodeResult result1 = await audioGraph.CreateDeviceInputNodeAsync(
            //         //    Windows.Media.Capture.MediaCategory.Media, audioGraph.EncodingProperties, defaultAudioCapture);
            //         //if (result1.Status != AudioDeviceNodeCreationStatus.Success)
            //         //{
            //         //}
            //         //deviceInputNode = result1.DeviceInputNode;

            //         //////  Create a device output node
            //         ////CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = await audioGraph.CreateDeviceOutputNodeAsync();
            //         ////if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            //         ////{
            //         ////    return;
            //         ////}
            //         ////deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;


            //         //// Operate node at the graph format, but save file at the specified format
            //         //var mediaEncodingProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
            //         //CreateAudioFileOutputNodeResult result2 = await audioGraph.CreateFileOutputNodeAsync(fileMicroAudio, mediaEncodingProfile);
            //         //if (result2.Status != AudioFileNodeCreationStatus.Success)
            //         //{
            //         //}
            //         //fileOutputNode = result2.FileOutputNode;

            //         //deviceInputNode.AddOutgoingConnection(fileOutputNode);
            //         ////deviceInputNode.AddOutgoingConnection(deviceOutputNode);
            //     }
            //     catch (Exception ex)
            //     {

            //     }
            // });
            //t2.Wait();

            //Task t3 = Task.Run(async () =>
            //{
            //    //MMDevice h = WasapiLoopbackCapture.GetDefaultCaptureDevice();

            //    //PCAudioCapture = new WasapiLoopbackCapture(h);
            //    //filePCAudio = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"test.mp3", CreationCollisionOption.ReplaceExisting);
            //    //PCAudioWriter = new WaveFileWriter(filePCAudio.Path, PCAudioCapture.WaveFormat);

            //    //PCAudioCapture.DataAvailable += (s, a) =>
            //    //{
            //    //    PCAudioWriter.Write(a.Buffer, 0, a.BytesRecorded);

            //    //    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            //    //    {
            //    //        PCAudioRecordedSeconds = PCAudioWriter.Position / PCAudioCapture.WaveFormat.AverageBytesPerSecond;
            //    //        RecordedSecondsStr.Text = TimeSpan.FromSeconds(PCAudioRecordedSeconds).ConvertToStr();
            //    //    });

            //    //};

            //    //PCAudioCapture.RecordingStopped += (s, a) =>
            //    //{
            //    //    PCAudioWriter?.Dispose();
            //    //    PCAudioWriter = null;
            //    //    PCAudioCapture?.Dispose();
            //    //    PCAudioCapture = null;
            //    //};

            //    //var silence = new SilenceProvider(new WaveFormat(44100, 2)).ToSampleProvider();
            //    //SilenceWaveOut = new WaveOutEvent();
            //    //SilenceWaveOut.Init(silence);
            //    //SilenceWaveOut.PlaybackStopped += (s, a) =>
            //    //{
            //    //    SilenceWaveOut?.Dispose();
            //    //    SilenceWaveOut = null;
            //    //};
            //});
            //t3.Wait();


            //SilenceWaveOut.Play();
            //PCAudioCapture.StartRecording();

            //Task t4 = Task.Run(async () =>
            //{
            //    await Task.Delay(TimeSpan.FromSeconds(18));

            //    //audioGraph.Stop();
            //    //audioGraph?.Dispose();
            //    //audioGraph = null;
            //    //deviceInputNode = null;
            //    //fileOutputNode = null;

            //    PCAudioCapture.StopRecording();
            //    SilenceWaveOut.Stop();
            //});

            Setup();
            SetupSignlR();
        }

        private void SetupSignlR()
        {
            try
            {
                connection = new HubConnectionBuilder()
                    .WithUrl("https://localhost:7139/video")
                    .Build();

                connection.Closed += async (error) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    await connection.StartAsync();
                };

                //try
                //{
                //    Task startSignalTask = Task.Run(async () => await connection.StartAsync());
                //    startSignalTask.Wait();
                //}
                //catch (Exception e)
                //{

                //}

                //Task sendSignalTask = Task.Run(async () =>
                //{
                //    try
                //    {
                //        while (true)
                //        {
                //            while (framesToSend.Count == 0) { }

                //            var bytes = framesToSend.Dequeue();

                //            SoftwareBitmap softwareBitmap = null;
                //            softwareBitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(bytes);
                //            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                //                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                //            float newWidth = softwareBitmap.PixelWidth / 5;
                //            float newHeight = softwareBitmap.PixelHeight / 5;
                //            using var resourceCreator = CanvasDevice.GetSharedDevice();
                //            using var canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(resourceCreator, softwareBitmap);
                //            using CanvasRenderTarget canvasRenderTarget = new(resourceCreator, newWidth, newHeight, canvasBitmap.Dpi);
                //            using var drawingSession = canvasRenderTarget.CreateDrawingSession();
                //            using ScaleEffect scaleEffect = new();

                //            scaleEffect.Source = canvasBitmap;
                //            scaleEffect.Scale = new System.Numerics.Vector2(newWidth / softwareBitmap.PixelWidth, newHeight / softwareBitmap.PixelHeight);
                //            drawingSession.DrawImage(scaleEffect);
                //            drawingSession.Flush();

                //            var pixels = canvasRenderTarget.GetPixelBytes();
                //            var c = pixels.Count(x => x != 0);

                //            if (pixels.Any())
                //                await connection.InvokeAsync("UploadStream", pixels);

                //            //var news = SoftwareBitmap.CreateCopyFromBuffer(canvasRenderTarget.GetPixelBytes().AsBuffer(), BitmapPixelFormat.Bgra8, (int)newWidth, (int)newHeight, BitmapAlphaMode.Premultiplied);

                //            softwareBitmap?.Dispose();
                //        }
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //});
            }
            catch (Exception e)
            {

            }
        }

        private void Setup()
        {
            _canvasDevice = new CanvasDevice();

            _compositor = App.MainWindow.Compositor;

            _compositionGraphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(
                _compositor,
                _canvasDevice);

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

        private async void Click_SelectGraphicsCapture(object sender, RoutedEventArgs e)
        {
            GraphicsCapturePicker picker = new();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            GraphicsCaptureItem captureItem = await picker.PickSingleItemAsync();

            if (captureItem != null)
            {
                StartCaptureInternal(captureItem);
            }
        }

        private async void StartCaptureInternal(GraphicsCaptureItem item)
        {
            // Stop the previous capture if we had one.
            StopCapture();

            _captureItem = item;
            _lastSize = _captureItem.Size;

            _framePool = Direct3D11CaptureFramePool.Create(
                _canvasDevice,
                Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                _captureItem.Size);

            _framePool.FrameArrived += (Direct3D11CaptureFramePool sender, object args) =>
            {
                using Direct3D11CaptureFrame frame = _framePool.TryGetNextFrame();
                ProcessFrame(frame);
            };

            _captureItem.Closed += (GraphicsCaptureItem sender, object args) =>
            {
                StopCapture();
            };

            _session = _framePool.CreateCaptureSession(_captureItem);
            _session.StartCapture();
        }

        private void ProcessFrame(Direct3D11CaptureFrame frame)
        {
            bool needsReset = false;
            bool recreateDevice = false;

            if ((frame.ContentSize.Width != _lastSize.Width) || (frame.ContentSize.Height != _lastSize.Height))
            {
                needsReset = true;
                _lastSize = frame.ContentSize;
            }

            try
            {
                _currentFrame = frame.Surface;

                CanvasBitmap canvasBitmap = CanvasBitmap.CreateFromDirect3D11Surface(_canvasDevice, frame.Surface);

                #region FillSurfaceWithBitmap

                CanvasComposition.Resize(_surface, canvasBitmap.Size);

                using var session = CanvasComposition.CreateDrawingSession(_surface);
                session.Clear(Colors.Transparent);
                session.DrawImage(canvasBitmap);

                #endregion FillSurfaceWithBitmap

                if (_isRecording)
                {
                    //framesToSend.Enqueue(frame.Surface);

                    framesToSave.Enqueue(new SurfaceWithInfo()
                    {
                        Surface = frame.Surface,
                        SystemRelativeTime = frame.SystemRelativeTime,
                    });
                }
            }
            catch (Exception e) when (_canvasDevice.IsDeviceLost(e.HResult))
            {
                needsReset = true;
                recreateDevice = true;
            }

            if (needsReset)
                ResetFramePool(frame.ContentSize, recreateDevice);
        }
        private void ResetFramePool(SizeInt32 size, bool recreateDevice)
        {
            do
            {
                try
                {
                    if (recreateDevice)
                    {
                        _canvasDevice = new CanvasDevice();
                    }

                    _framePool.Recreate(
                        _canvasDevice,
                        Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized,
                        2,
                        size);
                }
                // This is the device-lost convention for Win2D.
                catch (Exception e) when (_canvasDevice.IsDeviceLost(e.HResult))
                {
                    _canvasDevice = null;
                    recreateDevice = true;
                }
            } while (_canvasDevice == null);
        }

        private async void Click_StartCapture(object sender, RoutedEventArgs e)
        {
            if (!_isRecording)
            {
                var nowDate = $"{DateTime.Now:yyyyMMdd-HHmm-ss}";

                #region capture PC audio

                MMDevice defaultLoopbackCaptureDevice = WasapiLoopbackCapture.GetDefaultLoopbackCaptureDevice();
                MMDevice defaultCaptureDevice = WasapiLoopbackCapture.GetDefaultCaptureDevice();

                //var l = new MMDeviceEnumerator();
                //var lll1 = l.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);
                //MMDevice? lll1Activ = lll1.FirstOrDefault(x => x.FriendlyName.Equals("Динамики (H310-1)"));
                //MMDevice? lll1Activ1 = lll1.FirstOrDefault(x => x.FriendlyName.Equals("Громкоговорители / головные телефоны (IDT High Definition Audio CODEC)"));
                //var lll2 = l.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All);
                //MMDevice? lll2Activ = lll2.FirstOrDefault(x => x.FriendlyName.Equals("Микрофон (H310-1)"));

                PCAudioCapture = new WasapiLoopbackCapture(defaultLoopbackCaptureDevice);
                filePCAudio = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"{nowDate}_pc.mp3", CreationCollisionOption.GenerateUniqueName);
                PCAudioWriter = new WaveFileWriter(filePCAudio.Path, PCAudioCapture.WaveFormat);

                PCAudioCapture.DataAvailable += (s, a) =>
                {
                    PCAudioWriter.Write(a.Buffer, 0, a.BytesRecorded);
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        try
                        {
                            PCAudioRecordedSeconds = PCAudioWriter.Position / PCAudioCapture.WaveFormat.AverageBytesPerSecond;
                            PCRecordedSecondsStr.Text = $"pc: {TimeSpan.FromSeconds(PCAudioRecordedSeconds).ConvertToStr()}";
                        }
                        catch (Exception e) { }
                    });
                };

                PCAudioCapture.RecordingStopped += (s, a) =>
                {
                    PCAudioWriter?.Dispose();
                    PCAudioWriter = null;
                    PCAudioCapture?.Dispose();
                    PCAudioCapture = null;
                };

                var silence = new SilenceProvider(new WaveFormat(44100, 2)).ToSampleProvider();
                SilenceWaveOut = new WaveOutEvent();
                SilenceWaveOut.Init(silence);
                SilenceWaveOut.PlaybackStopped += (s, a) =>
                {
                    SilenceWaveOut?.Dispose();
                    SilenceWaveOut = null;
                };

                #endregion capture PC audio

                fileMicroAudio = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"{nowDate}_micro.mp3", CreationCollisionOption.GenerateUniqueName);

                #region capture microphone v1

                string defaultAudioCaptureId = MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Communications);
                DeviceInformation defaultAudioCapture = await DeviceInformation.CreateFromIdAsync(defaultAudioCaptureId);

                mediaCapture = new MediaCapture();
                mediaCaptureSettings = new MediaCaptureInitializationSettings
                {
                    AudioDeviceId = defaultAudioCapture.Id,
                    SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                    StreamingCaptureMode = StreamingCaptureMode.Audio,
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu
                };
                await mediaCapture.InitializeAsync(mediaCaptureSettings);

                mediaCapture.RecordLimitationExceeded += (MediaCapture sender) => { };
                mediaCapture.Failed += (MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs) => { };

                MediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(
                    MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High), fileMicroAudio);

                #endregion capture microphone v1

                #region capture microphone v2

                //string a = MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default);
                //DeviceInformation a1 = await DeviceInformation.CreateFromIdAsync(a);

                //string defaultAudioCaptureId = MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Communications);
                //DeviceInformation defaultAudioCapture = await DeviceInformation.CreateFromIdAsync(defaultAudioCaptureId);

                //AudioGraphSettings settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);
                //settings.PrimaryRenderDevice = a1;
                //CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
                //if (result.Status != AudioGraphCreationStatus.Success)
                //{ }
                //audioGraph = result.Graph;

                //audioGraph.QuantumProcessed += (AudioGraph a, object b) =>
                //{
                //    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                //    {
                //        try
                //        {
                //            MicroAudioRecordedSeconds = a.CompletedQuantumCount * 10;
                //            MicroRecordedSecondsStr.Text = $"micro: {TimeSpan.FromMilliseconds(a.CompletedQuantumCount * 10).ConvertToStr()}";
                //        }
                //        catch (Exception e) { }
                //    });
                //};
                //audioGraph.QuantumStarted += (AudioGraph a, object b) => { };
                //audioGraph.UnrecoverableErrorOccurred += (a, b) => { };

                //// Create a device output node
                //CreateAudioDeviceInputNodeResult result1 = await audioGraph.CreateDeviceInputNodeAsync(
                //    Windows.Media.Capture.MediaCategory.Media, audioGraph.EncodingProperties, defaultAudioCapture);
                //if (result1.Status != AudioDeviceNodeCreationStatus.Success)
                //{ }
                //deviceInputNode = result1.DeviceInputNode;

                //// Operate node at the graph format, but save file at the specified format
                //var mediaEncodingProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
                //CreateAudioFileOutputNodeResult result2 = await audioGraph.CreateFileOutputNodeAsync(fileMicroAudio, mediaEncodingProfile);
                //if (result2.Status != AudioFileNodeCreationStatus.Success)
                //{ }
                //fileOutputNode = result2.FileOutputNode;

                //deviceInputNode.AddOutgoingConnection(fileOutputNode);

                #endregion capture microphone v2

                #region
                var width = _captureItem.Size.Width;
                var height = _captureItem.Size.Height;

                VideoEncodingProperties videoProps = VideoEncodingProperties.CreateUncompressed(MediaEncodingSubtypes.Bgra8, (uint)width, (uint)height);
                VideoStreamDescriptor videoDescriptor = new(videoProps);

                MediaStreamSource streamSource = new(videoDescriptor);
                streamSource.BufferTime = TimeSpan.FromSeconds(0);
                streamSource.SampleRequested += StreamSource_SampleRequested;
                streamSource.Closed += (MediaStreamSource sender, MediaStreamSourceClosedEventArgs args) => { };
                streamSource.Starting += (MediaStreamSource sender, MediaStreamSourceStartingEventArgs args) =>
                {
                    while (framesToSave.Count == 0) { }
                    var videoFrame = framesToSave.Dequeue();
                    args.Request.SetActualStartPosition(videoFrame.SystemRelativeTime);
                    StartRecoedVide = videoFrame.SystemRelativeTime;
                };
                streamSource.Paused += (MediaStreamSource sender, object args) => { };
                streamSource.SwitchStreamsRequested += (MediaStreamSource sender, MediaStreamSourceSwitchStreamsRequestedEventArgs args) => { };
                #endregion

                #region
                MediaEncodingProfile encodingProfile = new();
                encodingProfile.Container.Subtype = "MPEG4";
                encodingProfile.Video.Subtype = "H264";
                encodingProfile.Video.Width = 1920;
                encodingProfile.Video.Height = 1080;
                encodingProfile.Video.Bitrate = 18000000;
                encodingProfile.Video.FrameRate.Numerator = 30;
                encodingProfile.Video.FrameRate.Denominator = 1;
                encodingProfile.Video.PixelAspectRatio.Numerator = 1;
                encodingProfile.Video.PixelAspectRatio.Denominator = 1;
                #endregion

                #region
                var tempFolder = ApplicationData.Current.LocalCacheFolder;
                fileVideo = await tempFolder.CreateFileAsync($"{nowDate}_video_capture.mp4", CreationCollisionOption.ReplaceExisting);
                var outputStream = await fileVideo.OpenAsync(FileAccessMode.ReadWrite);
                #endregion

                #region start capture
                try
                {
                    MediaTranscoder transcoder = new();
                    //transcoder.HardwareAccelerationEnabled = true;

                    var transcode = await transcoder.PrepareMediaStreamSourceTranscodeAsync(streamSource, outputStream, encodingProfile);
                    if (!transcode.CanTranscode)
                        throw new Exception($"transcode can not transcode");

                    // start streamSource
                    //await Task.Delay((int)TimeSpan.FromSeconds(5).TotalMilliseconds);
                    var op = transcode.TranscodeAsync();
                    //op.Progress += new AsyncActionProgressHandler<double>(TranscodeProgress);
                    //op.Completed += new AsyncActionWithProgressCompletedHandler<double>(TranscodeComplete);

                    #region capture microphone v1
                    await MediaRecording.StartAsync();
                    #endregion capture microphone v1

                    #region capture microphone v2
                    //audioGraph.Start();
                    #endregion capture microphone v2

                    #region capture PC audio
                    SilenceWaveOut.Play();
                    PCAudioCapture.StartRecording();
                    #endregion capture PC audio

                    _isRecording = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex}");
                    throw;
                }
                #endregion start capture

            }
        }
        private void StreamSource_SampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            try
            {
                //if (_isRecording)
                //{
                //    while (frames.Count == 0) { }
                //    lock (frames)
                //    {
                //        var videoFrame = frames.Dequeue();
                //        var samp = MediaStreamSample.CreateFromDirect3D11Surface(videoFrame.Surface, videoFrame.SystemRelativeTime);
                //        args.Request.Sample = samp;
                //    }
                //}
                //else
                //{
                //    args.Request.Sample = null;
                //    StopCapture();
                //}


                while (framesToSave.Count == 0) { }
                //if (frames.Count == 0)
                //{
                //    Thread.Sleep(TimeSpan.FromMilliseconds(50));
                //    StreamSource_SampleRequested(sender, args);
                //}
                SurfaceWithInfo videoFrame = framesToSave.Dequeue();

                if (videoFrame == null)
                {
                    PCAudioCapture?.StopRecording();
                    args.Request.Sample = null;
                    return;
                }

                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        var time = videoFrame.SystemRelativeTime - StartRecoedVide;
                        VideoRecordedSeconds = time.TotalSeconds;
                        VideoRecordedSecondsStr.Text = $"video: {time.ConvertToStr()}";
                    }
                    catch (Exception e) { }
                });

                var samp = MediaStreamSample.CreateFromDirect3D11Surface(videoFrame.Surface, videoFrame.SystemRelativeTime);

                samp.Processed += (MediaStreamSample sender, object args) => { };
                args.Request.Sample = samp;
            }
            catch (Exception ex)
            {
                PCAudioCapture?.StopRecording();
                args.Request.Sample = null;
                return;
            }
        }

        private async void Click_StopCapture(object sender, RoutedEventArgs e)
        {
            #region capture microphone v1
            if (MediaRecording != null)
                await MediaRecording.FinishAsync();
            #endregion capture microphone v1

            #region capture microphone v2
            //audioGraph.Stop();
            //audioGraph?.Dispose();
            //audioGraph = null;
            //deviceInputNode = null;
            //fileOutputNode = null;
            #endregion capture microphone v2

            #region capture PC audio
            PCAudioCapture.StopRecording();
            SilenceWaveOut.Stop();
            #endregion capture PC audio

            _isRecording = false;
            framesToSave.Enqueue(null);

            StopCapture();

            await Task.Delay(TimeSpan.FromSeconds(5));
            await SaveToUnionFile();
        }

        private async void Click_TakeScreenShot(object sender, RoutedEventArgs e)
        {
            await SaveImageAsync();
        }
        private async Task SaveImageAsync()
        {
            if (_currentFrame == null)
                return;

            CanvasBitmap canvasBitmap = CanvasBitmap.CreateFromDirect3D11Surface(_canvasDevice, _currentFrame);

            StorageFile file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("screenShot.png", CreationCollisionOption.GenerateUniqueName);
            using var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            await canvasBitmap.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);
        }

        private async Task SaveToUnionFile()
        {
            //filePCAudio = await StorageFile.GetFileFromPathAsync("C:\\Users\\prode\\AppData\\Local\\Packages\\e3ce9e70-4227-4950-abfe-78557804a917_5q3yfc64hm9aa\\LocalCache\\20230206-1600-05_pc.mp3");
            //fileVideo = await StorageFile.GetFileFromPathAsync("C:\\Users\\prode\\AppData\\Local\\Packages\\e3ce9e70-4227-4950-abfe-78557804a917_5q3yfc64hm9aa\\LocalCache\\20230206-1600-05_video_capture.mp4");
            //fileMicroAudio = await StorageFile.GetFileFromPathAsync("C:\\Users\\prode\\AppData\\Local\\Packages\\e3ce9e70-4227-4950-abfe-78557804a917_5q3yfc64hm9aa\\LocalCache\\20230206-1600-05_micro.mp3");

            if (filePCAudio != null && fileVideo != null && fileMicroAudio != null)
            {
                var fileUnionName = $"{DateTime.Now:yyyyMMdd-HHmm-ss}_unioun.mp4";
                var fileUnion = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(fileUnionName, CreationCollisionOption.GenerateUniqueName);

                MediaComposition muxedStream = new();

                BackgroundAudioTrack pcAudioTrack = await BackgroundAudioTrack.CreateFromFileAsync(filePCAudio);
                BackgroundAudioTrack microAudioTrack = await BackgroundAudioTrack.CreateFromFileAsync(fileMicroAudio);
                MediaClip videoTrack = await MediaClip.CreateFromFileAsync(fileVideo);

                muxedStream.BackgroundAudioTracks.Add(pcAudioTrack);
                muxedStream.BackgroundAudioTracks.Add(microAudioTrack);
                muxedStream.Clips.Add(videoTrack);

                var r = await muxedStream.RenderToFileAsync(fileUnion, MediaTrimmingPreference.Precise);

                //MediaStreamSource mss = muxedStream.GenerateMediaStreamSource();
                //mpElement.Source = MediaSource.CreateFromMediaStreamSource(mss);
            }
        }

        public void StopCapture()
        {
            //if (_captureItem != null)
            //    _captureItem.Closed -= OnCaptureItemClosed;
            _captureItem = null;

            //_canvasDevice.Dispose();
            //_canvasDevice = null;

            _framePool?.Dispose();
            _framePool = null;

            _session?.Dispose();
            _session = null;
        }

        private async void OpenCacheClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalCacheFolder);
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