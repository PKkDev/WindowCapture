using CaptureHelper.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WindowCapture.WinApp.MVVM.View;
using WindowCapture.WinApp.MVVM.ViewModel;
using WindowCapture.WinApp.Service;
using Windows.Foundation.Collections;

namespace WindowCapture.WinApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window MainWindow { get; set; }
        public IHost Host { get; }

        public static CaptureItemSelected CaptureItemSelected { get; set; }

        private UIElement? _shell { get; set; }

        public static T GetService<T>() where T : class
        {
            if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");

            return service;
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            //LaunchSplachTask();

            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) =>
            {
            };

            Host = Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureServices((context, services) =>
            {
                // Views and ViewModels
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<ShellViewModel>();
                services.AddTransient<ShellPage>();
                services.AddTransient<CapturePage>();

                services.AddTransient<MediaFolderPage>();
                services.AddTransient<MediaFolderViewModel>();

                services.AddSingleton<NavigationHelperService>();

            })
            .Build();

            UnhandledException += App_UnhandledException;


            ToastNotificationManagerCompat.OnActivated += (ToastNotificationActivatedEventArgsCompat toastArgs) =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // Obtain any user input (text boxes, menu selections) from the notification
                ValueSet userInput = toastArgs.UserInput;

                // Need to dispatch to UI thread if performing UI operations
                MainWindow.DispatcherQueue.TryEnqueue(delegate
                {
                    // TODO: Show the corresponding content
                    //MessageBox.Show("Toast activated. Args: " + toastArgs.Argument);
                });
            };
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();

            _shell = App.GetService<ShellPage>();
            MainWindow.Content = _shell ?? new Frame();

            MainWindow.Activate();

            //var w = new CapureItemSelectorWindow();
            //w.Activate();

            SetWindowSize(MainWindow);
        }

        private void SetWindowSize(Window mainWindow)
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 600, Height = 700 });

            OverlappedPresenter overlappedPresenter = appWindow.Presenter as OverlappedPresenter;
            overlappedPresenter.IsResizable = false;
        }

        public SplashScreenHelper.SplashScreen m_sc;

        private async void LaunchSplachTask()
        {
            m_sc = new SplashScreenHelper.SplashScreen();
            m_sc.Initialize();
            IntPtr hBitmap = await m_sc.GetBitmap(@"Assets\AppIcon\SplashScreen.scale-400.png");
            m_sc.DisplaySplash(IntPtr.Zero, hBitmap, null);
            // m_sc.DisplaySplash(IntPtr.Zero, IntPtr.Zero, @"Assets\XboxSplashScreen.mp4");
            // m_sc.DisplaySplash(IntPtr.Zero, IntPtr.Zero, @"Assets\Firework_black_background_640x400.mp4");
        }
    }
}
