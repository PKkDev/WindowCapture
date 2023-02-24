using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WindowCapture.WinApp
{
    public sealed partial class MainWindow : Window
    {
        private IntPtr hWnd;
        private AppWindow App;

        public MainWindow()
        {
            InitializeComponent();

            hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            App = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(hWnd));

            App.TitleBar.ExtendsContentIntoTitleBar = true;
            App.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            App.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            App.TitleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(50, 255, 255, 255);
            App.TitleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(90, 255, 255, 255);

            MakeTransparent();

            //Maximize();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //SetMica(false, true);
            //SetAcrylic(false, true);
            //SetBlur(false, true);

            var r3 = Win32.SetLayeredWindowAttributes(hWnd, (uint)ColorTranslator.ToWin32(Color.Red), 10, Win32.LWA_COLORKEY);

            Main.Background = new SolidColorBrush(Colors.Transparent);
        }


        private void MakeTransparent()
        {
            Win32.SubClassDelegate = new(Win32.WindowSubClass);
            var r1 = Win32.SetWindowSubclass(hWnd, Win32.SubClassDelegate, 0, 0);

            long nExStyle = Win32.GetWindowLong(hWnd, Win32.GWL_EXSTYLE);
            if ((nExStyle & Win32.WS_EX_LAYERED) == 0)
            {
                var r2 = Win32.SetWindowLong(hWnd, Win32.GWL_EXSTYLE, (IntPtr)(nExStyle | Win32.WS_EX_LAYERED));
                var r3 = Win32.SetLayeredWindowAttributes(hWnd, (uint)ColorTranslator.ToWin32(Color.Magenta), 255, Win32.LWA_COLORKEY);
            }
        }

        private void SetMica(bool Enable, bool DarkMode)
        {
            int IsMicaEnabled = Enable ? 1 : 0;
            var r1 = Win32.DwmSetWindowAttribute(hWnd, (int)Win32.DWMWINDOWATTRIBUTE.DWMWA_MICA_EFFECT, ref IsMicaEnabled, sizeof(int));

            int IsDarkEnabled = DarkMode ? 1 : 0;
            var r2 = Win32.DwmSetWindowAttribute(hWnd, (int)Win32.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref IsDarkEnabled, sizeof(int));
        }

        public void SetAcrylic(bool Enable, bool DarkMode) =>
            SetComposition(Win32.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, Enable, DarkMode);

        public void SetBlur(bool Enable, bool DarkMode) =>
            SetComposition(Win32.AccentState.ACCENT_ENABLE_BLURBEHIND, Enable, DarkMode);

        public void SetComposition(Win32.AccentState AccentState, bool Enable, bool DarkMode)
        {
            var Accent = Enable ? new Win32.AccentPolicy()
            {
                AccentState = AccentState,
                GradientColor = Convert.ToUInt32(DarkMode ? 0x990000 : 0xFFFFFF)
            } : new Win32.AccentPolicy() { AccentState = 0 };

            var StructSize = Marshal.SizeOf(Accent);
            var Ptr = Marshal.AllocHGlobal(StructSize);
            Marshal.StructureToPtr(Accent, Ptr, false);

            var data = new Win32.WindowCompositionAttributeData()
            {
                Attribute = Win32.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = StructSize,
                Data = Ptr
            };

            var r1 = Win32.SetWindowCompositionAttribute(hWnd, ref data);
            Marshal.FreeHGlobal(Ptr);
        }


        private void Maximize()
        {
            Win32.ShowWindow(hWnd, 3);
        }
        private void Normal()
        {
            Win32.ShowWindow(hWnd, 1);
        }
    }
}
