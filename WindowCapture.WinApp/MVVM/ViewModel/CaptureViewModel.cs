using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Graphics.Canvas;
using System.Collections.ObjectModel;
using System.Linq;
using WindowCapture.WinApp.MVVM.Model;
using Windows.Devices.PointOfService;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.Input;
using CaptureHelper.Model;
using CaptureHelper;
using System;
using WindowCapture.WinApp.Dilogs.CaptureItemSelect;
using Windows.Graphics.Capture;
using Windows.Graphics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Media.Devices;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Core;
using Windows.Media.Transcoding;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Media.Editing;
using Windows.Graphics.DirectX.Direct3D11;
// NAudio
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace WindowCapture.WinApp.MVVM.ViewModel
{
    public class CaptureViewModel : ObservableRecipient
    {
        // WidthxHeight
        public ObservableCollection<ResolutionItem> Resolutions { get; set; }
        private ResolutionItem _selectedResolution;
        public ResolutionItem SelectedResolution
        {
            get => _selectedResolution;
            set => SetProperty(ref _selectedResolution, value);
        }

        // N/1000000 Mbps
        // var temp = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
        // var bitrate = temp.Video.Bitrate;
        public ObservableCollection<uint> Bitrates { get; set; }
        private uint _selectedBitrate;
        public uint SelectedBitrate
        {
            get => _selectedBitrate;
            set => SetProperty(ref _selectedBitrate, value);
        }

        // N fps
        public ObservableCollection<uint> FrameRates { get; set; }
        private uint _selectedFrameRate;
        public uint SelectedFrameRate
        {
            get => _selectedFrameRate;
            set => SetProperty(ref _selectedFrameRate, value);
        }

        #region IsCaptureMicro
        public ObservableCollection<MicrophoneInfo> MicrophoneInfos { get; set; }
        private MicrophoneInfo _selectedMicrophone;
        public MicrophoneInfo SelectedMicrophone
        {
            get => _selectedMicrophone;
            set => SetProperty(ref _selectedMicrophone, value);
        }
        private bool _isCaptureMicro;
        public bool IsCaptureMicro
        {
            get => _isCaptureMicro;
            set
            {
                SetProperty(ref _isCaptureMicro, value);
                App.MainWindow.DispatcherQueue.TryEnqueue(async () => await LoadMicrophoneInfos());
            }
        }
        #endregion IsCaptureMicro

        private bool _isRendring;
        public bool IsRendring
        {
            get => _isRendring;
            set => SetProperty(ref _isRendring, value);
        }

        private bool _isRecording;
        public bool IsRecording
        {
            get => _isRecording;
            set => SetProperty(ref _isRecording, value);
        }

        private bool _isCapturePCAudio;
        public bool IsCapturePCAudio
        {
            get => _isCapturePCAudio;
            set => SetProperty(ref _isCapturePCAudio, value);
        }

        // PCAudioCapture API objects.
        private SizeInt32 _lastSize;
        private GraphicsCaptureItem _captureItem;
        private Direct3D11CaptureFramePool _framePool;
        private GraphicsCaptureSession _session;

        // Non-API related members.
        public CanvasDevice _canvasDevice;
        private IDirect3DSurface _currentFrame;

        public RelayCommand SelectGraphicsCaptureCmdV1 { get; set; }
        public RelayCommand SelectGraphicsCaptureCmdV2 { get; set; }

        public RelayCommand StartCaptureCmd { get; set; }
        public RelayCommand StopCaptureCmd { get; set; }
        public RelayCommand TakeScreenShotCmd { get; set; }

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

        public TimeSpan StartRecoedVide;

        #region capture microphone v1
        private MediaCapture mediaCapture;
        private MediaCaptureInitializationSettings mediaCaptureSettings;
        private LowLagMediaRecording MediaRecording;
        #endregion capture microphone v1

        StorageFile filePCAudio;
        StorageFile fileMicroAudio;
        StorageFile fileVideo;

        private Queue<SurfaceWithInfo> framesToSave = new();

        public event EventHandler<CanvasBitmap> FillSurfaceWithBitmap;

        public CaptureViewModel()
        {
            MicrophoneInfos = new();
            Resolutions = new()
            {
                new ResolutionItem(new SizeUInt32() { Width = 640, Height = 480 }) ,
                new ResolutionItem(new SizeUInt32() { Width = 1280, Height = 720 }) ,
                new ResolutionItem(new SizeUInt32() { Width = 1920, Height = 1080 }) ,
                new ResolutionItem(new SizeUInt32() { Width = 3840, Height = 2160 }),
                new ResolutionItem(new SizeUInt32() { Width = 7680, Height = 4320 })
            };
            Bitrates = new() { 1125000, 2250000, 4500000, 9000000, 18000000, 36000000, 72000000 };
            FrameRates = new() { 24, 30, 60 };

            SelectedResolution = Resolutions.First(x => x.Size.Width == 1280 && x.Size.Height == 720);
            SelectedBitrate = 4500000;
            SelectedFrameRate = 24;

            _canvasDevice = new CanvasDevice();

            SelectGraphicsCaptureCmdV1 = new RelayCommand(async () =>
            {
                GraphicsCapturePicker picker = new();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                GraphicsCaptureItem captureItem = await picker.PickSingleItemAsync();

                if (captureItem != null)
                    StartCaptureInternal(captureItem);
            });

            SelectGraphicsCaptureCmdV2 = new RelayCommand(async () =>
            {
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

                ContentDialog dialog = new()
                {
                    XamlRoot = App.MainWindow.Content.XamlRoot,
                    // Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                    Title = resourceLoader.GetString("CapureItemSelectorWindowTitle"),
                    PrimaryButtonText = resourceLoader.GetString("CapureItemSelectorWindowPrimBtnTxt"),
                    CloseButtonText = resourceLoader.GetString("CapureItemSelectorWindowCloseBtnTxt"),
                    DefaultButton = ContentDialogButton.Close,
                    Content = new CapureItemSelectorPage()
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary && App.CaptureItemSelected != null)
                {
                    var item = App.CaptureItemSelected.Type == CaptureItemSelectedType.Monitor
                        ? CaptureCreateHelper.CreateItemForMonitor(App.CaptureItemSelected.Handler)
                        : CaptureCreateHelper.CreateItemForWindow(App.CaptureItemSelected.Handler);

                    StartCaptureInternal(item);
                }
                else
                    App.CaptureItemSelected = null;
            });

            StartCaptureCmd = new RelayCommand(async () =>
            {
                if (!IsRecording && _captureItem != null)
                {
                    var nowDate = $"{DateTime.Now:yyyyMMdd-HHmm-ss}";

                    if (IsCapturePCAudio)
                    {
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
                                    //PCRecordedSecondsStr.Text = $"pc: {TimeSpan.FromSeconds(PCAudioRecordedSeconds).ConvertToStr()}";
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
                    }

                    if (IsCaptureMicro)
                    {
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
                    }

                    #region MediaStreamSource

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

                    #endregion MediaStreamSource

                    #region MediaEncodingProfile

                    MediaEncodingProfile encodingProfile = new();
                    encodingProfile.Container.Subtype = "MPEG4";
                    encodingProfile.Video.Subtype = "H264";
                    encodingProfile.Video.Width = SelectedResolution.Size.Width;// 1920;
                    encodingProfile.Video.Height = SelectedResolution.Size.Height;// 1080;
                    encodingProfile.Video.Bitrate = SelectedBitrate; //18000000;
                    encodingProfile.Video.FrameRate.Numerator = SelectedFrameRate; // 30;
                    encodingProfile.Video.FrameRate.Denominator = 1;
                    encodingProfile.Video.PixelAspectRatio.Numerator = 1;
                    encodingProfile.Video.PixelAspectRatio.Denominator = 1;

                    #endregion MediaEncodingProfile

                    #region Video stream

                    fileVideo = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"{nowDate}_video_capture.mp4", CreationCollisionOption.ReplaceExisting);
                    var outputStream = await fileVideo.OpenAsync(FileAccessMode.ReadWrite);

                    #endregion Video stream

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

                        if (IsCaptureMicro)
                        {
                            #region capture microphone v1

                            await MediaRecording.StartAsync();

                            #endregion capture microphone v1

                            #region capture microphone v2

                            //audioGraph.Start();

                            #endregion capture microphone v2
                        }

                        if (IsCapturePCAudio)
                        {
                            #region capture PC audio

                            SilenceWaveOut.Play();
                            PCAudioCapture.StartRecording();

                            #endregion capture PC audio
                        }

                        IsRecording = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex}");
                        throw;
                    }

                    #endregion start capture

                }
            });

            StopCaptureCmd = new RelayCommand(async () =>
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
                PCAudioCapture?.StopRecording();
                SilenceWaveOut?.Stop();
                #endregion capture PC audio

                IsRecording = false;
                framesToSave.Enqueue(null);

                StopCapture();

                await RenderToUnionFile();
            });

            TakeScreenShotCmd = new RelayCommand(async () =>
            {
                if (_currentFrame == null)
                    return;

                CanvasBitmap canvasBitmap = CanvasBitmap.CreateFromDirect3D11Surface(_canvasDevice, _currentFrame);

                StorageFile file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("screenShot.png", CreationCollisionOption.GenerateUniqueName);
                using var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                await canvasBitmap.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);
            });
        }

        private void StartCaptureInternal(GraphicsCaptureItem item)
        {
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

        private void StopCapture()
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

                #region FillSurfaceWithBitmap
                CanvasBitmap canvasBitmap = CanvasBitmap.CreateFromDirect3D11Surface(_canvasDevice, frame.Surface);
                FillSurfaceWithBitmap.Invoke(this, canvasBitmap);
                #endregion FillSurfaceWithBitmap

                if (IsRecording)
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

        private void StreamSource_SampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            try
            {
                while (framesToSave.Count == 0) { }

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
                        //VideoRecordedSeconds = time.TotalSeconds;
                        //VideoRecordedSecondsStr.Text = $"video: {time.ConvertToStr()}";
                    }
                    catch (Exception ex) { }
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

        private async Task RenderToUnionFile()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            IsRendring = true;

            if (filePCAudio == null && fileVideo == null && fileMicroAudio == null)
                return;

            var fileUnionName = $"{DateTime.Now:yyyyMMdd-HHmm-ss}_unioun.mp4";
            var fileUnion = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(fileUnionName, CreationCollisionOption.GenerateUniqueName);

            MediaComposition muxedStream = new();

            if (filePCAudio != null)
            {
                BackgroundAudioTrack pcAudioTrack = await BackgroundAudioTrack.CreateFromFileAsync(filePCAudio);
                muxedStream.BackgroundAudioTracks.Add(pcAudioTrack);
            }

            if (fileVideo != null)
            {
                MediaClip videoTrack = await MediaClip.CreateFromFileAsync(fileVideo);
                muxedStream.Clips.Add(videoTrack);
            }

            if (fileMicroAudio != null)
            {
                BackgroundAudioTrack microAudioTrack = await BackgroundAudioTrack.CreateFromFileAsync(fileMicroAudio);
                muxedStream.BackgroundAudioTracks.Add(microAudioTrack);
            }

            var result = await muxedStream.RenderToFileAsync(fileUnion, MediaTrimmingPreference.Precise);

            IsRendring = false;

            new ToastContentBuilder()
                .AddText("rendered file is ready")
                .Show();
        }

        private async Task LoadMicrophoneInfos()
        {
            MicrophoneInfos.Clear();

            if (IsCaptureMicro)
            {
                string defaultId = MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Communications);

                string audioCaptureSelector = MediaDevice.GetAudioCaptureSelector();
                var audioCapture = await DeviceInformation.FindAllAsync(audioCaptureSelector);

                foreach (var item in audioCapture)
                    MicrophoneInfos.Add(new(item, item.Id.Equals(defaultId)));
            }
            else
                fileMicroAudio = null;
        }

    }
}
