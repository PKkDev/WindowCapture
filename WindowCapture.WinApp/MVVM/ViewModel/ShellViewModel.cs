using CommunityToolkit.Mvvm.ComponentModel;
using WindowCapture.WinApp.Service;

namespace WindowCapture.WinApp.MVVM.ViewModel
{
    public class ShellViewModel : ObservableRecipient
    {
        public NavigationHelperService NavigationHelperService { get; private set; }

        public ShellViewModel(NavigationHelperService navigationHelperService)
        {
            NavigationHelperService = navigationHelperService;
        }
    }
}
