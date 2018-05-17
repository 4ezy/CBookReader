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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CBookReader
{
    /// <summary>
    /// Логика взаимодействия для ImageProcessingWindow.xaml
    /// </summary>
    public partial class ImageProcessingWindow : Window
    {
        public event Action<double> BrightnessChanged;
        public event Action<double> ContrastChanged;

        public ImageProcessingWindow()
        {
            InitializeComponent();
        }

        private void ContrastSlider_ValueChanged(
            object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.ContrastChanged(e.NewValue);
        }

        private void BrightnessSlider_ValueChanged(
            object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.BrightnessChanged(e.NewValue);
        }
    }
}
