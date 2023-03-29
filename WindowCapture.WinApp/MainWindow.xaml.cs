using Microsoft.UI.Xaml;
using System;

namespace WindowCapture.WinApp
{
    public sealed partial class MainWindow : Window
    {
        private IntPtr hWnd;
        private Microsoft.UI.Windowing.AppWindow App;
        Microsoft.UI.WindowId windowId;

        public int Test { get; set; } = 50;

        public MainWindow()
        {
            InitializeComponent();

            hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            App = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd));

            //App.TitleBar.ExtendsContentIntoTitleBar = true;
            //App.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            //App.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            //App.TitleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(50, 255, 255, 255);
            //App.TitleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(90, 255, 255, 255);

            //MakeTransparent();

            //Maximize();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Maximize() => Win32.ShowWindow(hWnd, 3);

        private void Normal() => Win32.ShowWindow(hWnd, 1);

    }
}
