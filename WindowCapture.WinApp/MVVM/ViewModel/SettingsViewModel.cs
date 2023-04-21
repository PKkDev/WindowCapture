using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowCapture.WinApp.Service;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources.Core;

namespace WindowCapture.WinApp.MVVM.ViewModel
{
    public class SettingsViewModel : ObservableRecipient
    {
        public NavigationHelperService NavigationHelperService { get; private set; }

        private string _versionDescription;
        public string VersionDescription { get => _versionDescription; set => SetProperty(ref _versionDescription, value); }

        private ElementTheme _elementTheme;
        public ElementTheme ElementTheme { get => _elementTheme; set => SetProperty(ref _elementTheme, value); }

        public ICommand SwitchThemeCommand { get; }

        public SettingsViewModel(NavigationHelperService navigationHelperService)
        {
            NavigationHelperService = navigationHelperService;

            _versionDescription = GetVersionDescription();

            SwitchThemeCommand = new RelayCommand<ElementTheme>(
                async (param) =>
                {
                    if (ElementTheme != param)
                    {
                        ElementTheme = param;
                        await SetThemeAsync(param);
                    }
                });
        }

        private static string GetVersionDescription()
        {
            Version version;

            if (false)
            {
                var packageVersion = Package.Current.Id.Version;

                version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            }
            else
            {
                version = Assembly.GetExecutingAssembly().GetName().Version!;
            }

            return $"{"AppDisplayName"} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        public async Task SetThemeAsync(ElementTheme theme)
        {
            if (App.MainWindow.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = theme;

                var lang = rootElement.Language;

                //TitleBarHelper.UpdateTitleBar(theme);


                if (Windows.UI.Core.CoreWindow.GetForCurrentThread() != null)
                {

                }

                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    //var res = ResourceContext.GetForCurrentView();

                    var lang = "en-US";
                    // var lang = "ru-RU";

                    ResourceContext.SetGlobalQualifierValue("Language", lang);

                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = lang;

                    // not using ResourceContext.GetForCurrentView
                    var resourceContext = new ResourceContext();
                    resourceContext.QualifierValues["Language"] = lang;
                    var resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");

                    var t = resourceMap.GetValue("IsCapturePCAudio/Content", resourceContext);
                    var tt = t.ValueAsString;

                    // Change the app language
                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = lang;

                    // Be sure to clear the Frame stack so that cached Pages are removed, otherwise they will have the old language.
                    //Frame.BackStack.Clear();
                    NavigationHelperService.ClearBackStack();

                    // Reload the page that you want to have the new language
                    //Frame.Navigate(typeof(MainPage));
                    NavigationHelperService.Navigate("Settings");

                });

            }

            await Task.CompletedTask;
        }
    }
}
