using Microsoft.UI.Xaml.Controls;
using WindowCapture.WinApp.MVVM.ViewModel;

namespace WindowCapture.WinApp.MVVM.View
{
    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; set; }

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ViewModel = App.GetService<ShellViewModel>();

            ViewModel.NavigationHelperService.Initialize(NavView, ContentFrame);
            ViewModel.NavigationHelperService.Navigate("PictureDetect");
        }
    }
}
