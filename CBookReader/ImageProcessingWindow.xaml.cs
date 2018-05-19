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
        private bool processingForAllPages;

        public event Action<double> BrightnessChanged;
        public event Action<double> ContrastChanged;
        public event Action ProcessingForAllPagesChanged;
        public double PreviousBrightness { get; set; }
        public double PreviousContrast { get; set; }
        public bool OkClicked { get; set; }

        public bool ProcessingForAllPages {
            get => processingForAllPages;
            set
            {
                this.processingForAllPages = value;
                this.ProcessingForAllPagesChanged?.Invoke();
            }
        }

    public ImageProcessingWindow(double previousBrightness, double previousContrast)
        {
            InitializeComponent();
            this.PreviousBrightness = previousBrightness;
            this.PreviousContrast = previousContrast;
            this.brightnessSlider.Value = previousBrightness;
            this.contrastSlider.Value = previousContrast;
            this.ProcessingForAllPages = false;
            this.OkClicked = false;
        }

        private void ContrastSlider_ValueChanged(
            object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.ContrastChanged?.Invoke(e.NewValue);
        }

        private void BrightnessSlider_ValueChanged(
            object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.BrightnessChanged?.Invoke(e.NewValue);
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            this.OkClicked = true;

            if (this.procAllChkBox.IsChecked == true)
                this.ProcessingForAllPages = true;

            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.OkClicked)
            {
                this.contrastSlider.Value = this.PreviousContrast;
                this.brightnessSlider.Value = this.PreviousBrightness;
            }
        }
    }
}
