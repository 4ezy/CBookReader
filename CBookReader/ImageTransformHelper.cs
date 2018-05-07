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
        public static TransformedBitmap StretchToWidth(BitmapSource image,
            double width, out double scaleX, out double scaleY)
        {
            scaleX = width / image.Width;
            scaleY = width / image.Width;

            return ImageTransformHelper.Scale(image, scaleX, scaleY);
        }

        public static TransformedBitmap StretchToHeight(BitmapSource image,
            double height, out double scaleX, out double scaleY)
        {
            scaleX = height / image.Height;
            scaleY = height / image.Height;

            return ImageTransformHelper.Scale(image, scaleX, scaleY);
        }

        public static TransformedBitmap Stretch(BitmapSource image,
            double width, double height, out double scaleX, out double scaleY)
        {
            scaleX = width / image.Width;
            scaleY = height / image.Height;

            return ImageTransformHelper.Scale(image, scaleX, scaleY);
        }

        public static TransformedBitmap Scale(BitmapSource image, double scaleX, double scaleY)
        {
            TransformedBitmap transformedBitmap = new TransformedBitmap();
            transformedBitmap.BeginInit();
            transformedBitmap.Source = image;
            transformedBitmap.Transform = new ScaleTransform(scaleX, scaleY);
            transformedBitmap.EndInit();
            return transformedBitmap;
        }
    }
}
