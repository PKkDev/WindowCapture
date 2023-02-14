using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WindowCapture.WinApp.MVVM.Model;
using WindowCapture.WinApp.MVVM.View;
using Windows.Storage;
using Windows.Storage.Search;

namespace WindowCapture.WinApp.MVVM.ViewModel
{
    public class MediaFolderViewModel : ObservableRecipient
    {
        private StorageFolder CurrentFolder { get; set; } = ApplicationData.Current.LocalCacheFolder;
        private readonly List<string> _acceptedExtenson = new() { ".mp4", ".mp3" };

        public ObservableCollection<MediaFileDetail> ViewFiles { get; set; }

        public MediaFileDetail SelectedMediaFileDetail { get; set; } = null;

        public MediaFolderViewModel()
        {
            ViewFiles = new();
            Task t = Task.Run(async () => { await LoadFilesInFolder(); });
            t.Wait();
        }

        private async Task LoadFilesInFolder()
        {
            if (CurrentFolder == null) { return; }
            CommonFileQuery query = CommonFileQuery.DefaultQuery;
            var files = await CurrentFolder.GetFilesAsync(query);

            foreach (var file in files)
                if (_acceptedExtenson.Contains(file.FileType))
                    ViewFiles.Add(new(file));
        }

        public void ViewFilesClick(MediaFileDetail fileDetail)
        {
            SelectedMediaFileDetail = fileDetail;

            System.Diagnostics.Process.Start("CMD.exe", $"/C {fileDetail.File.Path}");

            //System.Diagnostics.Process process = new System.Diagnostics.Process();
            //System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //startInfo.FileName = "cmd.exe";
            //startInfo.Arguments = "/C {fileDetail.File.Path}";
            //process.StartInfo = startInfo;
            //process.Start();

            //var newWindow = new MainWindow();
            //var shell = App.GetService<ShellPage>();
            //shell.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Default;
            //newWindow.Content = shell;
            //newWindow.Activate();

        }
    }
}
