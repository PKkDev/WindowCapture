using Windows.Storage;

namespace WindowCapture.WinApp.MVVM.Model
{
    public class MediaFileDetail
    {
        public string LogoPath { get; set; }

        public StorageFile File { get; set; }

        public string DisplayName { get; set; }

        public string FileType { get; set; }

        public string DateCreated { get; set; }

        public MediaFileDetail(StorageFile file)
        {
            File = file;
            DisplayName = file.DisplayName;
            DateCreated = file.DateCreated.ToString("dd.MM.yyyy HH:mm");
            FileType = file.FileType.Trim('.');
            LogoPath = GetLogoPath(FileType);
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
