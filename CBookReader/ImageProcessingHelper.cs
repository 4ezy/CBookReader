using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CBookReader
{
    internal static class ImageProcessingHelper
    {
        public static BitmapSource ChangeContrast(BitmapSource source, double contrastCoefficient)
        {
            WriteableBitmap writable = new WriteableBitmap(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY,
                source.Format, null);
            int stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);
            byte[] buffer = new byte[source.PixelHeight * stride];
            source.CopyPixels(buffer, stride, 0);
            return writable;
        }
    }
}
