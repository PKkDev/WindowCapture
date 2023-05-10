using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;

namespace CaptureHelper.Model
{
    public class WindowInfo
    {
        public Process Process { get; set; }

        public string Title { get; set; }

        public IntPtr HWND { get; set; }

        public ImageSource IconSource { get; set; }

        public ImageSource PreviewSource { get; set; }

        public WindowInfo(Process process)
        {
            Process = process;
            Title = process.MainWindowTitle;
            HWND = process.MainWindowHandle;

            IconSource = new BitmapImage(new Uri("https://howfix.net/wp-content/uploads/2018/02/sIaRmaFSMfrw8QJIBAa8mA-article.png"));
            PreviewSource = new BitmapImage(new Uri("https://howfix.net/wp-content/uploads/2018/02/sIaRmaFSMfrw8QJIBAa8mA-article.png"));
        }
    }
}
