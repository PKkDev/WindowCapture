using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WindowCapture.WinApp.MVVM.Model;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.System;

namespace WindowCapture.WinApp.MVVM.ViewModel
{
    public class MediaFolderViewModel : ObservableRecipient
    {
        private StorageFolder _currentFolder;
        public StorageFolder CurrentFolder
        {
            get => _currentFolder;
            set => SetProperty(ref _currentFolder, value);
        }

        private bool _isFolderScaning;
        public bool IsFolderScaning
        {
            get => _isFolderScaning;
            set => SetProperty(ref _isFolderScaning, value);
        }

        public RelayCommand OpenCurrentFolder { get; set; }

        private readonly List<string> _acceptedExtenson = new() { ".mp4", ".mp3" };

        public ObservableCollection<MediaFileDetail> ViewFiles { get; set; }

        public MediaFileDetail SelectedMediaFileDetail { get; set; } = null;

        public StandardUICommand DeleteCommand { get; set; }
        public StandardUICommand OpenCommand { get; set; }

        public MediaFolderViewModel()
        {

            DeleteCommand = new StandardUICommand(StandardUICommandKind.Delete);
            DeleteCommand.ExecuteRequested += async (XamlUICommand sender, ExecuteRequestedEventArgs args) =>
            {
                if (args.Parameter is string displayName)
                {
                    var search = ViewFiles.FirstOrDefault(x => x.DisplayName.Equals(displayName));
                    if (search != null)
                    {
                        await search.File.DeleteAsync(StorageDeleteOption.Default);
                        await LoadFilesInFolder();
                    }
                }
            };
            OpenCommand = new StandardUICommand(StandardUICommandKind.Open);
            OpenCommand.ExecuteRequested += async (XamlUICommand sender, ExecuteRequestedEventArgs args) =>
            {
                if (args.Parameter is string displayName)
                {
                    var search = ViewFiles.FirstOrDefault(x => x.DisplayName.Equals(displayName));
                    if (search != null)
                        System.Diagnostics.Process.Start("CMD.exe", $"/C {search.File.Path}");
                }
            };

            ViewFiles = new();
            CurrentFolder = ApplicationData.Current.LocalCacheFolder;
            OpenCurrentFolder = new RelayCommand(async () =>
            {
                if (CurrentFolder != null)
                    await Launcher.LaunchFolderAsync(CurrentFolder);
            });
            Task t = Task.Run(async () => { await LoadFilesInFolder(); });
            t.Wait();
        }

        private async Task LoadFilesInFolder()
        {
            ViewFiles.Clear();
            IsFolderScaning = true;
            if (CurrentFolder == null)
            {
                IsFolderScaning = false;
                return;
            }
            CommonFileQuery query = CommonFileQuery.DefaultQuery;
            var files = await CurrentFolder.GetFilesAsync(query);

            foreach (var file in files)
                if (_acceptedExtenson.Contains(file.FileType))
                    ViewFiles.Add(new(file, DeleteCommand, OpenCommand));

            IsFolderScaning = false;
        }

        public async void ViewFilesClick(MediaFileDetail fileDetail)
        {
            SelectedMediaFileDetail = fileDetail;

            MediaClip videoTrack = await MediaClip.CreateFromFileAsync(SelectedMediaFileDetail.File);
            var s = MediaSource.CreateFromStorageFile(SelectedMediaFileDetail.File);

            var mediaPlayer = new MediaPlayer();
            mediaPlayer.Source = MediaSource.CreateFromStorageFile(SelectedMediaFileDetail.File);

            var mediaCapture = new MediaCapture();
            var mediaCaptureSettings = new MediaCaptureInitializationSettings
            {
                //VideoSource = s.So,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                StreamingCaptureMode = StreamingCaptureMode.Audio,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu
            };
            await mediaCapture.InitializeAsync(mediaCaptureSettings);

            System.Diagnostics.Process.Start("CMD.exe", $"/C {fileDetail.File.Path}");
        }
    }
}
