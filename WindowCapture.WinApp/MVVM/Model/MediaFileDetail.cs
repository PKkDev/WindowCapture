using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Windows.Input;
using Windows.Storage;

namespace WindowCapture.WinApp.MVVM.Model
{
    public class MediaFileDetail
    {
        public ImageSource Icon { get; set; }

        public StorageFile File { get; set; }

        public string DisplayName { get; set; }

        public string FileType { get; set; }

        public string DateCreated { get; set; }

        public ICommand DeleteCommand { get; set; }
        public ICommand OpenCommand { get; set; }

        public MediaFileDetail(StorageFile file, ImageSource icon, ICommand deleteCommand, ICommand openCommand)
        {
            DeleteCommand = deleteCommand;
            OpenCommand = openCommand;

            File = file;
            DisplayName = file.DisplayName;
            DateCreated = file.DateCreated.ToString("dd.MM.yyyy HH:mm");
            FileType = file.FileType.Trim('.');

            Icon = icon;
        }

        private string GetLogoPath(string fileType)
        {
            return fileType switch
            {
                "mp4" => "/Assets/Icons/mp4.png",
                "mp3" => "/Assets/Icons/mp3.png",
                _ => "/Assets/Icons/dat.png",
            };
        }
    }
}
