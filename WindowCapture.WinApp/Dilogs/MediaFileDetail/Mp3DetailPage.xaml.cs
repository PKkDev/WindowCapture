using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WindowCapture.WinApp.Dilogs.MediaFileDetail
{
    public sealed partial class Mp3DetailPage : Page
    {
        private MVVM.Model.MediaFileDetail _fileDetail { get; set; }

        public Mp3DetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MVVM.Model.MediaFileDetail fileDetail)
            {
                _fileDetail = fileDetail;
                LoadDetail();
            }
            base.OnNavigatedTo(e);
        }

        private void LoadDetail()
        {

        }
    }
}
