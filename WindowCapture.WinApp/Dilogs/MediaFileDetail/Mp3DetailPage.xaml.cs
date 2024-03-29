using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WindowCapture.WinApp.Extensios;
using Windows.Storage.FileProperties;

namespace WindowCapture.WinApp.Dilogs.MediaFileDetail
{
    public sealed partial class Mp3DetailPage : Page, INotifyPropertyChanged
    {
        private MVVM.Model.MediaFileDetail FileDetail { get; set; }

        public MusicProperties MusicProps { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public Mp3DetailPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MVVM.Model.MediaFileDetail fileDetail)
            {
                FileDetail = fileDetail;
                LoadingProgress.IsIndeterminate = true;
                LoadingProgress.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                await LoadDetail();
            }
            base.OnNavigatedTo(e);
        }

        private async Task LoadDetail()
        {
            MusicProps = await FileDetail.File.Properties.GetMusicPropertiesAsync();
            OnPropertyChanged("MusicProps");

            filePreview.Source = await FileDetail.File.GetFileIcon(HelpersWin32.FileIconHelper.IconSizeEnum.ExtraLargeIcon);

            LoadingProgress.IsIndeterminate = false;
            LoadingProgress.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }
}
