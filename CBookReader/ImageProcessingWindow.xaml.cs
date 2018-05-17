using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CBookReader
{
    /// <summary>
    /// Логика взаимодействия для ImageProcessingWindow.xaml
    /// </summary>
    public partial class ImageProcessingWindow : Window
    {
        public Image ParentImage { get; set; }

        public ImageProcessingWindow(Image parentImage)
        {
            InitializeComponent();
            this.ParentImage = parentImage;
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BitmapSource source = this.ParentImage.Source as BitmapSource;
            // изменять яркость
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BitmapSource source = this.ParentImage.Source as BitmapSource;
            ImageProcessingHelper.ChangeContrast(source, e.NewValue);
        }
    }
}
