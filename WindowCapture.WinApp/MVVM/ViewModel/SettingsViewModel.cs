using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WindowCapture.WinApp.MVVM.Model;
using WindowCapture.WinApp.Service;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using Windows.UI.Popups;

namespace WindowCapture.WinApp.MVVM.ViewModel
{
    public class SettingsViewModel : ObservableRecipient
    {
        public NavigationHelperService NavigationHelperService { get; private set; }

        private string _versionDescription;
        public string VersionDescription { get => _versionDescription; set => SetProperty(ref _versionDescription, value); }

        public ObservableCollection<UILanguage> Languages { get; set; }
        private UILanguage _selectedLanguage;
        public UILanguage SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                SetLanguageAsync(value);
                SetProperty(ref _selectedLanguage, value);
            }
        }

        public ObservableCollection<UITheme> Themes { get; set; }
        private UITheme _selectedTheme;
        public UITheme SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                SetTheme(value);
                SetProperty(ref _selectedTheme, value);
            }
        }

        public RelayCommand RestartAppCmd { get; set; }
        private bool _isRestartAppCmd;
        public bool IsRestartAppCmd { get => _isRestartAppCmd; set => SetProperty(ref _isRestartAppCmd, value); }

        public SettingsViewModel(NavigationHelperService navigationHelperService)
        {
            NavigationHelperService = navigationHelperService;

            _versionDescription = GetVersionDescription();

            Languages = new()
            {
                new UILanguage("English", "en-US"),
                new UILanguage("Russian", "ru")
            };

            if (App.MainWindow.Content is FrameworkElement rootElement1)
            {
                var langCode = rootElement1.Language;
                //var langCode2 = ApplicationLanguages.PrimaryLanguageOverride;
                var search = Languages.FirstOrDefault(x => x.Code.Contains(langCode));
                if (search != null)
                    SelectedLanguage = search;
            }

            Themes = new()
            {
                new UITheme("Default", ElementTheme.Default),
                new UITheme("Dark", ElementTheme.Dark),
                new UITheme("Light", ElementTheme.Light),
            };

            if (App.MainWindow.Content is FrameworkElement rootElement2)
            {
                var theme = rootElement2.RequestedTheme;
                var search = Themes.FirstOrDefault(x => x.Theme == theme);
                if (search != null)
                    SelectedTheme = search;
            }

            RestartAppCmd = new RelayCommand(() =>
            {
                Microsoft.Windows.AppLifecycle.AppInstance.Restart("Application Restart Programmatically");
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


        public void SetTheme(UITheme theme)
        {
            if (App.MainWindow.Content is FrameworkElement rootElement)
            {
                if (rootElement.RequestedTheme == theme.Theme)
                    return;

                rootElement.RequestedTheme = theme.Theme;
            }
        }

        public void SetLanguageAsync(UILanguage language)
        {
            if (App.MainWindow.Content is FrameworkElement rootElement)
            {
                var langCode = rootElement.Language;
                //var langCode2 = ApplicationLanguages.PrimaryLanguageOverride;

                if (language.Code.Contains(langCode))
                    return;

                ResourceContext.SetGlobalQualifierValue("Language", language.Code);
                ApplicationLanguages.PrimaryLanguageOverride = language.Code;

                IsRestartAppCmd = true;
            }
        }
    }
}
