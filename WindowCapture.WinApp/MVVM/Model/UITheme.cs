using Microsoft.UI.Xaml;

namespace WindowCapture.WinApp.MVVM.Model
{
    public class UITheme
    {
        public string Title { get; set; }

        public ElementTheme Theme { get; set; }

        public UITheme(string title, ElementTheme theme)
        {
            Title = title;
            Theme = theme;
        }
    }
}
