using System.Collections.Generic;
using System.Drawing;

namespace CaptureHelper.Extensions
{
    public static class BitmapExtensions
    {
        public static byte[] GetRGBAVector(this Bitmap btmToPrse)
        {
            List<byte> listT = new();
            for (int y = 0; y < btmToPrse.Height; y++)
            {
                for (int x = 0; x < btmToPrse.Width; x++)
                {
                    Color clr = btmToPrse.GetPixel(x, y);
                    listT.Add(clr.R);
                    listT.Add(clr.G);
                    listT.Add(clr.B);
                    listT.Add(clr.A);
                }
            }
            return listT.ToArray();
        }
    }
}
