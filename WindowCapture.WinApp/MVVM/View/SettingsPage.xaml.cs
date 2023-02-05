using Microsoft.UI.Xaml.Controls;
using WindowCapture.WinApp.MVVM.ViewModel;

namespace WindowCapture.WinApp.MVVM.View
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; set; }

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = ViewModel = App.GetService<SettingsViewModel>();
        }
    }
}
