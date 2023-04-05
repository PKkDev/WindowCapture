﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WindowCapture.WinApp.Dilogs.CaptureItemSelect.Tabs;
using WindowCapture.WinApp.Extensios;
using WindowCapture.WinApp.MVVM.Model;
using Windows.Storage;
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

        private Frame _detailFrame;
        public void SetDetailFrame(Frame detailFrame)
        {
            _detailFrame = detailFrame;
        }

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
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        var icon = await file.GetFileIcon();
                        ViewFiles.Add(new(file, icon, DeleteCommand, OpenCommand));
                    });
                }

            IsFolderScaning = false;
        }

        public async void ViewFilesClick(MediaFileDetail fileDetail)
        {
            SelectedMediaFileDetail = fileDetail;

            switch (fileDetail.FileType)
            {
                case "mp3":
                    {
                        _detailFrame.Navigate(typeof(WindowCaptureItemPage), null, new DrillInNavigationTransitionInfo());
                        break;
                    }
                case "mp4":
                    {
                        _detailFrame.Navigate(typeof(WindowCaptureItemPage), null, new DrillInNavigationTransitionInfo());
                        break;
                    }
            }

            //using MemoryStream ras = new MemoryStream();
            //var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            //ffMpeg.GetVideoThumbnail(fileDetail.File.Path, ras, 1);
            //var bmp2 = Bitmap.FromStream(ras);
            //bmp2.Save("D:\\work\\yesy2.png");

            //System.Diagnostics.Process.Start("CMD.exe", $"/C {fileDetail.File.Path}");
        }

    }
}
