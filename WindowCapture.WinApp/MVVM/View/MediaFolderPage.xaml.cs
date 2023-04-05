using Microsoft.UI.Xaml.Controls;
using WindowCapture.WinApp.MVVM.Model;
using WindowCapture.WinApp.MVVM.ViewModel;

namespace WindowCapture.WinApp.MVVM.View
{
    public sealed partial class MediaFolderPage : Page
    {
        public MediaFolderViewModel ViewModel { get; set; }

        public MediaFolderPage()
        {
            InitializeComponent();
            DataContext = ViewModel = App.GetService<MediaFolderViewModel>();
            ViewModel.SetDetailFrame(detailFrame);
        }

        private void ViewFilesItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is MediaFileDetail fileDetail)
                ViewModel.ViewFilesClick(fileDetail);
        }
    }
}
