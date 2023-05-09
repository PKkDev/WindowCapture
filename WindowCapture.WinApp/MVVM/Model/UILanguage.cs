namespace WindowCapture.WinApp.MVVM.Model
{
    public sealed class UILanguage
    {
        public string Title { get; set; }

        public string Code { get; set; }

        public UILanguage(string title, string code)
        {
            Title = title;
            Code = code;
        }
    }
}
