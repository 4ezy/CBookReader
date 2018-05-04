using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CBookReader
{
    internal static class ImageTransformHelper
    {
        public static TransformedBitmap StretchToWidth(BitmapSource image, double width)
        {
            return ImageTransformHelper.Scale(image,
                width / 1.4 / image.PixelWidth,
                width / 1.4 / image.PixelWidth);
        }

        public static TransformedBitmap StretchToHeight(BitmapSource image, double height)
        {
            return ImageTransformHelper.Scale(image,
                height / 1.55 / image.PixelHeight,
                height / 1.55 / image.PixelHeight);
        }

        public static TransformedBitmap Scale(BitmapSource image, double scaleX, double scaleY)
        {
            //int stride = (int)image.PixelWidth * (image.Format.BitsPerPixel / 8);
            //byte[] pixels = new byte[image.PixelWidth * stride];
            //image.CopyPixels(pixels, stride, 0);

            TransformedBitmap transformedBitmap = new TransformedBitmap();
            transformedBitmap.BeginInit();
            transformedBitmap.Source = image;//BitmapSource.Create(
            //    image.PixelWidth, image.PixelHeight, image.DpiX, image.DpiY,
            //    image.Format, image.Palette, pixels, stride);
            transformedBitmap.Transform = new ScaleTransform(scaleX, scaleY);
            transformedBitmap.EndInit();
            return transformedBitmap;
        }
    }
}
