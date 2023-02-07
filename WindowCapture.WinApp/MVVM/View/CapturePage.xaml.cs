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
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.Devices;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Devices.PointOfService;
using Windows.Foundation;
// NAudio
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
// SignalR
using Microsoft.AspNetCore.SignalR.Client;
using NAudio.CoreAudioApi;
using System.Linq;
using System.Threading;
using static CommunityToolkit.Mvvm.ComponentModel.__Internals.__TaskExtensions.TaskAwaitableWithoutEndValidation;
using System.Text.RegularExpressions;

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
        // Capture API objects.
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

        #region
        public Queue<SurfaceWithInfo> framesToSave = new();
        public Queue<IDirect3DSurface> framesToSend = new();
        public bool _isRecording = false;
        #endregion

        private MediaCapture mediaCapture;
        private MediaCaptureInitializationSettings mediaCaptureSettings;
        private LowLagMediaRecording MediaRecording;

        private WasapiLoopbackCapture Capture = null;
        private WaveFileWriter Writer = null;
        private WaveOutEvent SilenceWaveOut = null;
        private double RecordedSeconds = 0;

        StorageFile filePCAudio;
        StorageFile fileMicroAudio;
        StorageFile fileVideo;


        HubConnection connection;

        public CapturePage()
        {
            InitializeComponent();

            Task t1 = Task.Run(async () =>
            {
                try
                {
                    MMDevice g = WasapiLoopbackCapture.GetDefaultLoopbackCaptureDevice();
                    MMDevice h = WasapiLoopbackCapture.GetDefaultCaptureDevice();
                    //WasapiLoopbackCapture J = new WasapiLoopbackCapture();

                    var l = new MMDeviceEnumerator();
                    var lll1 = l.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);
                    MMDevice? lll1Activ = lll1.FirstOrDefault(x => x.FriendlyName.Equals("Динамики (H310-1)"));
                    MMDevice? lll1Activ1 = lll1.FirstOrDefault(x => x.FriendlyName.Equals("Громкоговорители / головные телефоны (IDT High Definition Audio CODEC)"));
                    var lll2 = l.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All);
                    MMDevice? lll2Activ = lll2.FirstOrDefault(x => x.FriendlyName.Equals("Микрофон (H310-1)"));

                    Capture = new WasapiLoopbackCapture(lll1Activ);
                    filePCAudio = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"test.mp3", CreationCollisionOption.ReplaceExisting);
                    Writer = new WaveFileWriter(filePCAudio.Path, Capture.WaveFormat);

                    Capture.DataAvailable += (s, a) =>
                    {
                        Writer.Write(a.Buffer, 0, a.BytesRecorded);
                        RecordedSeconds = Writer.Position / Capture.WaveFormat.AverageBytesPerSecond;
                    };

                    Capture.RecordingStopped += (s, a) =>
                    {
                        Writer?.Dispose();
                        Writer = null;
                        Capture?.Dispose();
                        Capture = null;
                    };

                    var silence = new SilenceProvider(new WaveFormat(44100, 2)).ToSampleProvider();
                    SilenceWaveOut = new WaveOutEvent();
                    SilenceWaveOut.Init(silence);
                    SilenceWaveOut.Play();
                    SilenceWaveOut.PlaybackStopped += (s, a) =>
                    {
                        SilenceWaveOut?.Dispose();
                        SilenceWaveOut = null;
                    };

                }
                catch (Exception ex) { }

                Capture.StartRecording();

            });
            t1.Wait();

            Task t2 = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(15));
                Capture.StopRecording();
                SilenceWaveOut.Stop();
            });

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

                filePCAudio = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"{nowDate}_pc.mp3", CreationCollisionOption.GenerateUniqueName);
                Capture = new WasapiLoopbackCapture();
                Writer = new WaveFileWriter(filePCAudio.Path, Capture.WaveFormat);

                Capture.DataAvailable += (s, a) =>
                {
                    Writer.Write(a.Buffer, 0, a.BytesRecorded);
                };

                Capture.RecordingStopped += (s, a) =>
                {
                    Writer?.Dispose();
                    Writer = null;
                    Capture?.Dispose();
                    Capture = null;
                };

                #endregion capture PC audio

                #region capture microphone

                fileMicroAudio = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"{nowDate}_micro.mp3", CreationCollisionOption.GenerateUniqueName);

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
                mediaCapture.Failed += (MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs) =>
                {
                };

                MediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(
                    MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High), fileMicroAudio);

                #endregion capture microphone

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

                #region
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

                    var sine20Seconds = new SignalGenerator() { Gain = 0.2, Frequency = 500, Type = SignalGeneratorType.Sin }
                    .Take(TimeSpan.FromSeconds(2));
                    using var wo = new WaveOutEvent();
                    wo.Init(sine20Seconds);
                    wo.Play();

                    Capture.StartRecording();

                    await MediaRecording.StartAsync();

                    _isRecording = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex}");
                    throw;
                }
                #endregion

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
                    Capture?.StopRecording();
                    args.Request.Sample = null;
                    return;
                }

                var samp = MediaStreamSample.CreateFromDirect3D11Surface(videoFrame.Surface, videoFrame.SystemRelativeTime);

                samp.Processed += (MediaStreamSample sender, object args) => { };
                args.Request.Sample = samp;
            }
            catch (Exception ex)
            {
                Capture?.StopRecording();
                args.Request.Sample = null;
                return;
            }
        }

        private async void Click_StopCapture(object sender, RoutedEventArgs e)
        {
            if (MediaRecording != null)
                await MediaRecording.FinishAsync();

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

                MediaComposition muxedStream = new MediaComposition();

                BackgroundAudioTrack pcAudioTrack = await BackgroundAudioTrack.CreateFromFileAsync(filePCAudio);
                BackgroundAudioTrack microAudioTrack = await BackgroundAudioTrack.CreateFromFileAsync(fileMicroAudio);
                MediaClip videoTrack = await MediaClip.CreateFromFileAsync(fileVideo);

                muxedStream.BackgroundAudioTracks.Add(pcAudioTrack);
                muxedStream.BackgroundAudioTracks.Add(microAudioTrack);
                muxedStream.Clips.Add(videoTrack);

                await muxedStream.RenderToFileAsync(fileUnion, MediaTrimmingPreference.Precise);

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