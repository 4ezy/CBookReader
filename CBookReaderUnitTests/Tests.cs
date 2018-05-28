using System;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CBookReader;

namespace CBookReaderUnitTests
{
    [TestClass]
    public class Tests
    {
        const double testWidth = 800;
        const double testHeight = 600;
        const double rotationAngle = 90;

        public BitmapSource Init(out double width, out double height)
        {
            BitmapSource bitmap = new BitmapImage(new Uri("pic.jpg", UriKind.Relative));
            width = bitmap.Width;
            height = bitmap.Height;
            return bitmap;
        }

        [TestMethod]
        public void StretchToWidthTest()
        {
            BitmapSource bitmap = Init(
                out double defaultWidth, out double defaultHeight);
            BitmapSource transBitmap = ImageTransformHelper.StretchToWidth(
                bitmap, testWidth, out double scX, out double scY);

            Assert.AreEqual(testWidth, transBitmap.Width);
        }

        [TestMethod]
        public void StretchToHeightTest()
        {
            BitmapSource bitmap = Init(
                out double defaultWidth, out double defaultHeight);
            BitmapSource transBitmap = ImageTransformHelper.StretchToHeight(
                bitmap, testHeight, out double scX, out double scY);

            Assert.AreEqual(testHeight, transBitmap.Height);
        }

        [TestMethod]
        public void StretchTest()
        {
            BitmapSource bitmap = Init(
                out double defaultWidth, out double defaultHeight);
            BitmapSource transBitmap = ImageTransformHelper.Stretch(
                bitmap, testWidth, testHeight, out double scX, out double scY);

            Assert.AreEqual(testWidth, transBitmap.Width);
            Assert.AreEqual(testHeight, transBitmap.Height);
        }

        [TestMethod]
        public void RotateTest()
        {
            BitmapSource bitmap = Init(
                out double defaultWidth, out double defaultHeight);
            BitmapSource rotatedBitmap = ImageTransformHelper.Rotate(bitmap,
                rotationAngle);

            Assert.AreEqual(bitmap.Width, rotatedBitmap.Height);
            Assert.AreEqual(bitmap.Height, rotatedBitmap.Width);
        }
    }
}
