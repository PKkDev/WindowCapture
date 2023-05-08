using Windows.Devices.PointOfService;

namespace WindowCapture.WinApp.MVVM.Model
{
    public class ResolutionItem
    {
        public SizeUInt32 Size { get; set; }

        public string SizeTxt => $"{Size.Width} x {Size.Height}";

        public ResolutionItem() { }

        public ResolutionItem(SizeUInt32 size)
        {
            Size = size;
        }
    }
}
