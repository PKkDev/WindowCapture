using Microsoft.UI.Xaml.Media;
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
    }
}
