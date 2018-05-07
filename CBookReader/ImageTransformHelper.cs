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
                width / image.Width,
                width / image.Width);
        }

        public static TransformedBitmap StretchToHeight(BitmapSource image, double height)
        {
            return ImageTransformHelper.Scale(image,
                height / image.Height,
                height / image.Height);
        }

        public static TransformedBitmap Stretch(BitmapSource image, double width, double height)
        {
            return ImageTransformHelper.Scale(image,
                width / image.Width,
                height / image.Height);
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
