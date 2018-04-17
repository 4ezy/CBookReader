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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CBookReader
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;

            if (rectangle == backRect)
            {
                backRect.Opacity = 0.2;
                backPoly.Visibility = Visibility.Visible;
            }
            else if (rectangle == nextRect)
            {
                nextRect.Opacity = 0.2;
                nextPoly.Visibility = Visibility.Visible;
            }
        }

        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;

            if (rectangle == backRect)
            {
                backRect.Opacity = 0;
                backPoly.Visibility = Visibility.Hidden;
            }
            else if (rectangle == nextRect)
            {
                nextRect.Opacity = 0;
                nextPoly.Visibility = Visibility.Hidden;
            }
        }
    }
}
